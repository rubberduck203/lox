using System;
using lox.tools;
using lox.ast;

namespace lox.astprinter
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: Use parser to accept input and print out the AST
            var expr = new BinaryExpr(
                new UnaryExpr(
                    new Token(TokenType.Minus, "-", null, 1),
                    new LiteralExpr(123)
                ),
                new Token(TokenType.Star, "*", null, 1),
                new GroupingExpr(
                    new BinaryExpr(
                        new LiteralExpr(45.67),
                        new Token(TokenType.Plus, "+", null, 1),
                        new LiteralExpr(1)
                    )
                )
            );

           Console.WriteLine(new AstPrinter().Print(expr));
        }
    }
}
