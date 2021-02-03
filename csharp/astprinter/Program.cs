using System;
using System.IO;
using System.Linq;

using lox.ast;
using lox.tools;

namespace lox.astprinter
{
    enum ExitCode : int {
        Success = 0,
        LexError = 1,
        ParseError = 2,
    }

    class Program
    {
        static int Main(string[] args)
        {
            var filePath = args[0];
            var contents = File.ReadAllText(filePath);
            var lexer = new parsing.Scanner(contents);
            var tokenResults = lexer.ScanTokens().ToList();
            var lexErrors = tokenResults.Where(r => r.IsErr());
            if (lexErrors.Any())
            {
                Console.Error.WriteLine($"Failed to lex {filePath}.");
                foreach(var error in lexErrors.Select(r => r.Error()))
                {
                    Console.Error.WriteLine($"[line {error.line}]: {error.message}");
                }
                return (int)ExitCode.LexError;
            }

            var tokens =
                tokenResults
                .Select(r => r.Unwrap())
                .Where(t =>
                    t.TokenType != TokenType.WhiteSpace
                    && t.TokenType != TokenType.Comment
                    && t.TokenType != TokenType.NewLine
                )
                .ToList();
            var parser = new parsing.Parser(tokens);
            var parseResults = parser.Parse();

            var parseErrors = parseResults.Where(r => r.IsErr());
            var parseFailed = false;
            if (parseErrors.Any())
            {
                parseFailed = true;
                Console.Error.WriteLine($"Failed to parse {filePath}.");
                foreach(var error in parseErrors.Select(r => r.Error()))
                {
                    Console.Error.WriteLine($"[token {error.token}]: {error.message}");
                }
            }

            var statements = parseResults.Where(r => r.IsOk()).Select(r => r.Unwrap()).ToList();
            Console.WriteLine(new AstPrinter().Print(statements));

            return parseFailed ? (int)ExitCode.ParseError : (int)ExitCode.Success;
        }
    }
}
