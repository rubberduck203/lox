using System;

namespace rubberduck.monads
{
    ///<Summary>
    /// A Functor
    /// Map is named Select for Linq compatibility
    ///<remarks>
    /// Implementers must adhere to the Functor laws
    /// Identity:
    ///   map Id === Id
    /// Composition:
    ///   map (f . g) === (map f) . (map g)
    ///<remarks>
    public interface Functor<A>
    {
        ///<Summary>
        ///(a -> b) -> f a -> f b
        ///</Summary>
        Functor<B> Select<B>(Func<A,B> func);
    }
}
