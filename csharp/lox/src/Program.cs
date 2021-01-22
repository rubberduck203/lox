using System;
using System.IO;
using System.Linq;
using lox.tools;

namespace lox
{
    class Program
    {
        //TODO: I don't like how the author is using global static state.
        private static bool HadError = false;

        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: lox [script]");
                return 64;
            } 
            else if(args.Length == 1) 
            {
                return RunFile(args[0]);
            } 
            else
            {
                RunPrompt();
                return 0;
            }
        }

        private static int RunFile(string filePath)
        {
            var contents = File.ReadAllText(filePath);
            Run(contents);
            return HadError ? 65 : 0;
        }

        private static void RunPrompt()
        {
            for(;;)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (!(line is object)) break;
                Run(line);
                //reset this so we don't crash the interactive session
                HadError = false;
            }
        }

        private static void Run(string contents)
        {
            var scanner = new Scanner(contents);
            var tokens =
                scanner
                .ScanTokens()
                .ToList() //it's not safe to scan more than once
                .Where(r =>
                    r.IsErr() 
                    || (
                        r.Unwrap().TokenType != TokenType.WhiteSpace
                        && r.Unwrap().TokenType != TokenType.Comment
                        && r.Unwrap().TokenType != TokenType.NewLine
                    )
                );

            foreach(var token in tokens)
            {
                if (token.IsOk()) {
                    Console.WriteLine(token.Unwrap());
                } else {
                    var error = token.Error();
                    Error(error.line, error.message);
                }
            }

            if (tokens.Any(r => r.IsErr()))
                return;

            // We can just unwrap since we just checked
            var parser = new Parser(tokens.Select(r => r.Unwrap()).ToList());
            var exprResult = parser.Parse();

            if (exprResult.IsErr())
            {
                var error = exprResult.Error();
                if (error.token.TokenType == TokenType.EoF) 
                {
                    Report(error.token.Line, " at end", error.message);
                } else {
                    Report(error.token.Line, $" at '{error.token.Lexeme}'", error.message);
                }
                return;
            }

            Console.WriteLine(new AstPrinter().Print(exprResult.Unwrap()));
        }

        public static void Error(int line, String message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            HadError = true;
        }
    }
}
