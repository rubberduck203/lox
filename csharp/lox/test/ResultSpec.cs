using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using lox.monads;

namespace lox.tests
{
    using IntResult = lox.monads.Result<int, string>;
    using DoubleResult = lox.monads.Result<double, string>;

    [TestClass]
    public class ResultSpec
    {
        [TestMethod]
        public void BindMapsFromOneResultTypeToAnother()
        {
            var result = IntResult.Ok(21);
            var actual =
                result
                .Bind(r => DoubleResult.Ok(r + 1.0));
            Assert.AreEqual(22.0, actual.Unwrap());
        }

        [TestMethod]
        public void BindRailroadsErrors()
        {
            var result = IntResult.Ok(21);
            var actual =
                result
                .Bind(r => IntResult.Err("boom"))
                .Bind(r => IntResult.Ok(r + 1));
            Assert.AreEqual("boom", actual.Error());
        }

        [TestMethod]
        public void SatisfiesLeftidentityLaw()
        {
            IntResult add1(int a) => IntResult.Ok(a + 1);
            var a = 1;
            // return a >>= f === f a
            Assert.AreEqual(IntResult.Ok(a).Bind(add1), add1(a));
        }

        [TestMethod]
        public void SatisfiesRightIdentityLaw()
        {
            var m = IntResult.Ok(2);
            // m >>= return === m
            Assert.AreEqual(m.Bind(IntResult.Ok), m);
        }

        [TestMethod]
        public void SatisfiesAssociativityLaw()
        {
            // (m >>= f) >>= g === m >>= (\x -> f x >>= g)
            var m = IntResult.Ok(1);
            IntResult f(int a) => IntResult.Ok(a + 1);
            IntResult g(int a) => IntResult.Ok(a * 2);

            var left =
                m.Bind(f).Bind(g);
            Func<int, IntResult> right =
                (x) => f(x).Bind(g);

            Assert.AreEqual(left, right(1));
        }

        [TestMethod]
        public void EqualsItself()
        {
            var result = IntResult.Ok(22);
            Assert.AreEqual(result, result);
        }

        [TestMethod]
        public void EqualsAnotherOk()
        {
            var x = IntResult.Ok(22);
            var y = IntResult.Ok(22);
            Assert.AreEqual(x,y);
        }

        public void OkDoesNotEqualDifferentOk()
        {
            var x = IntResult.Ok(22);
            var y = IntResult.Ok(1);
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void ErrorEqualsError()
        {
            var x = IntResult.Err("boom");
            var y = IntResult.Err("boom");
            Assert.AreEqual(x,y);
        }

        [TestMethod]
        public void ErrorDoesNotEqualDifferentError()
        {
            var x = IntResult.Err("foo");
            var y = IntResult.Err("bar");
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void ErrorDoesNotEqualOk()
        {
            var x = IntResult.Err("bar");
            var y = IntResult.Ok(1);
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void OkDoesNotEqualError()
        {
            var x = IntResult.Ok(1);
            var y = IntResult.Err("bar");
            Assert.AreNotEqual(x,y);
        }

        [TestMethod]
        public void DoesNotEqualNull()
        {
            Assert.AreNotEqual(IntResult.Ok(1), null);
        }

        [TestMethod]
        public void EqualsIsCommunitive()
        {
            var x = IntResult.Ok(22);
            var y = IntResult.Ok(22);

            Assert.AreEqual(x.Equals(y), y.Equals(x));
        }

        [TestMethod]
        public void EqualsIsAssociative()
        {
            var x = IntResult.Ok(22);
            var y = IntResult.Ok(22);
            var z = IntResult.Ok(22);

            var left = x.Equals(y) && y.Equals(z);
            Assert.AreEqual(left, x.Equals(z));
        }
    }
}
