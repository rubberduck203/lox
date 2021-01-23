using System;

namespace lox.monads {
    public class Result<T,E> : IEquatable<Result<T,E>> where E: class
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

        //TODO: I really need a MapError or something
        
        public bool Equals(Result<T, E> other)
        {
            if (this.IsOk() && other.IsErr())
                return false;
            if (this.IsErr() && other.IsOk())
                return false;
            if (this.IsErr() && other.IsErr())
                return this.error.Equals(other.error);
            //if (this.IsOk() && other.IsOk())
            return this.value.Equals(other.value);
        }

        public override bool Equals(object obj)
        {
            if (obj is Result<T,E> other)
                return this.Equals(other);

            return false;
        }

        public override int GetHashCode()
        {
            if (IsOk())
                return value.GetHashCode();
            return error.GetHashCode();
        }

        public override string ToString()
        {
            if (IsOk())
                return $"Ok({value})";
            return $"Err({error})";
        }
    }
}