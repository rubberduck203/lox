using System;

namespace rubberduck.monads
{
    public record Some<T>(T value)
    {
        public override string ToString() =>
            $"Some({value})";
    }
    public record None<T>
    {
        public override string ToString() =>
            "None";
    }

    public class Maybe<A> : Monad<A>, IEquatable<Maybe<A>>
    {
        private readonly object value;
        private Maybe()
        {
            this.value = new None<A>();
        }
        private Maybe(Some<A> value)
        {
            this.value = value;
        }
        
        public static Maybe<A> Some(A value) =>
            new Maybe<A>(new Some<A>(value));

        public static Maybe<A> None() =>
            new Maybe<A>();

        public Monad<T> Unit<T>(T value) =>
            Maybe<T>.Some(value);

        public Functor<B> Select<B>(Func<A, B> f) =>
            this.value switch {
                Some<A>(var v) => Unit(f(v)),
                None<A> => Maybe<B>.None()
            };

        public Monad<B> SelectMany<B>(Func<A, Monad<B>> f) =>
            this.value switch {
                Some<A>(var v) => f(v),
                None<A> => Maybe<B>.None()
            };

        public bool Equals(Maybe<A> other) =>
            other is null
            ? false
            : this.value.Equals(other.value);

        public override bool Equals(object obj) =>
            obj is Maybe<A> other
            ? this.Equals(other)
            : false;

        public override int GetHashCode() =>
            this.value?.GetHashCode() ?? 199 ^ 73;

        public override string ToString() =>
            this.value.ToString();
    }
}