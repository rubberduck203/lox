﻿using System;
using System.IO;
using System.Collections.Generic;
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
            var tokens = scanner.ScanTokens();

            foreach(var token in tokens)
            {
                Console.WriteLine(token);
            }
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

    public class Scanner
    {
        private readonly string source;
        private readonly List<Token> tokens = new List<Token>();

        public Scanner(string source)
        {
            this.source = source;
        }

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public List<Token> ScanTokens() {
            //TODO: this can probably be expressed as a foreach
            // or linq expression
            while(!IsAtEnd())
            {
                start = current;
                ScanToken();
            }

            this.tokens.Add(new Token(TokenType.EoF, String.Empty, null, line));
            return tokens;
        }

        private bool IsAtEnd() => 
            this.current >= this.source.Length;
        private void ScanToken()
        {
            var c = Advance();
            switch (c) 
            {
                //todo: return the tokentype and call AddToken once
                /* single char lexemes */
                case '(': AddToken(TokenType.LeftParen); break;
                case ')': AddToken(TokenType.RightParen); break;
                case '{': AddToken(TokenType.LeftBrace); break;
                case '}': AddToken(TokenType.RightBrace); break;
                case ',': AddToken(TokenType.Comma); break;
                case '.': AddToken(TokenType.Dot); break;
                case '-': AddToken(TokenType.Minus); break;
                case '+': AddToken(TokenType.Plus); break;
                case ';': AddToken(TokenType.Semicolon); break;
                case '*': AddToken(TokenType.Star); break;
                /* two char lexemes */
                case '!': AddToken(Match('=') ? TokenType.BangEqual : TokenType.Bang);
                    break;
                case '=': AddToken(Match('=') ? TokenType.EqualEqual : TokenType.Equal);
                    break;
                case '<': AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                    break;
                case '>': AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                    break;
                case '/':
                    // a slash could indicate division or the beginning of a comment
                    if(Match('/'))
                    {
                        //comments go to the end of the line
                        //a better version would store the comment as metadata to assist in automated refactrings, etc.
                        //TODO: Environment.Newline is better, but is often multiple characters
                        while(Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        AddToken(TokenType.Slash);
                    }
                    break;
                /* whitespace */
                case ' ':
                case '\r':
                case '\t':
                    /* ignore whitespace */
                    break;
                case '\n':
                    this.line++;
                    break;
                default:
                    //TODO: this is totally gross, we should return a Result
                    Program.Error(line, "Unexpected character.");
                    break;
            }
        }

        //consumes next character
        private char Advance()
        {
            this.current++;
            return this.source[this.current - 1];
        }

        //consumes next character if it matches expected
        private bool Match(char expected)
        {
            // /* alternative impl in terms of peek and advance */
            // if (Peek() == expected)
            // {
            //     Advance();
            //     return true;
            // }

            // return false;

            if (IsAtEnd()) return false;
            if (this.source[this.current] != expected) return false;

            this.current++;
            return true;
        }

        // returns current character without consuming it
        private char Peek()
        {
            if (IsAtEnd()) return '\0';
            return this.source[current];
        }

        private void AddToken(TokenType tokenType) =>
            AddToken(tokenType, null);

        private void AddToken(TokenType tokenType, object literal)
        {
            var text = this.source.Substring(start, current - start);
            this.tokens.Add(new Token(tokenType, text, literal, this.line));
        }

    }
}
