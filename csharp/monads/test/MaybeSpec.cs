using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using rubberduck.monads;

namespace rubberduck.monads.tests
{
    [TestClass]
    public class MaybeSpec : MonadSpec
    {
        protected override Monad<int> MonadFixture { get;} = Maybe<int>.Some(12);
        protected override Functor<int> FunctorFixture {get => MonadFixture;}

#region Equals
        [TestMethod]
        public void SomeEqualsItself()
        {
            var result = Maybe<int>.Some(22);
            Assert.AreEqual(result, result);
        }

        [TestMethod]
        public void NoneEqualsIteself()
        {
            var result = Maybe<int>.None();
            Assert.AreEqual(result, result);
        }

        [TestMethod]
        public void EqualsAnotherSome()
        {
            var x = Maybe<int>.Some(22);
            var y = Maybe<int>.Some(22);
            Assert.AreEqual(x,y);
        }

        [TestMethod]
        public void EqualsAnotherNone()
        {
            var x = Maybe<int>.None();
            var y = Maybe<int>.None();
            Assert.AreEqual(x,y);
        }

        [TestMethod]
        public void SomeDoesNotEqualDifferentSome()
        {
            var x = Maybe<int>.Some(22);
            var y = Maybe<int>.Some(1);
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void SomeDoesNotEqualNone()
        {
            var x = Maybe<int>.Some(22);
            var y = Maybe<int>.None();
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void NoneDoesNotEqualSome()
        {
            var x = Maybe<int>.None();
            var y = Maybe<int>.Some(22);
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void SomeDoesNotEqualNull()
        {
            Assert.AreNotEqual(Maybe<int>.Some(1), null);
        }

        [TestMethod]
        public void NoneDoesNotEqualNull()
        {
            Assert.AreNotEqual(Maybe<int>.None(), null);
        }

        [TestMethod]
        public void EqualsIsCommunitive()
        {
            var x = Maybe<int>.Some(22);
            var y = Maybe<int>.Some(22);

            Assert.AreEqual(x.Equals(y), y.Equals(x));
        }

        [TestMethod]
        public void EqualsIsAssociative()
        {
            var x = Maybe<int>.Some(22);
            var y = Maybe<int>.Some(22);
            var z = Maybe<int>.Some(22);

            var left = x.Equals(y) && y.Equals(z);
            Assert.AreEqual(left, x.Equals(z));
        }

        [TestMethod]
        public void HashcodesAreEqualWhenInstancesAreSameSome()
        {
            var x = Maybe<int>.Some(22);
            var y = Maybe<int>.Some(22);
            Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
        }

        [TestMethod]
        public void HashcodesAreEqualWhenInstancesAreNone()
        {
            var x = Maybe<int>.None();
            var y = Maybe<int>.None();
            Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
        }
#endregion Equals

#region ToString

        [TestMethod]
        public void NoneToString()
        {
            Assert.AreEqual("None", Maybe<int>.None().ToString());
        }

        [TestMethod]
        public void SomeToString()
        {
            Assert.AreEqual("Some(23)", Maybe<int>.Some(23).ToString());
        }

#endregion ToString
    }
}