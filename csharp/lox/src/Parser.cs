using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;
using lox.monads;

namespace lox
{
    using ExprResult = lox.monads.Result<Expr, ParseError>;
    using StmtResult = lox.monads.Result<Stmt, ParseError>;
    public record ParseError(Token token, string message);
    public class Parser
    {
        private readonly List<Token> tokens;
        private int current;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public List<StmtResult> Parse() 
        {
            /*
             * program -> declaration* EOF ;
             */

            //this is begging to be enumerable
            var statements = new List<StmtResult>();
            while(!IsAtEnd())
                statements.Add(Declaration());

            return statements;
        }

        private StmtResult Declaration()
        {
            /* 
             * declaration → varDecl | statement ;
             */
            var result =
                Match(TokenType.Var)
                ? VarDeclaration()
                : Statement();

            return
                result
                .MapErr(err => {
                    Synchronize();
                    return err;
                });
        }

        private StmtResult VarDeclaration() =>
            /*
             * varDecl → "var" IDENTIFIER ( "=" expression )? ";" ;
             */
            from name in Consume(TokenType.Identifier, "Expected variable name.")
            from initializer in Match(TokenType.Equal) ? Expression() : ExprResult.Ok(null)
            from _ in Consume(TokenType.Semicolon, "Expected ';' after variable declaration.")
            select new VarStmt(name, initializer) as Stmt;

        private StmtResult Statement()
        {
            /*
             * statement -> exprStmt | ifStmt | printStmt | block ; 
             */
            if (Match(TokenType.If))
                return IfStatement();
            if (Match(TokenType.Print))
                return PrintStatement();
            if (Match(TokenType.LeftBrace))
                return BlockStatement();

            return ExpressionStatement();
        }

        private StmtResult IfStatement() =>
            /*
             * ifStmt → "if" "(" expression ")" statement
             *          ( "else" statement )? ;
             */
            from lParen in Consume(TokenType.LeftParen, "Expected '(' after 'if'.")
            from condition in Expression()
            from rParent in Consume(TokenType.RightParen, "Expected ')' after condition.")
            from thenBranch in Statement()
            from elseBranch in Match(TokenType.Else) ? Statement() : StmtResult.Ok(null)
            select new IfStmt(condition, thenBranch, elseBranch) as Stmt;

        private StmtResult BlockStatement()
        {
            /* block → "{" declaration* "}" ; */
            var statements = StatementsInBlock().ToList();
            // because StatementsInBlock stops producing on error
            // it's okay to just check the last one
            return 
                from last in statements.Last()
                from token in Consume(TokenType.RightBrace, "Expected '}' after block.")
                select new BlockStmt(statements.Select(r => r.Unwrap()).ToList()) as Stmt;
        }

        private IEnumerable<StmtResult> StatementsInBlock()
        {
            var failed = false;
            while(!Check(TokenType.RightBrace) && !IsAtEnd() && !failed)
            {
                var result = Declaration();
                if (result.IsErr())
                {
                    failed = true;
                }
                yield return result;
            }
        }

        private StmtResult PrintStatement() =>
            /*
             * printStmt -> "print" expression ";" ; 
             */
            from value in Expression()
            from token in Consume(TokenType.Semicolon, "Expected ';' after value.")
            select new PrintStmt(value) as Stmt;

        private StmtResult ExpressionStatement() =>
            /* 
             * exprStmt -> expression ";" ; 
             */
            from expr in Expression()
            from token in Consume(TokenType.Semicolon, "Expected ';' after expression.")
            select new ExpressionStmt(expr) as Stmt;

        private ExprResult Expression() => Assignment();

        private ExprResult Assignment()
        {
            /*
             * assignment → IDENTIFIER "=" assignment
             *              | logic_or ;
             */
            //https://craftinginterpreters.com/statements-and-state.html#assignment
            return
                from expr in LogicOr()        //if left is a valid expression
                from expr2 in LookAhead(expr)  //look to see if it's an assignment
                select expr2;                  //if not, return expr we gave to the lookahead

            ExprResult LookAhead(Expr expr)
            {
                if(Match(TokenType.Equal))
                {
                    var eq = PreviousToken();
                    return
                        from value in Assignment() //recurse to snag the right hand expr
                        from assignment in VariableAssignment(expr, value, eq)
                        select assignment;
                }
                return ExprResult.Ok(expr); //not a variable assignment, give the plain expr back
            }

            ExprResult VariableAssignment(Expr expr, Expr value, Token equals) =>
                (expr is VariableExpr variable)
                    ? ExprResult.Ok(new AssignExpr(variable.name, value))
                    : ExprResult.Err(new ParseError(equals, "Invalid assignment target."));
        }
        
        private ExprResult LogicOr() =>
            // logic_or → logic_and ( "or" logic_and )* ;
            ParseLogicalExpr(TokenType.Or, LogicOr, LogicAnd);

