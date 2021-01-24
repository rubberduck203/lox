using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using rubberduck.monads;

namespace rubberduck.monads.tests
{
    [TestClass]
    public class EitherSpec : MonadSpec
    {
        protected override int InnerValue => 32;

        protected override Monad<int> MonadFixture => Either<int,string>.Left(InnerValue);

        protected override Functor<int> FunctorFixture => MonadFixture;

#region Equals
        [TestMethod]
        public void LeftEqualsItself()
        {
            var result = Either<int,string>.Left(22);
            Assert.AreEqual(result, result);
        }

        [TestMethod]
        public void RightEqualsIteself()
        {
            var result = Either<int,string>.Right("foo");
            Assert.AreEqual(result, result);
        }

        [TestMethod]
        public void EqualsAnotherLeft()
        {
            var x = Either<int,string>.Left(22);
            var y = Either<int,string>.Left(22);
            Assert.AreEqual(x,y);
        }

        [TestMethod]
        public void EqualsAnotherRight()
        {
            var x = Either<int,string>.Right("foo");
            var y = Either<int,string>.Right("foo");
            Assert.AreEqual(x,y);
        }

        [TestMethod]
        public void LeftDoesNotEqualDifferentLeft()
        {
            var x = Either<int,string>.Left(22);
            var y = Either<int,string>.Left(1);
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void LeftDoesNotEqualRight()
        {
            var x = Either<int,string>.Left(22);
            var y = Either<int,string>.Right("foo");
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void RightDoesNotEqualLeft()
        {
            var x = Either<int,string>.Right("foo");
            var y = Either<int,string>.Left(22);
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void LeftDoesNotEqualNull()
        {
            Assert.AreNotEqual(Either<int,string>.Left(22), null);
        }

        [TestMethod]
        public void RightDoesNotEqualNull()
        {
            Assert.AreNotEqual(Either<int,string>.Right("foo"), null);
        }

        [TestMethod]
        public void EqualsIsCommunitive()
        {
            var x = Either<int,string>.Left(22);
            var y = Either<int,string>.Left(22);

            Assert.AreEqual(x.Equals(y), y.Equals(x));
        }

        [TestMethod]
        public void EqualsIsAssociative()
        {
            var x = Either<int,string>.Left(22);
            var y = Either<int,string>.Left(22);
            var z = Either<int,string>.Left(22);

            var left = x.Equals(y) && y.Equals(z);
            Assert.AreEqual(left, x.Equals(z));
        }

        [TestMethod]
        public void HashcodesAreEqualWhenInstancesAreSameLeft()
        {
            var x = Either<int,string>.Left(22);
            var y = Either<int,string>.Left(22);
            Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
        }

        [TestMethod]
        public void HashcodesAreEqualWhenInstancesAreSameRight()
        {
            var x = Either<int,string>.Right("foo");
            var y = Either<int,string>.Right("foo");
            Assert.AreEqual(x.GetHashCode(), y.GetHashCode());
        }
#endregion Equals

#region ToString

        [TestMethod]
        public void RightToString()
        {
            Assert.AreEqual("Right(foo)", Either<int,string>.Right("foo").ToString());
        }

        [TestMethod]
        public void SomeToString()
        {
            Assert.AreEqual("Left(23)", Either<int,string>.Left(23).ToString());
        }

#endregion ToString

    }
}