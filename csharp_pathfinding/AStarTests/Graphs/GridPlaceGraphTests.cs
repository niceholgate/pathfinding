using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AStarTests {

    [TestClass]
    public class GridPlaceGraphTests {

        private GridPlaceGraph sut;

        //[TestInitialize]
        //public void Initialize() {
        //    sut = new();
        //}

        [TestMethod]
        public void TestBuild_SucceedsForGoodGraphWithDiagonals() {
            sut = new GridPlaceGraph(true);
            sut.Build("../../../Resources/excel_mazes/3x3_test.csv");

            // Check the costs
            Dictionary<(int, int), double> expectedCosts = new() {
                { (0, 0), 1.0 }, { (1, 0), 2.0 }, { (2, 0), 3.0 },
                { (0, 1), 8.0 }, { (1, 1), 0.0 }, { (2, 1), 4.0 },
                { (0, 2), 7.0 }, { (1, 2), 6.0 }, { (2, 2), 5.0 },
            };

            foreach (var item in expectedCosts) Assert.AreEqual(item.Value, sut.GetTerrainCost(item.Key));

            // Check the Places and their Neighbours
            foreach (var item in expectedCosts) Assert.IsTrue(sut.Places.ContainsKey(item.Key));

            GridPlace place00 = sut.GetPlaceOrCreate((0, 0));
            HashSet<(int, int)> expectedNeighbourLabels00 = new HashSet<(int, int)> { place00.E, place00.SE, place00.S };
            HashSet<(int, int)> neighbourLabels00 = place00.Neighbours.Select(x => x.Label).ToHashSet();
            Assert.IsTrue(expectedNeighbourLabels00.SetEquals(neighbourLabels00));

            GridPlace place10 = sut.GetPlaceOrCreate((1, 0));
            HashSet<(int, int)> expectedNeighbourLabels10 = new HashSet<(int, int)> { place10.E, place10.SE, place10.S, place10.SW, place10.W };
            HashSet<(int, int)> neighbourLabels10 = place10.Neighbours.Select(x => x.Label).ToHashSet();
            Assert.IsTrue(expectedNeighbourLabels10.SetEquals(neighbourLabels10));

            GridPlace place11 = sut.GetPlaceOrCreate((1, 1));
            HashSet<(int, int)> expectedNeighbourLabels11 = new HashSet<(int, int)> { place11.E, place11.SE, place11.S, place11.SW, place11.W, place11.NW, place11.N, place11.NE };
            HashSet<(int, int)> neighbourLabels11 = place11.Neighbours.Select(x => x.Label).ToHashSet();
            Assert.IsTrue(expectedNeighbourLabels11.SetEquals(neighbourLabels11));
            
            // Check inaccessible Place
            foreach (var placeLabel in sut.Places.Keys)
            {
                Assert.IsTrue(sut.IsBlocked(placeLabel, (1, 1)));
            }
            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1)));
        }

        [TestMethod]
        public void TestBuild_SucceedsForGoodGraphWithoutDiagonals() {
            sut = new GridPlaceGraph(false);
            sut.Build("../../../Resources/excel_mazes/3x3_test.csv");

            // Check the costs
            Dictionary<(int, int), double> expectedCosts = new() {
                { (0, 0), 1.0 }, { (1, 0), 2.0 }, { (2, 0), 3.0 },
                { (0, 1), 8.0 }, { (1, 1), 0.0 }, { (2, 1), 4.0 },
                { (0, 2), 7.0 }, { (1, 2), 6.0 }, { (2, 2), 5.0 },
            };

            foreach (var item in expectedCosts) Assert.AreEqual(item.Value, sut.GetTerrainCost(item.Key));

            // Check the Places and their Neighbours
            foreach (var item in expectedCosts) Assert.IsTrue(sut.Places.ContainsKey(item.Key));

            GridPlace place00 = sut.GetPlaceOrCreate((0, 0));
            HashSet<(int, int)> expectedNeighbourLabels00 = new HashSet<(int, int)> { place00.E, place00.S };
            HashSet<(int, int)> neighbourLabels00 = place00.Neighbours.Select(x => x.Label).ToHashSet();
            Assert.IsTrue(expectedNeighbourLabels00.SetEquals(neighbourLabels00));

            GridPlace place10 = sut.GetPlaceOrCreate((1, 0));
            HashSet<(int, int)> expectedNeighbourLabels10 = new HashSet<(int, int)> { place10.E, place10.S, place10.W };
            HashSet<(int, int)> neighbourLabels10 = place10.Neighbours.Select(x => x.Label).ToHashSet();
            Assert.IsTrue(expectedNeighbourLabels10.SetEquals(neighbourLabels10));

            GridPlace place11 = sut.GetPlaceOrCreate((1, 1));
            HashSet<(int, int)> expectedNeighbourLabels11 = new HashSet<(int, int)> { place11.E, place11.S, place11.W, place11.N };
            HashSet<(int, int)> neighbourLabels11 = place11.Neighbours.Select(x => x.Label).ToHashSet();
            Assert.IsTrue(expectedNeighbourLabels11.SetEquals(neighbourLabels11));
            
            // Check inaccessible Place
            foreach (var placeLabel in sut.Places.Keys)
            {
                Assert.IsTrue(sut.IsBlocked(placeLabel, (1, 1)));
            }
            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1)));
        }

        [TestMethod]
        public void TestBuild_ExceptionNonRectangularGrid() {
            sut = new GridPlaceGraph(true);
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.Build("../../../Resources/excel_mazes/non-rectangular_test.csv"),
                "Cannot have a non-rectangular grid (row 0 has length 3 but row 1 has length 2).");
        }

        [TestMethod]
        public void TestBuild_ExceptionOnNegativeCost() {
            sut = new GridPlaceGraph(true);
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.Build("../../../Resources/excel_mazes/negative_cost_test.csv"),
                "Cannot have a negative cost: -6 for (1, 2)");
        }

        //[TestMethod]
        //public void TestBuild_FailsDisjointGraph() {
        //    TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
        //        () => new GenericPlaceGraph("../../../Resources/mermaid_networks/netdisjoint.mmd"),
        //        "Cannot support a disjoint Graph!");
        //}
    }
}