using System;
using System.Runtime.Serialization;

namespace lox
{
    [Serializable]
    internal class ParseError : Exception
    {
        public Token Token {get;}

        public ParseError(string message )
            : base(message)
        { }
            

        public ParseError(Token token, string message)
            :base(message)
        {
            Token = token;
        }
    }
}