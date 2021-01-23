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
        public void IsAFunctor()
        {
            var a = IntResult.Ok(2);
            var b = IntResult.Ok(3);

            var actual = a.Map((a) => a + 3);
            Assert.AreEqual(IntResult.Ok(5), actual);
        }

        [TestMethod]
        public void SatisfiesFunctorIdentityLaw()
        {
            int identity(int a) => a;
            var result = IntResult.Ok(23);
            Assert.AreEqual(result, result.Map(identity));
        }

        [TestMethod]
        public void SatisfiesFunctorCompositionLaw()
        {
            int add1(int a) => a;
            int mult2(int a) => a;

            var result = IntResult.Ok(3);
            Assert.AreEqual(
                result.Map(x => add1(mult2(x))),
                result.Map(add1).Map(mult2)
            );
        }

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
        public void SatisfiesMonadLeftidentityLaw()
        {
            IntResult add1(int a) => IntResult.Ok(a + 1);
            var a = 1;
            // return a >>= f === f a
            Assert.AreEqual(IntResult.Ok(a).Bind(add1), add1(a));
        }

        [TestMethod]
        public void SatisfiesMonadRightIdentityLaw()
        {
            var m = IntResult.Ok(2);
            // m >>= return === m
            Assert.AreEqual(m.Bind(IntResult.Ok), m);
        }

        [TestMethod]
        public void SatisfiesMonadAssociativityLaw()
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

        record SomeError(string msg);
        [TestMethod]
        public void MapErrMapsToANewErrorType()
        {
            var expected =
                Result<int,SomeError>.Err(new SomeError("boom"));

            var actual =
                IntResult.Err("boom")
                .MapErr(e => new SomeError(e));
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MapErrPassesThroughOks()
        {
            var actual =
                IntResult.Ok(2)
                .MapErr(e => new SomeError(e));

            Assert.AreEqual(Result<int, SomeError>.Ok(2), actual);
        }

        [TestMethod]
        public void MapOrElseMapsOks()
        {
            var expected =
                Result<int, SomeError>.Ok(4);

            var actual =
                IntResult.Ok(2)
                .MapOrElse(
                    x => x * 2,
                    e => new SomeError(e)
                );

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MapOrElseMapsErrors()
        {
            var expected =
                Result<int,SomeError>.Err(new SomeError("boom"));

            var actual =
                IntResult.Err("boom")
                .MapOrElse(
                    x => x * 2,
                    e => new SomeError(e)
                );

            Assert.AreEqual(expected, actual);
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