        private ExprResult LogicAnd() =>
            // logic_and → equality ( "and" equality )* ;
            ParseLogicalExpr(TokenType.And, LogicAnd, Equality);

        private ExprResult ParseLogicalExpr(TokenType tokenType, Func<ExprResult> current, Func<ExprResult> next)
        {
            // current -> next ( "token" next )* ;
            ExprResult ParseRight(Expr left) {
                var @operator = PreviousToken();
                return
                    from right in current()
                    select new LogicalExpr(left, @operator, right) as Expr;
            }

            return
                from left in next()
                from right in Match(tokenType) ? ParseRight(left) : ExprResult.Ok(left)
                select right;
        }

        private ExprResult Equality()
        {
            /* 
             * equality → comparison ( ( "!=" | "==" ) comparison )* ; 
             */
            return ParseBinaryOperation(Comparison, TokenType.BangEqual, TokenType.EqualEqual);
        }

        private ExprResult Comparison()
        {
            /*
             * comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
             */
            return ParseBinaryOperation(Term, TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual);
        }

        private ExprResult Term()
        {
            /*
             * term → factor ( ( "-" | "+" ) factor )* ;
             */
            return ParseBinaryOperation(Factor, TokenType.Minus, TokenType.Plus);
        }

        private ExprResult Factor()
        {
            /*
             * factor → unary ( ( "/" | "*" ) unary )* ;
             */
            return ParseBinaryOperation(Unary, TokenType.Slash, TokenType.Star);
        }

        private ExprResult ParseBinaryOperation(Func<ExprResult> next, params TokenType[] tokenTypes)
        {
            /*
             * current -> next ( ( tokenA | tokenB | etc. ) next )* ;
             */
            return
                next()
                .Bind(expr => {
                    var result = ExprResult.Ok(expr);
                    while(Match(tokenTypes))
                    {
                        var @operator = PreviousToken();
                        result = 
                            next()
                            .Bind(right => 
                                ExprResult.Ok(new BinaryExpr(expr, @operator, right))
                            );
                    }
                    return result;
                });
        }

        private ExprResult Unary()
        {
            /*
             * unary → ( "!" | "-" ) unary
             *       | primary ;
             */
            if(Match(TokenType.Bang, TokenType.Minus))
            {
                var @operator = PreviousToken();
                return 
                    Unary()
                    .Bind(right => ExprResult.Ok(new UnaryExpr(@operator, right)));
            }
            return Primary();
        }

        private ExprResult Primary()
        {
            /*
             * primary → NUMBER | STRING | "true" | "false" | "nil"
             *         | "(" expression ")"
             *         | IDENTIFIER ;
             */
            if(Match(TokenType.False))
                return ExprResult.Ok(new LiteralExpr(false));
            if(Match(TokenType.True))
                return ExprResult.Ok(new LiteralExpr(true));
            if(Match(TokenType.Nil))
                return ExprResult.Ok(new LiteralExpr(null));
            if(Match(TokenType.Number, TokenType.String))
                return ExprResult.Ok(new LiteralExpr(PreviousToken().Literal));
            if(Match(TokenType.Identifier))
                return ExprResult.Ok(new VariableExpr(PreviousToken()));
            
            if(Match(TokenType.LeftParen))
            {
                return 
                    from expr in Expression()
                    from token in Consume(TokenType.RightParen, "Expected ')' after expression.")
                    select new GroupingExpr(expr) as Expr;
            }

            return ExprResult.Err(new ParseError(Peek(), "Expected expression."));
        }

        private Result<Token, ParseError> Consume(TokenType tokenType, String message)
        {
            if (Check(tokenType))
                return Result<Token,ParseError>.Ok(Advance());
            
            return Result<Token, ParseError>.Err(new ParseError(Peek(), message));
        }

        private void Synchronize()
        {
            //when an error occurs, try to find the next statement we can parse
            while(!IsAtEnd())
            {
                Advance();
                if(PreviousToken().TokenType == TokenType.Semicolon)
                    return;
                switch(Peek().TokenType)
                {
                    case TokenType.Class:
                    case TokenType.Fun:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }
            }
        }

        private bool Match(params TokenType[] tokenTypes)
        {
            foreach(var tokenType in tokenTypes)
            {
                if(Check(tokenType))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private Token Advance()
        {
            var token = Peek();
            this.current++;
            return token;
        }

        private bool Check(TokenType tokenType) =>
            IsAtEnd() ? false : Peek().TokenType == tokenType;

        private bool IsAtEnd() =>
            Peek().TokenType == TokenType.EoF;

        private Token Peek() =>
            this.tokens[this.current];

        private Token PreviousToken() =>
            this.tokens[this.current - 1];
    }
}