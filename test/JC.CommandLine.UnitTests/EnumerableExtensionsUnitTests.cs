using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace JC.CommandLine.UnitTests
{
    [TestClass]
    public class EnumerableExtensionsUnitTests
    {
        [TestMethod]
        public void ToArray_converts_array_of_ints()
        {
            var ints = new int[] { 1, 2, 4, 8 };
            var intsAsObjectArray = ints.Cast<object>();
            var output = intsAsObjectArray.ToArray(typeof(int));
            Assert.IsTrue(output is int[]);
            var outputAsInts = (int[])output;
            Assert.IsTrue(ints.SequenceEqual(outputAsInts));
        }

        [TestMethod]
        public void ToList_converts_array_of_ints()
        {
            var ints = new int[] { 1, 2, 4, 8 };
            var intsAsObjectArray = ints.Cast<object>().ToArray();
            var output = intsAsObjectArray.ToList(typeof(int));
            Assert.IsTrue(output.GetType() == typeof(List<int>));
            var outputAsList = (IList)output;
            for (int i = 0; i < ints.Length; i++)
            {
                Assert.AreEqual(ints[i], outputAsList[i]);
            }
        }

        [TestMethod]
        public void ToImmutableArray_converts_array_of_ints()
        {
            var ints = new int[] { 1, 2, 4, 8 };
            var intsAsObjectArray = ints.Cast<object>().ToArray();
            var intsAsImmutableArray = (ImmutableArray<int>)ints.ToImmutableArray();
            Assert.IsTrue(ints.SequenceEqual(intsAsImmutableArray.ToArray()));
        }

        [TestMethod]
        public void StartsWith_SequenceA_starts_with_sequenceB()
        {
            var sequenceA = new[] { 1, 2, 3, 4, 5 };
            var sequenceB = new[] { 1, 2, 3 };
            var result = sequenceA.StartsWith(sequenceB);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void StartsWith_SequenceA_does_not_start_with_sequenceB()
        {
            var sequenceA = new[] { 1, 2, 3 };
            var sequenceB = new[] { 1, 2, 3, 4, 5 };
            var result = sequenceA.StartsWith(sequenceB);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void StartsWith_Sequences_completely_different()
        {
            var sequenceA = new[] { 1, 2, 3 };
            var sequenceB = new[] { 4, 5, 6 };
            var result = sequenceA.StartsWith(sequenceB);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void StartsWith_Compares_strings_case_insensitively()
        {
            var sequenceA = new[] { "a", "b", "c" };
            var sequenceB = new[] { "A", "B" };
            var result = sequenceA.StartsWith(sequenceB,
                (a, b) => string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void StartsWith_Sequences_equal()
        {
            var sequenceA = new[] { 1, 2, 3 };
            var sequenceB = new[] { 1, 2, 3 };
            var result = sequenceA.StartsWith(sequenceB);
            Assert.IsTrue(result);
        }
    }
}
