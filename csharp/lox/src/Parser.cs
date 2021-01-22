using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;

namespace lox
{
    public class Parser
    {
        private readonly List<Token> tokens;
        private int current;

        Parser(List<Token> tokens)
        {
            this.tokens = tokens;
        }

        private Expr Expression() => Equality();
        private Expr Equality()
        {
            /* 
             * equality → comparison ( ( "!=" | "==" ) comparison )* ; 
             */
            var expr = Comparison();
            while(Match(TokenType.BangEqual, TokenType.EqualEqual))
            {
                var @operator = Previous();
                var right = Comparison();
                expr = new BinaryExpr(expr, @operator, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            /*
             * comparison → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
             */

            var expr = Term();
            while(Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                var @operator = Previous();
                var right = Term();
                expr = new BinaryExpr(expr, @operator, right);
            }
            return expr;
        }

        private Expr Term()
        {
            /*
             * term → factor ( ( "-" | "+" ) factor )* ;
             */
            var expr = Factor();
            while(Match(TokenType.Minus, TokenType.Plus))
            {
                var @operator = Previous();
                var right = Factor();
                expr = new BinaryExpr(expr, @operator, right);
            }
            return expr;
        }

        private Expr Factor()
        {
            /*
             * factor → unary ( ( "/" | "*" ) unary )* ;
             */
            var expr = Unary();
            while(Match(TokenType.Slash, TokenType.Star))
            {
                var @operator = Previous();
                var right = Unary();
                expr = new BinaryExpr(expr, @operator, right);
            }
            return expr;
        }

        private Expr Unary()
        {
            /*
             * unary → ( "!" | "-" ) unary
             *       | primary ;
             */
            if(Match(TokenType.Bang, TokenType.Minus))
            {
                var @operator = Previous();
                var right = Unary();
                return new UnaryExpr(@operator, right);
            }
            return Primary();
        }

        private Expr Primary()
        {
            /*
             * primary → NUMBER | STRING | "true" | "false" | "nil"
             *         | "(" expression ")" ;
             */
            if(Match(TokenType.False))
                return new LiteralExpr(false);
            if(Match(TokenType.True))
                return new LiteralExpr(true);
            if(Match(TokenType.Nil))
                return new LiteralExpr(null);
            if(Match(TokenType.Number, TokenType.String))
                return new LiteralExpr(Previous().Literal);
            if(Match(TokenType.LeftParen))
            {
                var expr = Expression();
                Consume(TokenType.RightParen, "Expect ')' after expression.");
                return new GroupingExpr(expr);
            }

            throw new ParseError("Unexpectedly failed to parse.");
        }

        private Token Consume(TokenType tokenType, String message)
        {
            if (Check(tokenType))
                return Advance();
            
            throw new ParseError(Peek(), message);
        }

        private void Synchronize()
        {
            //when an error occurs, try to find the next statement we can parse
            do
            {
                Advance();
                if(Previous().TokenType == TokenType.Semicolon)
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

        private Token Previous() =>
            this.tokens[this.current - 1];
    }
}