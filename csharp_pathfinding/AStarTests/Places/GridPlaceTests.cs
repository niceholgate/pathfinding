using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using System;

namespace AStarTests {

    [TestClass]
    public class GridPlaceTests {
        private static readonly GridPlaceGraph mockGridPlaceGraph = Substitute.For<GridPlaceGraph>(true);
        private static readonly GridPlace gridPlaceBase1 = new((-3, 5));
        private static readonly GridPlace gridPlaceDiag1 = new((-4, 6));
        private static readonly GridPlace gridPlaceStra1 = new((-3, 6));
        private static readonly GridPlace gridPlaceDistant1 = new((-60, 60));

        [TestMethod]
        public void TestDistanceFrom() {
            double expectedDistance = Math.Sqrt(Math.Pow((-60) - (-3), 2.0) + Math.Pow((60 - 5), 2.0));
            double result = gridPlaceBase1.DistanceFrom(gridPlaceDistant1, NicUtils.Distances2D.HeuristicType.Euclidian);
            Assert.AreEqual(result, expectedDistance);
        }

        [TestMethod]
        public void TestDeltaFrom() {
            (int, int) expectedDelta = ((-60) - (-3), 60 - 5);
            Assert.AreEqual(gridPlaceDistant1.DeltaFrom(gridPlaceBase1), expectedDelta);
        }

        [TestMethod]
        public void TestIsDiagonalTo() {
            Assert.IsFalse(gridPlaceBase1.IsDiagonalTo(gridPlaceStra1));
            Assert.IsTrue(gridPlaceBase1.IsDiagonalTo(gridPlaceDiag1));
        }

        [TestMethod]
        public void TestIsAdjacentTo() {
            Assert.IsTrue(gridPlaceBase1.IsAdjacentTo(gridPlaceStra1));
            Assert.IsFalse(gridPlaceBase1.IsAdjacentTo(gridPlaceDiag1));
        }

        [TestMethod]
        public void TestToString() {
            Assert.AreEqual("(-3, 5)", gridPlaceBase1.ToString());
        }

        //[TestMethod]
        //public void TestIsNeighbour() {
        //    // Explicit neighbours (not automatically bidirectional)
        //    gridPlaceDistant1.ExplicitNeighboursWithCosts.Add(gridPlaceBase1, 1.0);
        //    Assert.IsTrue(gridPlaceDistant1.IsNeighbour(gridPlaceBase1));
        //    Assert.IsFalse(gridPlaceBase1.IsNeighbour(gridPlaceDistant1));
        //    gridPlaceBase1.ExplicitNeighboursWithCosts.Add(gridPlaceDistant1, 1.0);
        //    Assert.IsTrue(gridPlaceBase1.IsNeighbour(gridPlaceDistant1));

        //    // Grid neighbours (remove the above explicit neighbours first)
        //    gridPlaceBase1.ExplicitNeighboursWithCosts.Remove(gridPlaceDistant1);
        //    gridPlaceDistant1.ExplicitNeighboursWithCosts.Remove(gridPlaceBase1);
        //    Assert.IsFalse(gridPlaceBase1.IsNeighbour(gridPlaceDistant1));

        //    Assert.IsTrue(gridPlaceBase1.IsNeighbour(gridPlaceDiag1));
        //    Assert.IsTrue(gridPlaceBase1.IsNeighbour(gridPlaceStra1));
        //    Assert.IsTrue(gridPlaceDiag1.IsNeighbour(gridPlaceBase1));
        //    Assert.IsTrue(gridPlaceStra1.IsNeighbour(gridPlaceBase1));

        //    // Grid neighbours on different graphs are not neighbours
        //    Assert.IsFalse(gridPlaceBase1.IsNeighbour(gridPlaceDiag2));
        //    Assert.IsFalse(gridPlaceBase1.IsNeighbour(gridPlaceStra2));
        //    Assert.IsFalse(gridPlaceDiag1.IsNeighbour(gridPlaceBase2));
        //    Assert.IsFalse(gridPlaceStra1.IsNeighbour(gridPlaceBase2));
        //}
    }
}
