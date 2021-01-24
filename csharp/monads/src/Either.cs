using System;

namespace rubberduck.monads
{
    public record Right<T>(T value)
    {
        public override string ToString() =>
            $"Right({value})";
    }
    public record Left<T>(T value)
    {
        public override string ToString() =>
            $"Left({value})";
    }

    public class Either<L, R> : IEquatable<Either<L,R>>, Monad<L> //todo, implement Monad<R>
    {
        public static Either<L,R> Left(L left) => new Either<L, R>(new Left<L>(left));
        public static Either<L,R> Right(R right) => new Either<L,R>(new Right<R>(right));

        private readonly object value;

        private Either(Left<L> value)
        {
            this.value = value;
        }

        private Either(Right<R> value)
        {
            this.value = value;
        }

        public Functor<B> Select<B>(Func<L, B> func) =>
            this.value switch {
                Left<L>(var v) => Either<B,R>.Left(func(v)),
                Right<R>(var v) => Either<B,R>.Right(v)
            };

        public Monad<B> SelectMany<B>(Func<L, Monad<B>> f) =>
            this.value switch {
                Left<L>(var v) => f(v),
                Right<R>(var v) => Either<B,R>.Right(v)
            };

        public Monad<T> Unit<T>(T value) =>
            new Either<T,R>(new Left<T>(value));

        public bool Equals(Either<L, R> other) =>
            (this.value,other.value) switch {
                (Left<L>(var v),Left<L>(var o)) => v.Equals(o),
                (Right<R>(var v),Right<R>(var o)) => v.Equals(o),
                (_,_) => false
            };

        public override bool Equals(object obj) =>
            obj is Either<L,R> other
            ? this.Equals(other)
            : false;

        public override int GetHashCode() =>
            this.value.GetHashCode();

        public override string ToString() =>
            this.value.ToString();
    }
}