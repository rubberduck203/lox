using System;
using System.Linq;
using System.Collections.Generic;

using lox.ast;
using lox.monads;

namespace lox.runtime
{
    using Result = Result<object,RuntimeError>;
    public class Env
    {
        private readonly Dictionary<string,object> values = new();
        private readonly Env enclosing;

        public Env()
        {
            this.enclosing = null;
        }

        public Env(Env enclosing)
        {
            this.enclosing = enclosing;
        }

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

            if (enclosing is not null)
                return enclosing.Assign(name, value);

            return Result.Err(new RuntimeError(name, $"Undefined variable '{name.Lexeme}'."));
        }

        public Result AssignAt(int distance, Token name, object value)
        {
            // in theory, we don't have to check, we know it's here.
            var ancestor = Ancestor(distance);
            if (ancestor.values.ContainsKey(name.Lexeme))
            {
                ancestor.values[name.Lexeme] = value;
                return Result.Ok((object)null);
            }
            return Result.Err(new RuntimeError(name, $"Undefined variable '{name.Lexeme}'."));
        }

        public Result Lookup(Token name)
        {
            if (values.TryGetValue(name.Lexeme, out var value))
                return Result.Ok(value);

            if (enclosing is not null)
                return enclosing.Lookup(name);

            return Result.Err(new RuntimeError(name, $"Undefined variable '{name.Lexeme}'."));
        }

        public void Print()
        {
            foreach(var (k,v) in this.values)
            {
                Console.WriteLine($"{k: v}");
            }
            if (enclosing is not null)
            {
                Console.WriteLine("Enclosing environment:");
                enclosing.Print();
            }
        }

        public Result LookupAt(int distance, Token name)
        {
            // in theory, we don't have to check, we know it's here.
            if (Ancestor(distance).values.TryGetValue(name.Lexeme, out var value))
            {
                return Result.Ok(value);
            }
            return Result.Err(new RuntimeError(name, $"Undefined variable {name.Lexeme}."));
        }

        private Env Ancestor(int distance)
        {
            var env = this;
            for(int i = 0; i < distance; i++)
            {
                env = env.enclosing;
            }
            return env;
        }
    }
}