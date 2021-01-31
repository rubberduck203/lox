using System.Collections.Generic;
using lox.monads;

namespace lox
{
    public interface Callable
    {
        int Arity { get; }
        Result<object,RuntimeError> Call(Interpreter interpreter, IEnumerable<object> args);
    }
}