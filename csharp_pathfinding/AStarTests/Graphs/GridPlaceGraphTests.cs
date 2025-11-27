using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.Providers.LinearAlgebra;
using NicUtils.ExtensionMethods;
using NSubstitute;

namespace AStarTests
{
    [TestClass]
    public class GridPlaceGraphTests
    {
        private GridPlaceGraph sut;

        private double[,] gridTerrainCosts = {
            { 1, 0, 0, 0, 0, 1 }, // 0
            { 1, 1, 1, 0, 1, 1 }, // 1
            { 1, 1, 1, 0, 1, 1 }, // 2
            { 1, 1, 1, 0, 1, 1 }, // 3
            { 1, 1, 1, 0, 1, 1 }, // 4
            { 1, 1, 1, 0, 1, 1 }, // 5
            { 1, 1, 1, 0, 1, 1 }, // 6
            { 1, 1, 1, 0, 0, 0 }, // 7
            { 1, 1, 1, 1, 1, 1 }, // 8
            { 1, 1, 1, 1, 1, 1 }, // 9
            { 1, 1, 1, 1, 0, 0 }  // 10
        };
        
        [TestInitialize]
        public void Initialize() {
            gridTerrainCosts = gridTerrainCosts.Transpose();
        }

        [TestMethod]
        public void TestIsBlocked()
        {
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector(),
                new HashSet<double>{0.9, 2.1});
            sut.BuildFromArray(gridTerrainCosts);
            
            // Moving to a non-existent place is blocked
            Assert.IsTrue(sut.IsBlocked((0, 0), (-1, 0), 0.9));
            
