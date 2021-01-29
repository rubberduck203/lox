using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;
using lox.monads;

namespace lox
{
    using Result = Result<object,RuntimeError>;
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

        public Result Assign(Token name, object value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
                return Result.Ok((object)null);
            }
            return Result.Err(new RuntimeError(name, $"Undefined variable '{name.Lexeme}'."));
        }

        public Result Lookup(Token name)
        {
            if (values.TryGetValue(name.Lexeme, out var value))
                return Result.Ok(value);

            return Result.Err(new RuntimeError(name, $"Undefined variable '{name.Lexeme}'."));
        }
    }
}