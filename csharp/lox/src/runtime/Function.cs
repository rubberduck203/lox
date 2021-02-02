using System;
using System.Collections.Generic;
using System.Linq;
using lox.monads;
using lox.ast;

namespace lox.runtime
{
    public class Function : Callable
    {
        private readonly FunctionStmt declaration;

        public Function(FunctionStmt declaration)
        {
            this.declaration = declaration;
            Arity = declaration.parameters.Count();
        }

        public int Arity { get; }

        public Result<object, RuntimeError> Call(Interpreter interpreter, IEnumerable<object> args)
        {
            var env = new Env(interpreter.Globals);
            //todo: assert args == arity
            foreach(var(k,v) in declaration.parameters.Zip(args))
            {
                env.Define(k.Lexeme,v);
            }

            return interpreter.ExecuteBlock(declaration.body, env);
        }

        public override string ToString() =>
            $"<fn {declaration.name.Lexeme}>";
    }
}