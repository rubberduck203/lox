using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;
using lox.monads;

namespace lox
{
    using Result = lox.monads.Result<Expr, ParseError>;
    public record ParseError(Token token, string message);
    public class Parser
    {
        private readonly List<Token> tokens;
        private int current;

        public Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        public Result Parse() => Expression();

        private Result Expression() => Equality();
        private Result Equality()
        {
            /* 
             * equality → comparison ( ( "!=" | "==" ) comparison )* ; 
             */
            return ParseBinaryOperation(Comparison, TokenType.BangEqual, TokenType.EqualEqual);
        }

        private Result Comparison()
        {
            /*
             * comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
             */
            return ParseBinaryOperation(Term, TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual);
        }

        private Result Term()
        {
            /*
             * term → factor ( ( "-" | "+" ) factor )* ;
             */
            return ParseBinaryOperation(Factor, TokenType.Minus, TokenType.Plus);
        }

        private Result Factor()
        {
            /*
             * factor → unary ( ( "/" | "*" ) unary )* ;
             */
            return ParseBinaryOperation(Unary, TokenType.Slash, TokenType.Star);
        }

        private Result ParseBinaryOperation(Func<Result> next, params TokenType[] tokenTypes)
        {
            /*
             * current -> next ( ( tokenA | tokenB | etc. ) next )* ;
             */
            return
                next()
                .Bind(expr => {
                    var result = Result.Ok(expr);
                    while(Match(tokenTypes))
                    {
                        var @operator = PreviousToken();
                        result = 
                            next()
                            .Bind(right => 
                                Result.Ok(new BinaryExpr(expr, @operator, right))
                            );
                    }
                    return result;
                });
        }

        private Result Unary()
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
                    .Bind(right => Result.Ok(new UnaryExpr(@operator, right)));
            }
            return Primary();
        }

        private Result Primary()
        {
            /*
             * primary → NUMBER | STRING | "true" | "false" | "nil"
             *         | "(" expression ")" ;
             */
            if(Match(TokenType.False))
                return Result.Ok(new LiteralExpr(false));
            if(Match(TokenType.True))
                return Result.Ok(new LiteralExpr(true));
            if(Match(TokenType.Nil))
                return Result.Ok(new LiteralExpr(null));
            if(Match(TokenType.Number, TokenType.String))
                return Result.Ok(new LiteralExpr(PreviousToken().Literal));
            if(Match(TokenType.LeftParen))
            {
                Expression()
                .Bind(expr => {
                    //consume returns a different result type
                    return
                        Consume(TokenType.RightParen, "Expected ')' after expression.")
                        .Bind(_token => Result.Ok(new GroupingExpr(expr)));
                });
            }

            return Result.Err(new ParseError(Peek(), "Expected expression."));
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
            do
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
            } while(!IsAtEnd());
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