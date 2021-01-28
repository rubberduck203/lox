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
            select new VarStmt(name, initializer) as Stmt;

        private StmtResult Statement()
        {
            /*
             * statement -> exprStmt | printStmt ; 
             */
            if (Match(TokenType.Print))
                return PrintStatement();

            return ExpressionStatement();
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

        private ExprResult Expression() => Equality();
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