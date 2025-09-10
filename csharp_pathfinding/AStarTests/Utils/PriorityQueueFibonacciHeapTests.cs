using System;
using System.Collections.Generic;
using System.Linq;
using FibonacciHeap;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AStarTests {
    [TestClass]
    public class PriorityQueueFibonacciHeapTests {
        private readonly FibonacciHeap<string, int> heap = new(0);

        [TestInitialize]
        public void TestFixtureSetup() {
            heap.Insert(new FibonacciHeapNode<string, int>("E", 5));
            heap.Insert(new FibonacciHeapNode<string, int>("C", 3));
            heap.Insert(new FibonacciHeapNode<string, int>("Z", 26));
            heap.Insert(new FibonacciHeapNode<string, int>("D", 4));
            heap.Insert(new FibonacciHeapNode<string, int>("B", 2));
        }

        [TestMethod]
        public void TestNodeExaminationAndRemoval() {
            // Examination
            Assert.AreEqual(heap.Size(), 5);
            Assert.AreEqual(heap.Min().Data, "B");

            // Examination + removal
            Assert.AreEqual(heap.RemoveMin().Data, "B");
            Assert.AreEqual(heap.Size(), 4);

            // Empty heap behaviour
            for (int i = 0; i < 4; i++) {
                heap.RemoveMin();
            }
            Assert.AreEqual(heap.Size(), 0);
            Assert.IsTrue(heap.IsEmpty());
            Assert.IsNull(heap.Min());
            Assert.IsNull(heap.RemoveMin());
        }

        [TestMethod]
        public void TestExpectedDequeueOrder() {
            string[] expectedDataOrder = { "B", "C", "D", "E", "Z" };

            List<string> nodeDataInOrderRetrieved = new List<string>();
            while (!heap.IsEmpty()) {
                nodeDataInOrderRetrieved.Add(heap.RemoveMin().Data);
            }
            Assert.IsTrue(nodeDataInOrderRetrieved.SequenceEqual(expectedDataOrder));
        }
    }
}
