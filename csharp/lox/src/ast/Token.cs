namespace lox.ast {
    public record Token
    {
        public TokenType TokenType {get;}
        public string Lexeme {get;}
        public object Literal {get;}
        public int Line {get;}

        public Token(TokenType tokenType, string lexeme, object literal, int line) =>
            (TokenType, Lexeme, Literal, Line) = (tokenType, lexeme, literal, line);
        
    }
}