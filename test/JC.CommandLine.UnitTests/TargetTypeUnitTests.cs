using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class TargetTypeUnitTests
    {
        [TestMethod]
        public void String_is_scalar_type()
        {
            var testee = new TargetType(typeof(string));
            Assert.IsFalse(testee.IsNullable);
            Assert.IsFalse(testee.IsVectorType);
            Assert.AreEqual(typeof(string), testee.ScalarType);
            Assert.AreEqual(typeof(string), testee.Target);
        }

        [TestMethod]
        public void DateTime_is_scalar_type()
        {
            var testee = new TargetType(typeof(DateTime[]));
            Assert.IsFalse(testee.IsNullable);
            Assert.IsTrue(testee.IsVectorType);
            Assert.AreEqual(typeof(DateTime), testee.ScalarType);
            Assert.AreEqual(typeof(DateTime[]), testee.Target);
        }

        [TestMethod]
        public void Int_is_scalar_type()
        {
            var testee = new TargetType(typeof(int));
            Assert.IsFalse(testee.IsNullable);
            Assert.IsFalse(testee.IsVectorType);
            Assert.AreEqual(typeof(int), testee.ScalarType);
            Assert.AreEqual(typeof(int), testee.Target);
        }

        [TestMethod]
        public void List_of_DateTime_is_vector_type()
        {
            var type = typeof(List<DateTime>);
            var testee = new TargetType(type);
            Assert.IsFalse(testee.IsNullable);
            Assert.IsTrue(testee.IsVectorType);
            Assert.AreEqual(typeof(DateTime), testee.ScalarType);
            Assert.AreEqual(type, testee.Target);
        }

        [TestMethod]
        public void Enumerable_of_Nullable_DateTime_is_vector_type()
        {
            var type = typeof(IEnumerable<Nullable<DateTime>>);
            var testee = new TargetType(type);
            Assert.IsFalse(testee.IsNullable);
            Assert.IsTrue(testee.IsVectorType);
            Assert.AreEqual(typeof(DateTime), testee.ScalarType);
            Assert.AreEqual(type, testee.Target);
        }

        [TestMethod]
        public void Enumerable_is_vector_type()
        {
            var type = typeof(IEnumerable);
            var testee = new TargetType(type);
            Assert.IsFalse(testee.IsNullable);
            Assert.IsTrue(testee.IsVectorType);
            Assert.AreEqual(typeof(object), testee.ScalarType);
            Assert.AreEqual(type, testee.Target);
        }

        [TestMethod]
        public void Enumeration_is_scalar_type()
        {
            var type = typeof(StringComparison);
            var testee = new TargetType(type);
            Assert.IsFalse(testee.IsNullable);
            Assert.IsFalse(testee.IsVectorType);
            Assert.AreEqual(type, testee.ScalarType);
            Assert.AreEqual(type, testee.Target);
        }

        [TestMethod]
        public void Nullable_int_is_scalar_type()
        {
            var type = typeof(int?);
            var testee = new TargetType(type);
            Assert.IsTrue(testee.IsNullable);
            Assert.IsFalse(testee.IsVectorType);
            Assert.AreEqual(typeof(int), testee.ScalarType);
            Assert.AreEqual(type, testee.Target);
        }

        [TestMethod]
        public void XmlDocument_is_scalar_type()
        {
            var testee = new TargetType(typeof(XmlDocument));
            Assert.IsFalse(testee.IsVectorType);
        }
    }
}
