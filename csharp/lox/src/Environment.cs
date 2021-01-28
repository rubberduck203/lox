using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;
using lox.monads;

namespace lox
{
    using LookupResult = Result<object,RuntimeError>;
    public class Env
    {
        private readonly Dictionary<string,object> values = new();

        public void Define(string name, object value)
        {
            //silent redefinition is a design choice in the book
            //probably not one I'd make myself
            // use values.Add(name, value) for a version that would not allow redefinition
            values[name] = value;
        }

        public LookupResult Lookup(Token name)
        {
            if (values.TryGetValue(name.Lexeme, out var value))
                return LookupResult.Ok(value);

            return LookupResult.Err(new RuntimeError(name, $"Undefined variable '{name.Lexeme}'."));
        }
    }
}