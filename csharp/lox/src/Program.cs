using System;
using System.IO;
using System.Linq;
using lox.runtime;

namespace lox
{
    class Program
    {
        //TODO: I don't like how the author is using global static state.
        private static bool HadError = false;
        private static bool HadRuntimeError = false;
        private static Interpreter Interpreter = new Interpreter();

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
            if (HadError) return 65;
            if (HadRuntimeError) return 70;
            return 0;
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

            var tokenResults = scanner.ScanTokens().ToList(); //it's not safe to scan more than once

            if (tokenResults.Any(r => r.IsErr()))
            {
                var errors =
                    tokenResults
                    .Where(r => r.IsErr())
                    .Select(r => r.Error());
                foreach(var error in errors)
                {
                    Error(error.line, error.message);
                }
                return;
            }

            // We can just unwrap since we just checked for errors
            var tokens =
                tokenResults
                .Select(r => r.Unwrap())
                .Where(t =>
                    t.TokenType != TokenType.WhiteSpace
                    && t.TokenType != TokenType.Comment
                    && t.TokenType != TokenType.NewLine
                );
            var parser = new Parser(tokens.ToList());
            var statementResults = parser.Parse();

            if (statementResults.Any(r => r.IsErr()))
            {
                var errors =
                    statementResults
                    .Where(r => r.IsErr())
                    .Select(r => r.Error());
                foreach(var error in errors)
                {
                    if (error.token.TokenType == TokenType.EoF) 
                    {
                        Report(error.token.Line, " at end", error.message);
                    } else {
                        Report(error.token.Line, $" at '{error.token.Lexeme}'", error.message);
                    }
                }
                return;
            }

            //we just checked for parse errors,
            // so we can simply unwrap
            var statements =
                statementResults
                .Select(r => r.Unwrap())
                .ToList();

            var runtimeErrors = Interpreter.Interpret(statements);
            foreach(var error in runtimeErrors)
            {
                Error(error);
            }
        }

        private static void Error(RuntimeError error)
        {
            Console.Error.WriteLine($"{error.message}");
            Console.Error.WriteLine($"[line {error.token.Line}]");
            HadRuntimeError = true;
        }

        private static void Error(int line, String message)
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
