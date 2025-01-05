using System;
using System.Collections.Generic;
using System.Linq;
using FibonacciHeap;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AStarTests {
    [TestClass]
    public class PriorityQueueFibonacciHeapTests {
        private readonly FibonacciHeap<string, int> _heap = new(0);

        [TestInitialize]
        public void TestFixtureSetup() {
            _heap.Insert(new FibonacciHeapNode<string, int>("E", 5));
            _heap.Insert(new FibonacciHeapNode<string, int>("C", 3));
            _heap.Insert(new FibonacciHeapNode<string, int>("Z", 26));
            _heap.Insert(new FibonacciHeapNode<string, int>("D", 4));
            _heap.Insert(new FibonacciHeapNode<string, int>("B", 2));
        }

        [TestMethod]
        public void TestNodeExaminationAndRemoval() {
            // Examination
            Assert.AreEqual(_heap.Size(), 5);
            Assert.AreEqual(_heap.Min().Data, "B");

            // Examination + removal
            Assert.AreEqual(_heap.RemoveMin().Data, "B");
            Assert.AreEqual(_heap.Size(), 4);

            // Empty heap behaviour
            for (int i = 0; i < 4; i++) {
                _heap.RemoveMin();
            }
            Assert.AreEqual(_heap.Size(), 0);
            Assert.IsTrue(_heap.IsEmpty());
            Assert.IsNull(_heap.Min());
            Assert.IsNull(_heap.RemoveMin());
        }

        [TestMethod]
        public void TestExpectedDequeueOrder() {
            string[] expectedDataOrder = new string[5] { "B", "C", "D", "E", "Z" };

            List<string> nodeDataInOrderRetrieved = new List<string>();
            while (!_heap.IsEmpty()) {
                nodeDataInOrderRetrieved.Add(_heap.RemoveMin().Data);
            }
            Assert.IsTrue(nodeDataInOrderRetrieved.SequenceEqual(expectedDataOrder));
        }
    }
}
