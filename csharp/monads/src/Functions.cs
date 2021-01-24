using System;

namespace rubberduck.monads
{
    ///<summary>
    /// Generic functions that operate on many things
    ///</summary>
    public static class Functions
    {
        public static A Identity<A>(A a) => a;
        public static Func<A,C> Compose<A,B,C>(Func<B,C> f, Func<A,B> g) =>
            x => f(g(x));
    }
}
