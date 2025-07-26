using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var intsAsImmutableArray = (ImmutableArray<int>)intsAsObjectArray.ToImmutableArray(typeof(int));
            Assert.IsTrue(ints.SequenceEqual(intsAsImmutableArray.ToArray()));
        }
    }
}
