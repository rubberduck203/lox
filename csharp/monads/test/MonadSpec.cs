using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace rubberduck.monads.tests
{
    public abstract class MonadSpec : FunctorSpec
    {
        ///<summary>
        ///Must return the MonadFixture's inner value.
        ///</summary>
        protected abstract int InnerValue {get;}
        protected abstract Monad<int> MonadFixture {get;}

        [TestMethod]
        public void LinqComprehensionsWork()
        {
            var m =
                from x in MonadFixture
                from y in MonadFixture
                select x + y;
            
            Assert.AreEqual(MonadFixture.Unit(InnerValue * 2), m);
        }

        [TestMethod]
        public void SatisfiesMonadLeftIdentityLaw()
        {
            Monad<int> add1(int a) => MonadFixture.Unit(a + 1);
            var a = 1;
            // return a >>= f === f a
            Assert.AreEqual(MonadFixture.Unit(a).SelectMany(add1), add1(a));
        }

        [TestMethod]
        public void SatisfiesMonadRightIdentityLaw()
        {
            // m >>= return === m
            Assert.AreEqual(MonadFixture.SelectMany(MonadFixture.Unit), MonadFixture);
        }

        [TestMethod]
        public void SatisfiesMonadAssociativityLaw()
        {
            // (m >>= f) >>= g === m >>= (\x -> f x >>= g)
            Monad<int> f(int a) => MonadFixture.Unit(a + 1);
            Monad<int> g(int a) => MonadFixture.Unit(a * 2);

            var left =
                MonadFixture
                .SelectMany(f)
                .SelectMany(g);
            Func<int, Monad<int>> right =
                (x) => f(x).SelectMany(g);

            Assert.AreEqual(left, right(InnerValue));
        }
    }
}
