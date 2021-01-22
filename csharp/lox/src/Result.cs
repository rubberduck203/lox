using System;

namespace lox.monads {
    public class Result<T,E> where E: class
    {
        private readonly T value;
        private readonly E error;

        public static Result<T,E> Ok(T value) => new Result<T, E>(value);
        public static Result<T,E> Err(E error) => new Result<T, E>(error);

        private Result(T value)
        {
            this.value = value;
        }

        private Result(E error)
        {
            this.error = error;
        }

        public bool IsErr() => this.error != null;
        public bool IsOk() => !IsErr();

        public T Unwrap()
        {
            if (IsErr())
                throw new InvalidOperationException($"Cannot unwrap error: {this.error}");
            return this.value;
        }

        public E Error()
        {
            if (IsOk())
                throw new InvalidOperationException($"Result is Ok({this.value}");
            return this.error;
        }

        public Result<TO, E> Bind<TO>(Func<T, Result<TO, E>> func) =>
            IsOk() ? func(value) : new Result<TO, E>(this.error);
        
    }
}