using System;

namespace lox.monads {

    public record Ok<T>(T value)
    {
        public override string ToString() =>
            $"Ok({value})";
    }
    public record Err<E>(E error)
    {
        public override string ToString() =>
            $"Err({error})";
    }

    public class Result<T,E> : IEquatable<Result<T,E>> where E: class
    {
        private readonly object inner;

        public static Result<T,E> Ok(T value) => new Result<T, E>(new Ok<T>(value));
        public static Result<T,E> Err(E error) => new Result<T, E>(new Err<E>(error));

        private Result(Ok<T> value)
        {
            this.inner = value;
        }

        private Result(Err<E> error)
        {
            this.inner = error;
        }

        public bool IsErr() => !IsOk();
        public bool IsOk() => this.inner is Ok<T>;

        public T Unwrap() =>
            this.inner switch
            {
                Ok<T>(var v) => v,
                var err => throw new InvalidOperationException($"Cannot unwrap error: {err}")
            };

        public E Error() =>
            this.inner switch
            {
                Err<E>(var e) => e,
                var ok => throw new InvalidOperationException($"Result is {ok}")
            };

        /* By construction and the type system, we know the matches do cover all cases */
        #pragma warning disable CS8509
        // M a -> (a -> M b) -> M b
        public Result<TO, E> Bind<TO>(Func<T, Result<TO, E>> func) =>
            this.inner switch
            {
                Ok<T>(var v) => func(v),
                Err<E> err => new Result<TO, E>(err),
            };
        #pragma warning restore CS8509

        private A Identity<A>(A result) => result;

        // M a e -> (e -> e') -> M a e'
        public Result<T, U> MapErr<U>(Func<E,U> func)
        where U : class =>
            MapOrElse(Identity, func);

        /* By construction and the type system, we know the matches do cover all cases */
        #pragma warning disable CS8509
        // M a e -> (a -> b) -> (e -> e') -> M b e'
        public Result<U,V> MapOrElse<U,V>(Func<T,U> okSelector, Func<E,V> errSelector)
        where V: class =>
            this.inner switch
            {
                Ok<T>(var v) => Result<U,V>.Ok(okSelector(v)),
                Err<E>(var err) => Result<U,V>.Err(errSelector(err)),
            };
        #pragma warning restore CS8509

        public bool Equals(Result<T, E> other) =>
            (this.inner, other.inner) switch
            {
                (Ok<T>,Err<E>) => false,
                (Err<E>,Ok<T>) => false,
                (Err<E>(var a),Err<E>(var b)) => a.Equals(b),
                (Ok<T>(var a), Ok<T>(var b)) => a.Equals(b),
                (_,_) => false //should never happen, but equals shoudn't ever throw
            };

        public override bool Equals(object obj) =>
            obj is Result<T,E> other
            ? this.Equals(other)
            : false;

        public override int GetHashCode() =>
            this.inner.GetHashCode();

        public override string ToString() =>
            this.inner.ToString();
    }

    public static class ResultExtensions
    {
            // F a e -> (a -> b) -> F b e
            public static Result<U, E> Map<T,E,U>(this Result<T,E> result, Func<T, U> func)
            where E: class =>
                result.Bind(v => Result<U, E>.Ok(func(v)));

            // Alias for Map that enables linq query syntax for comprehensions
            // F a e -> (a -> b) -> F b e
            public static Result<U, E> Select<T,E,U>(this Result<T,E> result, Func<T, U> selector)
            where E: class =>
                result.Map(selector);

            // (a -> M b) -> (a -> b -> c) -> M c
            public static Result<V, E> SelectMany<T, E, U, V>(
                this Result<T,E> result,
                Func<T, Result<U,E>> convert,
                Func<T,U,V> select
            ) where E: class =>
                result
                .Bind(x =>
                        convert(x)
                        .Bind(y => Result<V,E>.Ok(select(x, y)))
                );
    }
}