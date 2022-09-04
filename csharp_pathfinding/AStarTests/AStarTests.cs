using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using AStarNickNS;
using System.Collections.Generic;
using System.Linq;

namespace AStarTests {

    [TestClass]
    public class GenericPlaceTests {
        GenericPlace gpA = new GenericPlace("A", new Dictionary<IPlace, double>());
        GenericPlace gpB = new GenericPlace("B", new Dictionary<IPlace, double>());
        GenericPlace gpC = new GenericPlace("C", new Dictionary<IPlace, double>());

        [TestMethod]
        public void TestGenericPlace_IsNeighbour() {
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
        public void TestGenericPlace_ToString() {
            // Check ToString() method returns the label
            Assert.AreEqual(gpA.Label, "A");
            Assert.AreEqual(gpA.Label, gpA.ToString());
        }
    }

    [TestClass]
    public class GridCoords2DTests {
        //Assert.ThrowsException<System.ArgumentOutOfRangeException>(() => account.Debit(debitAmount));
        GridCoords2D gridCoords2DBase = new GridCoords2D((-3, 5));
        GridCoords2D gridCoords2DDiag = new GridCoords2D((-4, 6));
        GridCoords2D gridCoords2DStra = new GridCoords2D((-3, 6));
        GridCoords2D gridCoords2DDistant = new GridCoords2D((-60, 60));
        GenericPlace genericPlace = new GenericPlace("GenericPlace");
        

        [TestMethod]
        public void TestGridCoords2D_IsNeighbour() {
            // Explicit neighbours with any IPlace (not automatically bidirectional)
            gridCoords2DDistant.ExplicitNeighboursWithCosts.Add(genericPlace, 1.0);
            Assert.IsTrue(gridCoords2DDistant.IsNeighbour(genericPlace));
            Assert.IsFalse(genericPlace.IsNeighbour(gridCoords2DDistant));
            genericPlace.ExplicitNeighboursWithCosts.Add(gridCoords2DDistant, 1.0);
            Assert.IsTrue(genericPlace.IsNeighbour(gridCoords2DDistant));

            // Grid neighbours
            Assert.IsFalse(gridCoords2DBase.IsNeighbour(gridCoords2DDistant));

            Assert.IsTrue(gridCoords2DBase.IsNeighbour(gridCoords2DDiag));
            Assert.IsTrue(gridCoords2DBase.IsNeighbour(gridCoords2DStra));
            Assert.IsTrue(gridCoords2DDiag.IsNeighbour(gridCoords2DBase));
            Assert.IsTrue(gridCoords2DStra.IsNeighbour(gridCoords2DBase));
        }

        [TestMethod]
        public void TestGridCoords2D_DeltaFrom() {
            int[] expectedDelta = new int[2] { (-60)-(-3), 60-5 };
            Assert.IsTrue(gridCoords2DDistant.DeltaFrom(gridCoords2DBase).SequenceEqual(expectedDelta));
        }

        [TestMethod]
        public void TestGridCoords2D_ToString() {
            Assert.AreEqual("(-3, 5)", gridCoords2DBase.ToString());
        }
    }

    //[TestClass]
    //public class NodeTests {
    //    static GenericPlace placeA = new GenericPlace("A", new HashSet<IPlace<string>> { });
    //    static GenericPlace placeB = new GenericPlace("B", new HashSet<IPlace<string>> { });
    //    static GridCoords2D placeC = new GridCoords2D(-3, 2);

    //    Node<GenericPlace> nodeA = new Node<GenericPlace>(placeA);
    //    Node<GenericPlace> nodeB = new Node<GenericPlace>(placeB);
    //    Node<GridCoords2D> nodeC = new Node<GridCoords2D>(placeC);

    //    public void SetupNodes() {
    //        Dictionary<Node<GenericPlace>, double> costDictA = new Dictionary<Node<GenericPlace>, double> { { nodeB, 4.0 } };
    //        foreach (Node<GenericPlace> node in costDictA.Keys) {
    //            nodeA.Coord.Neighbours.Add(node.Coord);
    //        }

    //        Dictionary<Node<GenericPlace>, double> costDictB = new Dictionary<Node<GenericPlace>, double> { { nodeA, 4.0 } };
    //        foreach (Node<GenericPlace> node in costDictB.Keys) {
    //            nodeB.Coord.Neighbours.Add(node.Coord);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestNode_GetLabel() {
    //        SetupNodes();
    //        Console.WriteLine(gpA.Neighbours);
    //        int a = 2;
    //        //Assert.IsTrue(gridCoords2DBase.GetLabel().SequenceEqual(new int[2] { -3, 5 }));
    //    }

    //}

    // test Node<GenericPlace> ? 
    //[TestClass]
    //public class NodeTests {
    //    static GenericPlace placeA = new GenericPlace("A", new HashSet<IPlace<string>> { });
    //    static GenericPlace placeB = new GenericPlace("B", new HashSet<IPlace<string>> { });
    //    static GridCoords2D placeC = new GridCoords2D(-3, 2);

    //    Node<GenericPlace> nodeA = new Node<GenericPlace>(placeA);
    //    Node<GenericPlace> nodeB = new Node<GenericPlace>(placeB);
    //    Node<GridCoords2D> nodeC = new Node<GridCoords2D>(placeC);

    //    public void SetupNodes() {
    //        Dictionary<Node<GenericPlace>, double> costDictA = new Dictionary<Node<GenericPlace>, double> { { nodeB, 4.0 } };
    //        foreach (Node<GenericPlace> node in costDictA.Keys) {
    //            nodeA.Coord.Neighbours.Add(node.Coord);
    //        }

    //        Dictionary<Node<GenericPlace>, double> costDictB = new Dictionary<Node<GenericPlace>, double> { { nodeA, 4.0 } };
    //        foreach (Node<GenericPlace> node in costDictB.Keys) {
    //            nodeB.Coord.Neighbours.Add(node.Coord);
    //        }
    //    }

    //    [TestMethod]
    //    public void TestNode_GetLabel() {
    //        SetupNodes();
    //        Console.WriteLine(gpA.Neighbours);
    //        int a = 2;
    //        //Assert.IsTrue(gridCoords2DBase.GetLabel().SequenceEqual(new int[2] { -3, 5 }));
    //    }

    //}
}