            // Moving into a >0 cost cell is not blocked
            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1),0.9));
            Assert.IsFalse(sut.IsBlocked((1, 0), (1, 1), 0.9));
            
            // Moving into a <=0 cost cell is blocked
            Assert.IsTrue(sut.IsBlocked((0, 0), (1, 0), 0.9));
            Assert.IsTrue(sut.IsBlocked((2, 0), (1, 0), 0.9));
            
            // Moving diagonally right past a corner is blocked
            Assert.IsTrue(sut.IsBlocked((2, 7), (3, 8), 0.9));
            Assert.IsTrue(sut.IsBlocked((3, 8), (2, 7), 0.9));
            
            // Otherwise moving diagonally is not blocked
            Assert.IsFalse(sut.IsBlocked((0, 7), (0, 8), 0.9));
            Assert.IsFalse(sut.IsBlocked((0, 8), (0, 7), 0.9));
            
            // Moving to a place where the pathfinder can't fit is blocked
            Assert.IsFalse(sut.IsBlocked((1, 3), (1, 2), 2.1));
            Assert.IsTrue(sut.IsBlocked((1, 2), (2, 1), 2.1));
        }
        
        [TestMethod]
        public void TestBuild_SucceedsForGoodGraphWithDiagonals()
        {
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector());
            sut.BuildFromFile("../../../Resources/excel_mazes/3x3_test.csv");

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
                Assert.IsTrue(sut.IsBlocked(placeLabel, (1, 1), 0.9));
            }

            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1), 0.9));
        }

        [TestMethod]
        public void TestBuild_SucceedsForGoodGraphWithoutDiagonals()
        {
            sut = new GridPlaceGraph(false, new PathfinderObstacleIntersector());
            sut.BuildFromFile("../../../Resources/excel_mazes/3x3_test.csv");

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
                Assert.IsTrue(sut.IsBlocked(placeLabel, (1, 1), 0.9));
            }

            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1), 0.9));
        }
        
        [TestMethod]
        public void TestBuild_ExceptionOnBadFileType()
        {
            sut = new GridPlaceGraph(false, new PathfinderObstacleIntersector());
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.BuildFromFile("../../../Resources/excel_mazes/3x3_test.txt"),
                "GridPlaceGraph only supports building from .csv files");
        }

        [TestMethod]
        public void TestBuild_ExceptionNonRectangularGrid()
        {
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector());
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.BuildFromFile("../../../Resources/excel_mazes/non-rectangular_test.csv"),
                "Cannot have a non-rectangular grid (row 0 has length 3 but row 1 has length 2).");
        }

        [TestMethod]
        public void TestBuild_ExceptionOnNegativeCost()
        {
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector());
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(
                () => sut.BuildFromFile("../../../Resources/excel_mazes/negative_cost_test.csv"),
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
            var mockIntersector = getMockIntersector();
            
            sut = new GridPlaceGraph(true, mockIntersector,
                new HashSet<double>{0.9, 1.1, 2.9, 3.1, 2*Math.Sqrt(2) - 0.01, 2*Math.Sqrt(2) + 0.01});
            sut.BuildFromArray(gridTerrainCosts);
            
            mockIntersector.Received(319)
                .CoordinateWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());
        
            // Inside a size 1 square
            Assert.IsTrue(sut.PathfinderCanFitCached(0, 0, 0.9));
            Assert.IsFalse(sut.PathfinderCanFitCached(0, 0, 1.1));
            
            // Inside a size 3 square
            Assert.IsTrue(sut.PathfinderCanFitCached(1, 2, 2.9));
            Assert.IsFalse(sut.PathfinderCanFitCached(1, 2, 3.1));
            
            // Overlap with a corner
            Assert.IsTrue(sut.PathfinderCanFitCached(2, 8, 0.9));
            Assert.IsTrue(sut.PathfinderCanFitCached(2, 8, 2*Math.Sqrt(2) - 0.01));
            Assert.IsFalse(sut.PathfinderCanFitCached(2, 8, 2*Math.Sqrt(2) + 0.01));
            
            // Due to caching, Intersector did not need to perform any further calcs after Build
            mockIntersector.Received(319)
                .CoordinateWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());
        }
        
        [TestMethod]
        public void TestPathfinderCanFitCached_SelectivelyCalledWhenTerrainGridAccessibilityUpdated()
        {
            var mockIntersector = getMockIntersector();
            
            sut = new GridPlaceGraph(true, mockIntersector,
                new HashSet<double>{0.9, 2*Math.Sqrt(2) + 0.01});
            sut.BuildFromArray(gridTerrainCosts);
            
            mockIntersector.Received(123)
                .CoordinateWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());
        
            // Initially, a collision
            Assert.IsFalse(sut.PathfinderCanFitCached(2, 8, 2*Math.Sqrt(2) + 0.01));
            
            // Caching means no further intersection checks
            mockIntersector.Received(123)
                .CoordinateWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());

            sut.SetTerrainCost((3, 7), 1);
            
            // No more collision
            Assert.IsTrue(sut.PathfinderCanFitCached(2, 8, 2*Math.Sqrt(2) + 0.01));
            
            // For bigger pathfinder, "radius" is 2 i.e. 25 cells. Should perform all 25 rechecks
            // Then for smaller pathfinder, "radius" is 1. Should perform only 8 rechecks since the large one can fit at coordinate (2, 8) after this change.
            mockIntersector.Received(123 + 25 + 8)
                .CoordinateWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());
        }
        
        // Initially, just calculated each cell's size accessibility from the middle of the cell.
        // That made this test fail because e.g. a size 1.9 pathfinder thinks it cannot stand on either of the
        // parallel lines of cells in a tunnel that is 2 wide.
        // Add collision checks centred on all corners as well as the cell center to make this pass.
        [TestMethod]
        public void TestPathfinderCanFitCached_FitsWhenSizeAndGapAreEqualAndEven()
        {
            var mockIntersector = getMockIntersector();
            
            sut = new GridPlaceGraph(true, mockIntersector,
                new HashSet<double>{0.9, 1.9});
            sut.BuildFromArray(gridTerrainCosts);
            
            mockIntersector.Received(83)
                .CoordinateWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());
        
            // Size 2 pathfinder can fit on either of the cells in a 2-width tunnel (by standing in the middle)
            // The results for PathfinderFitsCoords are deterministic the ordering of GRID_CORNER_DELTAS
            Assert.IsTrue(sut.PathfinderCanFitCached(4, 8, 1.9));
            Assert.AreEqual(sut.PathfinderFitsCoords[1.9][4, 8], (4.5, 8.5));
            Assert.IsTrue(sut.PathfinderCanFitCached(4, 9, 1.9));
            Assert.AreEqual(sut.PathfinderFitsCoords[1.9][4, 9], (3.5, 8.5));
            Assert.IsTrue(sut.PathfinderCanFitCached(5, 8, 1.9));
            Assert.AreEqual(sut.PathfinderFitsCoords[1.9][5, 8], (4.5, 8.5));
            Assert.IsTrue(sut.PathfinderCanFitCached(5, 9, 1.9));
            Assert.AreEqual(sut.PathfinderFitsCoords[1.9][5, 9], (4.5, 8.5));
            
            // Due to caching, Intersector did not need to perform any further calcs after Build
            mockIntersector.Received(83)
                .CoordinateWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>());
        }
        
        private IPathfinderObstacleIntersector getMockIntersector()
        {
            PathfinderObstacleIntersector concreteIntersector = new()
            {
                GridTerrainCosts = gridTerrainCosts
            };
            
            var mockIntersector = Substitute.For<IPathfinderObstacleIntersector>();
            mockIntersector
                .CoordinateWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<double>())
                .Returns(callInfo =>
                {
                    int x = callInfo.ArgAt<int>(0);
                    int y = callInfo.ArgAt<int>(1);
                    double size = callInfo.ArgAt<double>(2);
                    return concreteIntersector.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(x, y, size);
                });

            return mockIntersector;
        }
    }
}