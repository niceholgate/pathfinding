using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AStarTests {
    
    [TestClass]
    public class GenericPlaceTests {
        private static GenericPlaceGraph _genericPlaceGraph1 = new();
        GenericPlace gpA = new("A", _genericPlaceGraph1);
        GenericPlace gpB = new("B", _genericPlaceGraph1);
        GenericPlace gpC = new("C", _genericPlaceGraph1);

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
}
