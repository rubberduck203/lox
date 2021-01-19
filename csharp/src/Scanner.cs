using System;
using System.Collections.Generic;
namespace lox
{
    using Result = lox.monads.Result<Token, LexError>;

    public record LexError(int line, string message);

    public class Scanner
    {
        private readonly string source;

        private readonly Dictionary<String, TokenType> keywords = new() {
            {"and", TokenType.And},
            {"class", TokenType.Class},
            {"else", TokenType.Else},
            {"false", TokenType.False},
            {"for", TokenType.For},
            {"if", TokenType.If},
            {"nil", TokenType.Nil},
            {"or", TokenType.Or},
            {"print", TokenType.Print},
            {"return", TokenType.Return},
            {"super", TokenType.Super},
            {"this", TokenType.This},
            {"true", TokenType.True},
            {"var", TokenType.Var},
            {"while", TokenType.While}
        };

        //TODO: Work on a stream instead of the whole string
        public Scanner(string source)
        {
            this.source = source;
        }

        private int start = 0;
        private int current = 0;
        private int line = 1;

        public IEnumerable<Token> ScanTokens() {
            //TODO: this can probably be expressed as a foreach or linq expression
            while(!IsAtEnd())
            {
                start = current;
                var tokenResult = ScanToken();

                if (tokenResult.IsOk()) {
                    yield return tokenResult.Unwrap();
                } else {
                    //TODO: return the maybe token and handle printing elsewhere
                    var error = tokenResult.Error();
                    Program.Error(error.line, error.message);
                }
            }

            yield return new Token(TokenType.EoF, String.Empty, null, line);
        }

        private bool IsAtEnd() => 
            this.current >= this.source.Length;

        private Result ScanToken()
        {
            var c = Advance();
            return c switch
            {
                //todo: return the tokentype and call AddToken once
                /* single char lexemes */
                '(' => Token(TokenType.LeftParen),
                ')' => Token(TokenType.RightParen),
                '{' => Token(TokenType.LeftBrace),
                '}' => Token(TokenType.RightBrace),
                ',' => Token(TokenType.Comma),
                '.' => Token(TokenType.Dot),
                '-' => Token(TokenType.Minus),
                '+' => Token(TokenType.Plus),
                ';' => Token(TokenType.Semicolon),
                '*' => Token(TokenType.Star),
                /* two char lexemes */
                '!' => Token(Match('=') ? TokenType.BangEqual : TokenType.Bang),
                '=' => Token(Match('=') ? TokenType.EqualEqual : TokenType.Equal),
                '<' => Token(Match('=') ? TokenType.LessEqual : TokenType.Less),
                '>' => Token(Match('=') ? TokenType.GreaterEqual : TokenType.Greater),
                '/' => ScanDivisionOrComment(),
                /* whitespace */
                '\n' => ScanNewLine(),
                char cur when Char.IsWhiteSpace(cur) => Token(TokenType.WhiteSpace),
                /* strings, keywords, identifiers */
                '"' => ScanString(),
                char cur when Char.IsDigit(cur) => ScanNumber(),
                char cur when Char.IsLetter(cur) => ScanIdentifier(),
                /* error condition */
                _ => Result.Err(new LexError(this.line, "Unexpected character."))
            };
        }

        //consumes next character
        private char Advance()
        {
            var c = this.source[this.current];
            this.current++;
            return c;
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

        private char PeekNext()
        {
            // IsAtEnd using the next index instead of current one
            if (this.current + 1 >= source.Length) return '\0';
            return this.source[current + 1];
        }

        private Result ScanString()
        {
            while(Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n')
                {
                    line++;
                }
                Advance();
            }

            if(IsAtEnd()) 
            {
                return Result.Err(new LexError(line, "Unterminated string."));
            }
            // consume the closing double quote.
            Advance();

            // slightly less efficient but more clear than directly indexing them out
            var value = CurrentLexeme().Trim('"');
            return Token(TokenType.String, value);
        }

        private Result ScanNumber()
        {
            while(Char.IsDigit(Peek())) 
            {
                Advance();
            }

            if(Peek() == '.' && Char.IsDigit(PeekNext()))
            {
                //consume the dot
                Advance();
            }

            while(Char.IsDigit(Peek()))
            {
                Advance();
            }

            return Token(TokenType.Number, Double.Parse(CurrentLexeme()));
        }

        private Result ScanIdentifier()
        {
            bool IsAlphaNumericOrUnderScore(char c) =>
                Char.IsLetterOrDigit(c) || c == '_';

            while (IsAlphaNumericOrUnderScore(Peek()))
            {
                Advance();
            }

            if (keywords.TryGetValue(CurrentLexeme(), out var tokenType))
            {
                return Token(tokenType);
            }

            return Token(TokenType.Identifier);
        }

        private Result ScanDivisionOrComment()
        {
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
                return Token(TokenType.Comment);
            }
            else
            {
                return Token(TokenType.Slash);
            }
        }

        private Result ScanNewLine()
        {
            this.line++;
            return Token(TokenType.NewLine);
        }

        private Result Token(TokenType tokenType) =>
            Token(tokenType, null);

        private Result Token(TokenType tokenType, object literal) =>
            Result.Ok(new Token(tokenType, CurrentLexeme(), literal, this.line));

        /// Returns the current lexeme
        /// Note: Keep in mind that at any given point in time, the lexeme may be incomplete
        /// It's your job to call this at the right time.
        private string CurrentLexeme()
        {
            var length = this.current - this.start;
            return this.source.Substring(this.start, length);
        }
    }
}