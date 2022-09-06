using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using AStarNickNS;
using System.Collections.Generic;
using System.Linq;
using FibonacciHeap;

namespace AStarTests {

    [TestClass]
    public class GenericPlaceTests {
        GenericPlace gpA = new("A", new Dictionary<IPlace, double>());
        GenericPlace gpB = new("B", new Dictionary<IPlace, double>());
        GenericPlace gpC = new("C", new Dictionary<IPlace, double>());

        [TestMethod]
        public void TestIsNeighbour() {
            // A and B are neighbours, C is disjoint
            gpA.ExplicitNeighboursWithCosts.Add(gpB, 1.0);
            gpB.ExplicitNeighboursWithCosts.Add(gpA, 1.0);

            Assert.IsTrue(gpA.IsNeighbour(gpB));
            Assert.IsTrue(gpB.IsNeighbour(gpA));
            Assert.IsFalse(gpC.IsNeighbour(gpA));
            Assert.IsFalse(gpC.IsNeighbour(gpB));
            Assert.IsFalse(gpA.IsNeighbour(gpC));
            Assert.IsFalse(gpB.IsNeighbour(gpC));

            Assert.AreEqual(0, gpC.ExplicitNeighboursWithCosts.Count);
            Assert.AreEqual(1, gpB.ExplicitNeighboursWithCosts.Count);

            // Check you can remove a neighbour in the HashSet
            gpA.ExplicitNeighboursWithCosts.Remove(gpB);
            Assert.AreEqual(0, gpA.ExplicitNeighboursWithCosts.Count);

            // Check you cannot remove a non-existent neighbour
            gpC.ExplicitNeighboursWithCosts.Remove(gpB);
            Assert.AreEqual(0, gpC.ExplicitNeighboursWithCosts.Count);
        }

        [TestMethod]
        public void TestToString() {
            // Check ToString() method returns the label
            Assert.AreEqual(gpA.Label, "A");
            Assert.AreEqual(gpA.Label, gpA.ToString());
        }
    }

    [TestClass]
    public class GridPlaceTests {
        //Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => account.Debit(debitAmount));
        GridPlace gridPlaceBase = new((-3, 5));
        GridPlace gridPlaceDiag = new((-4, 6));
        GridPlace gridPlaceStra = new((-3, 6));
        GridPlace gridPlaceDistant = new((-60, 60));
        

        [TestMethod]
        public void TestIsNeighbour() {
            // Explicit neighbours (not automatically bidirectional)
            gridPlaceDistant.ExplicitNeighboursWithCosts.Add(gridPlaceBase, 1.0);
            Assert.IsTrue(gridPlaceDistant.IsNeighbour(gridPlaceBase));
            Assert.IsFalse(gridPlaceBase.IsNeighbour(gridPlaceDistant));
            gridPlaceBase.ExplicitNeighboursWithCosts.Add(gridPlaceDistant, 1.0);
            Assert.IsTrue(gridPlaceBase.IsNeighbour(gridPlaceDistant));

            // Grid neighbours (remove the above explicit neighbours first)
            gridPlaceBase.ExplicitNeighboursWithCosts.Remove(gridPlaceDistant);
            gridPlaceDistant.ExplicitNeighboursWithCosts.Remove(gridPlaceBase);
            Assert.IsFalse(gridPlaceBase.IsNeighbour(gridPlaceDistant));

            Assert.IsTrue(gridPlaceBase.IsNeighbour(gridPlaceDiag));
            Assert.IsTrue(gridPlaceBase.IsNeighbour(gridPlaceStra));
            Assert.IsTrue(gridPlaceDiag.IsNeighbour(gridPlaceBase));
            Assert.IsTrue(gridPlaceStra.IsNeighbour(gridPlaceBase));
        }

        [TestMethod]
        public void TestDeltaFrom() {
            int[] expectedDelta = new int[2] { (-60)-(-3), 60-5 };
            Assert.IsTrue(gridPlaceDistant.DeltaFrom(gridPlaceBase).SequenceEqual(expectedDelta));
        }

        [TestMethod]
        public void TestToString() {
            Assert.AreEqual("(-3, 5)", gridPlaceBase.ToString());
        }
    }

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

    [TestClass]
    public class GenericPlaceDijkstra {

        private readonly GenericPlace A = new("A");
        private readonly GenericPlace B = new("B");
        private readonly GenericPlace C = new("C");
        private readonly GenericPlace D = new("D");
        private readonly GenericPlace E = new("E");
        private readonly GenericPlace F = new("F");
        private readonly GenericPlace G = new("G");
        private DijkstraSolver<GenericPlace> _sut;

        [TestInitialize]
        public void TestFixtureSetup() {
            A.ExplicitNeighboursWithCosts.Add(B, 2.0);
            B.ExplicitNeighboursWithCosts.Add(A, 2.0);
            B.ExplicitNeighboursWithCosts.Add(C, 5.0);
            B.ExplicitNeighboursWithCosts.Add(E, 2.0);
            C.ExplicitNeighboursWithCosts.Add(B, 5.0);
            C.ExplicitNeighboursWithCosts.Add(D, 2.0);
            D.ExplicitNeighboursWithCosts.Add(C, 2.0);
            D.ExplicitNeighboursWithCosts.Add(G, 2.0);
            E.ExplicitNeighboursWithCosts.Add(B, 2.0);
            E.ExplicitNeighboursWithCosts.Add(F, 3.0);
            F.ExplicitNeighboursWithCosts.Add(E, 3.0);
            F.ExplicitNeighboursWithCosts.Add(G, 3.0);
            G.ExplicitNeighboursWithCosts.Add(D, 2.0);
            G.ExplicitNeighboursWithCosts.Add(F, 3.0);
            _sut = new(A, G);
        }

        [TestMethod]
        public void TestFindsBestPath() {
            /*               E -3- F -3- G (END)
                *               |           |
                *               2           2
                *               |           |
                * A (START) -2- B -5- C -2- D 
                * 
                * A->B->E->F->G takes 10 (should pick this path)
                * A->B->C->D->G takes 11
                */
            string[] expectedOptimalPathLabels = new string[5] { "A", "B", "E", "F", "G" };

            _sut.Solve();
            IEnumerable<IPlace<string>> optimalPath = _sut.ReconstructPath();
            //IEnumerable<GenericPlace> optimalPath = _sut.ReconstructPath();
            List<String> optimalPathLabels = optimalPath.Select(place => place.Label).ToList();
            Assert.IsTrue(optimalPathLabels.SequenceEqual(expectedOptimalPathLabels));
        }

        [TestMethod]
        public void TestNullPathIfNoSolutionFound() {
            /*               E     F -3- G (END)
                *               |           |
                *               2           2
                *               |           |
                * A (START) -2- B -5- C     D 
                * 
                * There should be no path
                */
            C.ExplicitNeighboursWithCosts.Remove(D);
            D.ExplicitNeighboursWithCosts.Remove(C);
            E.ExplicitNeighboursWithCosts.Remove(F);
            F.ExplicitNeighboursWithCosts.Remove(E);

            _sut.Solve();
            Assert.IsNull(_sut.ReconstructPath());
        }

        [TestMethod]
        public void TestNullPathIfNotYetRun() {
            Assert.IsNull(_sut.ReconstructPath());
        }
    }
}
