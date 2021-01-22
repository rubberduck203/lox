using System;
using System.IO;
using System.Linq;

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
                scanner.ScanTokens()
                .Where(r => r.IsErr() || r.Unwrap().TokenType != TokenType.WhiteSpace);

            foreach(var token in tokens)
            {
                if (token.IsOk()) {
                    Console.WriteLine(token.Unwrap());
                } else {
                    var error = token.Error();
                    Error(error.line, error.message);
                }
            }

            //TODO: catch parse errors and report them
            // if (ex.token == TokenType.Eof) {
            //     Report(ex.token.line, "at end", message)
            // } else {
            //     Report(ex.token.line, $" at '{token.lexeme}'", message);
            // }
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
