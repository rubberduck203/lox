using System;

namespace rubberduck.monads
{
    ///<Summary>
    /// A simple monad
    /// We use the alternative name Unit to avoid the keyword return
    /// and SelectMany instead of Bind for Linq compatibility
    ///<remarks>
    /// Implementers must adhere to the 3 laws
    /// Left Identity:
    ///   unit a >>= f === f a
    /// Right Identity:
    ///   m >>= unit === m
    /// Associativity
    ///   (m >>= f) >>= g === m >>= (x => f x >>= g)
    public interface Monad<A> : Functor<A>
    {
        // a -> M a
        Monad<T> Unit<T>(T value);
        // M a -> (a -> M b) -> M b
        Monad<B> SelectMany<B>(Func<A,Monad<B>> f);
    }

    public static class MonadExt
    {
        // (a -> M b) -> (a -> b -> c) -> M c
        public static Monad<C> SelectMany<A, B, C>(this Monad<A> monad, Func<A, Monad<B>> convertor, Func<A, B, C> selector) =>
            monad.SelectMany(x =>
                convertor(x)
                .SelectMany(y => monad.Unit(selector(x,y)))
            );
    }
}
