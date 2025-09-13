using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;


namespace AStarTests {
    
    [TestClass]
    public class GenericPlaceTests {
        GenericPlace gpA = new("A");
        GenericPlace gpB = new("B");
        GenericPlace gpC = new("C");
        //
        // [TestMethod]
        // public void TestIsNeighbour() {
        //     // A and B are neighbours, C is disjoint
        //     gpA.ExplicitNeighboursWithCosts.Add(gpB, 1.0);
        //     gpB.ExplicitNeighboursWithCosts.Add(gpA, 1.0);
        //
        //     Assert.IsTrue(gpA.IsNeighbour(gpB));
        //     Assert.IsTrue(gpB.IsNeighbour(gpA));
        //     Assert.IsFalse(gpC.IsNeighbour(gpA));
        //     Assert.IsFalse(gpC.IsNeighbour(gpB));
        //     Assert.IsFalse(gpA.IsNeighbour(gpC));
        //     Assert.IsFalse(gpB.IsNeighbour(gpC));
        //
        //     Assert.AreEqual(0, gpC.ExplicitNeighboursWithCosts.Count);
        //     Assert.AreEqual(1, gpB.ExplicitNeighboursWithCosts.Count);
        //
        //     // Check you can remove a neighbour in the HashSet
        //     gpA.ExplicitNeighboursWithCosts.Remove(gpB);
        //     Assert.AreEqual(0, gpA.ExplicitNeighboursWithCosts.Count);
        //
        //     // Check you cannot remove a non-existent neighbour
        //     gpC.ExplicitNeighboursWithCosts.Remove(gpB);
        //     Assert.AreEqual(0, gpC.ExplicitNeighboursWithCosts.Count);
        // }

        [TestMethod]
        public void TestToString() {
            // Check ToString() method returns the label
            Assert.AreEqual(gpA.Label, "A");
            Assert.AreEqual(gpA.Label, gpA.ToString());
        }
        
        [TestMethod]
        public void TestUpdateNeighbours() {
            // Check ToString() method returns the label
            Assert.IsNotNull(gpA.Neighbours);
            Assert.IsEmpty(gpA.Neighbours);
            gpA.Neighbours.Add(gpB);
            gpA.Neighbours.Add(gpB);
            Assert.HasCount(1, gpA.Neighbours);
            Assert.Contains(gpB, gpA.Neighbours);
            gpA.Neighbours.Add(gpC);
            Assert.HasCount(2, gpA.Neighbours);
            Assert.Contains(gpB, gpA.Neighbours);
            Assert.Contains(gpC, gpA.Neighbours);
        }
    }
}
