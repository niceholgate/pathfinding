using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NSubstitute;

namespace AStarTests
{
    [TestClass]
    public class GridPlaceGraphTests
    {
        private GridPlaceGraph sut;

        //[TestInitialize]
        //public void Initialize() {
        //    sut = new();
        //}

        [TestMethod]
        public void TestBuild_SucceedsForGoodGraphWithDiagonals()
        {
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector());
            sut.Build("../../../Resources/excel_mazes/3x3_test.csv");

            // Check the costs
            Dictionary<(int, int), double> expectedCosts = new()
            {
                { (0, 0), 1.0 }, { (1, 0), 2.0 }, { (2, 0), 3.0 },
                { (0, 1), 8.0 }, { (1, 1), 0.0 }, { (2, 1), 4.0 },
                { (0, 2), 7.0 }, { (1, 2), 6.0 }, { (2, 2), 5.0 },
            };

            foreach (var item in expectedCosts) Assert.AreEqual(item.Value, sut.GetTerrainCost(item.Key));

            // Check the Places and their Neighbours
            foreach (var item in expectedCosts) Assert.IsTrue(sut.Places.ContainsKey(item.Key));

            GridPlace place00 = sut.GetPlaceOrCreate((0, 0));
            HashSet<(int, int)> expectedNeighbourLabels00 =
                new HashSet<(int, int)> { place00.E, place00.SE, place00.S };
            HashSet<(int, int)> neighbourLabels00 = place00.Neighbours.Select(x => x.Label).ToHashSet();
            Assert.IsTrue(expectedNeighbourLabels00.SetEquals(neighbourLabels00));

            GridPlace place10 = sut.GetPlaceOrCreate((1, 0));
            HashSet<(int, int)> expectedNeighbourLabels10 = new HashSet<(int, int)>
                { place10.E, place10.SE, place10.S, place10.SW, place10.W };
            HashSet<(int, int)> neighbourLabels10 = place10.Neighbours.Select(x => x.Label).ToHashSet();
            Assert.IsTrue(expectedNeighbourLabels10.SetEquals(neighbourLabels10));

            GridPlace place11 = sut.GetPlaceOrCreate((1, 1));
            HashSet<(int, int)> expectedNeighbourLabels11 = new HashSet<(int, int)>
                { place11.E, place11.SE, place11.S, place11.SW, place11.W, place11.NW, place11.N, place11.NE };
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
        public void TestBuild_SucceedsForGoodGraphWithoutDiagonals()
        {
            sut = new GridPlaceGraph(false, new PathfinderObstacleIntersector());
            sut.Build("../../../Resources/excel_mazes/3x3_test.csv");

            // Check the costs
            Dictionary<(int, int), double> expectedCosts = new()
            {
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
            HashSet<(int, int)> expectedNeighbourLabels11 = new HashSet<(int, int)>
                { place11.E, place11.S, place11.W, place11.N };
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
        public void TestBuild_ExceptionNonRectangularGrid()
        {
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector());
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.Build("../../../Resources/excel_mazes/non-rectangular_test.csv"),
                "Cannot have a non-rectangular grid (row 0 has length 3 but row 1 has length 2).");
        }

        [TestMethod]
        public void TestBuild_ExceptionOnNegativeCost()
        {
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector());
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.Build("../../../Resources/excel_mazes/negative_cost_test.csv"),
                "Cannot have a negative cost: -6 for (1, 2)");
        }

        // Not currently testing this because the disjoint check is not aware of blocked cells
        // [TestMethod]
        // public void TestBuild_FailsDisjointGraph() {
        //     sut = new GridPlaceGraph(true);
        //     TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
        //         () => sut.Build("../../../Resources/excel_mazes/spiral_disjoint_test.csv"),
        //         "Cannot support a disjoint Graph!");
        // }
        
        [TestMethod]
        public void TestPathfinderCanFitCached()
        {
            // Initial Build just to get the GridTerrainCosts for the concreteIntersector
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector(),
                new HashSet<double>{0.9, 1.1, 2.9, 3.1, Math.Sqrt(2) - 0.01, Math.Sqrt(2) + 0.01});
            sut.Build("../../../Resources/excel_mazes/walls_test.csv");
            
            PathfinderObstacleIntersector concreteIntersector = new()
            {
                GridTerrainCosts = sut.GetGridTerrainCosts()
            };
            
            var mockIntersector = Substitute.For<IPathfinderObstacleIntersector>();
            mockIntersector
                .PathfinderIntersectsWithObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>())
                .Returns(callInfo =>
                {
                    int x = callInfo.ArgAt<int>(0);
                    int y = callInfo.ArgAt<int>(1);
                    double size = callInfo.ArgAt<double>(2);
                    return concreteIntersector.PathfinderIntersectsWithObstacles(x, y, size);
                });
            
            sut = new GridPlaceGraph(true, mockIntersector,
                new HashSet<double>{0.9, 1.1, 2.9, 3.1, Math.Sqrt(2) - 0.01, Math.Sqrt(2) + 0.01});
            sut.Build("../../../Resources/excel_mazes/walls_test.csv");
            
            mockIntersector.Received(3936)
                .PathfinderIntersectsWithObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());
        
            // Inside a size 1 square
            Assert.IsTrue(sut.PathfinderCanFitCached(0, 0, 0.9));
            Assert.IsFalse(sut.PathfinderCanFitCached(0, 0, 1.1));
            
            // Inside a size 3 square
            Assert.IsTrue(sut.PathfinderCanFitCached(1, 2, 2.9));
            Assert.IsFalse(sut.PathfinderCanFitCached(1, 2, 3.1));
            
            // Overlap with a corner
            Assert.IsTrue(sut.PathfinderCanFitCached(2, 8, 0.9));
            Assert.IsTrue(sut.PathfinderCanFitCached(2, 8, Math.Sqrt(2) - 0.01));
            Assert.IsFalse(sut.PathfinderCanFitCached(2, 8, Math.Sqrt(2) + 0.01));
            
            // Due to caching, Intersector did not need to perform any further calcs
            mockIntersector.Received(3936)
                .PathfinderIntersectsWithObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());
        }
        
    }
}