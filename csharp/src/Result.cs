using System;

namespace lox.monads {

    public record Ok<T>(T value);
    public record Err<E>(E error);

    public class Result<T,E>
    {
        public static Result<T,E> Ok(T value) => new Result<T, E>(new Ok<T>(value));
        public static Result<T,E> Err(E err) => new Result<T, E>(new Err<E>(err));

        public object Inner { get; }
        private Result(Ok<T> value)
        {
            Inner = value;
        }

        private Result(Err<E> error)
        {
            Inner = error;
        }

        public bool IsOk() => Inner is Ok<T>;
        public bool IsErr() => !IsOk();

        public Result<TO, E> Bind<TO>(Func<T, Result<TO, E>> func) => 
            Inner switch
            {
                Ok<T> ok => func(ok.value),
                Err<E> err => new Result<TO, E>(err),
            };
    }
}