using Microsoft.VisualStudio.TestTools.UnitTesting;

using static rubberduck.monads.Functions;

namespace rubberduck.monads.tests
{
    public abstract class FunctorSpec
    {
        protected abstract Functor<int> FunctorFixture {get;}

        [TestMethod]
        public void SatisfiesFunctorIdentityLaw()
        {
            Assert.AreEqual(Identity(FunctorFixture), FunctorFixture.Select(Identity));
        }

        [TestMethod]
        public void SatisfiesFunctorCompositionLaw()
        {
            int add1(int x) => x + 1;
            int mult2(int x) => x * 2;

            Assert.AreEqual(
                FunctorFixture.Select(x => add1(mult2(x))),
                FunctorFixture.Select(mult2).Select(add1)
            );
        }
    }
}
