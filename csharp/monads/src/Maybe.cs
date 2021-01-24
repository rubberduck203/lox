using System;

namespace rubberduck.monads
{
    public class Maybe<A> : Monad<A>, IEquatable<Maybe<A>>
    {
        private readonly A value;
        private Maybe(){}
        private Maybe(A value)
        {
            this.value = value;
        }
        
        public static Maybe<A> Some(A value) =>
            new Maybe<A>(value);

        public static Maybe<A> None() =>
            new Maybe<A>();

        public Monad<T> Unit<T>(T value) =>
            Maybe<T>.Some(value);

        public Functor<B> Select<B>(Func<A, B> f) =>
            Unit(f(value));

        public Monad<B> SelectMany<B>(Func<A, Monad<B>> f) =>
            value is null ? Maybe<B>.None() : f(value);

        public bool Equals(Maybe<A> other) =>
            other is null
            ? false
            : this.value.Equals(other.value);

        public override bool Equals(object obj) =>
            obj is Maybe<A> other
            ? this.Equals(other)
            : false;

        public override int GetHashCode() =>
            value?.GetHashCode() ?? 199 ^ 73;

        public override string ToString() =>
            value is null
            ? "None"
            : $"Some({value})";
    }
}