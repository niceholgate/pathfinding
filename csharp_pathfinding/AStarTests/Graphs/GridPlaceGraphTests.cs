using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using NicUtils.ExtensionMethods;
using NSubstitute;

namespace AStarTests
{
    [TestClass]
    public class GridPlaceGraphTests
    {
        private GridPlaceGraph sut;

        private float[,] gridTerrainCosts = {
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
            { 1, 1, 1, 1, 0, 0 }, // 10
            { 1, 1, 0, 1, 0, 0 }, // 11
            { 0, 1, 1, 1, 0, 0 }, // 12
            { 1, 1, 1, 1, 0, 0 }  // 13
        };

        private IPathfinderObstacleIntersector mockIntersector;
        
        [TestInitialize]
        public void Initialize() {
            gridTerrainCosts = gridTerrainCosts.Transpose();
            mockIntersector = getMockIntersector();
        }

        [TestMethod]
        public void TestIsBlocked()
        {
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector(),
                new HashSet<float>{0.9f, 1.6f, 2.1f});
            sut.BuildFromArray(gridTerrainCosts);
            
            // Moving to a non-existent place is blocked
            Assert.IsTrue(sut.IsBlocked((0, 0), (-1, 0), 0.9f));
            
            // Moving into a >0 cost cell is not blocked
            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1), 0.9f));
            Assert.IsFalse(sut.IsBlocked((1, 0), (1, 1), 0.9f));
            
            // Moving into a <=0 cost cell is blocked
            Assert.IsTrue(sut.IsBlocked((0, 0), (1, 0), 0.9f));
            Assert.IsTrue(sut.IsBlocked((2, 0), (1, 0), 0.9f));
            
            // Moving diagonally right past a corner is blocked
            Assert.IsTrue(sut.IsBlocked((2, 7), (3, 8), 0.9f));
            Assert.IsTrue(sut.IsBlocked((3, 8), (2, 7), 0.9f));
            
            // Otherwise moving diagonally is not blocked
            Assert.IsFalse(sut.IsBlocked((0, 7), (0, 8), 0.9f));
            Assert.IsFalse(sut.IsBlocked((0, 8), (0, 7), 0.9f));
            
            // Moving to a place where the pathfinder can't fit is blocked
            Assert.IsFalse(sut.IsBlocked((1, 3), (1, 2), 2.1f));
            Assert.IsTrue(sut.IsBlocked((1, 2), (2, 1), 2.1f));
            
            // Squeezing between corners (but not edges) that are closer together than the pathfinder's size is blocked
            Assert.IsFalse(sut.IsBlocked((1, 11), (1, 12), 0.9f));
            Assert.IsTrue(sut.IsBlocked((1, 11), (1, 12), 1.6f));
            
            // (and same but diagonally)
            float[,] gridTerrainCosts2 = {
                { 1, 1, 1, 1, 1, 1 }, // 0
                { 1, 1, 1, 1, 0, 1 }, // 1
                { 1, 1, 1, 1, 1, 1 }, // 2
                { 1, 1, 1, 1, 1, 1 }, // 3
                { 1, 0, 1, 1, 1, 1 }, // 4
                { 1, 1, 1, 1, 1, 1 }, // 5
                { 1, 1, 1, 1, 1, 1 }  // 6
            };
            GridPlaceGraph sut2 = new(true, new PathfinderObstacleIntersector(),
            new HashSet<float>{2*MathF.Sqrt(2) - 0.01f, 2*MathF.Sqrt(2) + 0.01f});
            sut2.BuildFromArray(gridTerrainCosts2);
            Assert.IsFalse(sut2.IsBlocked((2, 2), (3, 3), 2*MathF.Sqrt(2) - 0.01f));
            Assert.IsTrue(sut2.IsBlocked((2, 2), (3, 3), 2*MathF.Sqrt(2) + 0.01f));
        }
        
        [TestMethod]
        public void TestBuild_SucceedsForGoodGraphWithDiagonals()
        {
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector());
            sut.BuildFromFile("../../../Resources/excel_mazes/3x3_test.csv");

            // Check the costs
            Dictionary<(int, int), float> expectedCosts = new()
            {
                { (0, 0), 1.0f }, { (1, 0), 2.0f }, { (2, 0), 3.0f },
                { (0, 1), 8.0f }, { (1, 1), 0.0f }, { (2, 1), 4.0f },
                { (0, 2), 7.0f }, { (1, 2), 6.0f }, { (2, 2), 5.0f },
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
                Assert.IsTrue(sut.IsBlocked(placeLabel, (1, 1), 0.9f));
            }

            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1), 0.9f));
        }

        [TestMethod]
        public void TestBuild_SucceedsForGoodGraphWithoutDiagonals()
        {
            sut = new GridPlaceGraph(false, new PathfinderObstacleIntersector());
            sut.BuildFromFile("../../../Resources/excel_mazes/3x3_test.csv");

            // Check the costs
            Dictionary<(int, int), float> expectedCosts = new()
            {
                { (0, 0), 1.0f }, { (1, 0), 2.0f }, { (2, 0), 3.0f },
                { (0, 1), 8.0f }, { (1, 1), 0.0f }, { (2, 1), 4.0f },
                { (0, 2), 7.0f }, { (1, 2), 6.0f }, { (2, 2), 5.0f },
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
                Assert.IsTrue(sut.IsBlocked(placeLabel, (1, 1), 0.9f));
            }

            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1), 0.9f));
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
            sut = new GridPlaceGraph(true, mockIntersector,
                new HashSet<float>{0.9f, 1.1f, 2.9f, 3.1f, 2*MathF.Sqrt(2) - 0.01f, 2*MathF.Sqrt(2) + 0.01f});
            sut.BuildFromArray(gridTerrainCosts);
            
            mockIntersector.Received(495)
                .CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>());
        
            // Inside a size 1 square
            Assert.IsTrue(sut.PathfinderCanFitCached(0, 0, 0.9f));
            Assert.IsFalse(sut.PathfinderCanFitCached(0, 0, 1.1f));

            // Inside a size 3 square
            Assert.IsTrue(sut.PathfinderCanFitCached(1, 2, 2.9f));
            Assert.IsFalse(sut.PathfinderCanFitCached(1, 2, 3.1f));

            // Overlap with a corner
            Assert.IsTrue(sut.PathfinderCanFitCached(2, 8, 0.9f));
            Assert.IsTrue(sut.PathfinderCanFitCached(2, 8, 2 * MathF.Sqrt(2.0f) - 0.01f));
            Assert.IsFalse(sut.PathfinderCanFitCached(2, 8, 2 * MathF.Sqrt(2.0f) + 0.01f));
            
            // Due to caching, Intersector did not need to perform any further calcs after Build
            mockIntersector.Received(495)
                .CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>());
            }
        
        [TestMethod]
        public void TestPathfinderCanFitCached_SelectivelyCalledWhenTerrainGridAccessibilityUpdated()
        {
            sut = new GridPlaceGraph(true, mockIntersector,
                new HashSet<float>{0.9f, 2*MathF.Sqrt(2) + 0.01f});
            sut.BuildFromArray(gridTerrainCosts);
            
            mockIntersector.Received(168)
                .CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>());
        
            // Initially, a collision
            Assert.IsFalse(sut.PathfinderCanFitCached(2, 8, 2*MathF.Sqrt(2) + 0.01f));
            
            // Caching means no further intersection checks
            mockIntersector.Received(168)
                .CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>());

            sut.SetTerrainCost((3, 7), 1);
            
            // No more collision
            Assert.IsTrue(sut.PathfinderCanFitCached(2, 8, 2*MathF.Sqrt(2) + 0.01f));
            
            // For bigger pathfinder, "radius" is 2 i.e. 25 cells. Should perform all 25 rechecks
            // Then for smaller pathfinder, "radius" is 1. Should perform all 9 rechecks since the large one can fit at coordinate (2, 8) after this change,
            // but not at every single corner on (2, 8) (in which case it would skip recheck for the central cell).
            mockIntersector.Received(168 + 25 + 9)
                .CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>());
        }
        
        // Initially, just calculated each cell's size accessibility from the middle of the cell.
        // That made this test fail because e.g. a size 1.9 pathfinder thinks it cannot stand on either of the
        // parallel lines of cells in a tunnel that is 2 wide.
        // Add collision checks centred on all corners as well as the cell center to make this pass.
        [TestMethod]
        public void TestPathfinderCanFitCached_FitsWhenSizeAndGapAreEqualAndEven()
        {
            sut = new GridPlaceGraph(true, mockIntersector,
                new HashSet<float>{0.9f, 1.9f});
            sut.BuildFromArray(gridTerrainCosts);
            
            mockIntersector.Received(159)
                .CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>());
        
            // Size 2 pathfinder can fit on either of the cells in a 2-width tunnel (by standing in the middle)
            // The results for PathfinderFitsCoords are deterministic the ordering of GRID_CORNER_DELTAS
            Assert.IsTrue(sut.PathfinderCanFitCached(4, 8, 1.9f));
            Assert.Contains((4.5f, 8.5f), sut.PathfinderFitsCoords[1.9f][4, 8].CornersFarthestFromBlockages);
            Assert.IsTrue(sut.PathfinderCanFitCached(4, 9, 1.9f));
            Assert.Contains((3.5f, 8.5f), sut.PathfinderFitsCoords[1.9f][4, 9].CornersFarthestFromBlockages);
            Assert.IsTrue(sut.PathfinderCanFitCached(5, 8, 1.9f));
            Assert.Contains((4.5f, 8.5f), sut.PathfinderFitsCoords[1.9f][5, 8].CornersFarthestFromBlockages);
            Assert.IsTrue(sut.PathfinderCanFitCached(5, 9, 1.9f));
            Assert.Contains((4.5f, 8.5f), sut.PathfinderFitsCoords[1.9f][5, 9].CornersFarthestFromBlockages);
            
            // Due to caching, Intersector did not need to perform any further calcs after Build
            mockIntersector.Received(495)
                .CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>());
            }

        [TestMethod]
        public void TestGetDistanceToLineSegment()
        {
            ////////////// PROJECTION OF POINT IS WITHIN THE LINE SEGMENT
            
            // Horizontal line
            Assert.AreEqual(10.0f, GridPlaceGraph.GetDistanceToLineSegment((0, 0), (1, 0), (0.5f, 10)), 1e-6f);
            Assert.AreEqual(10.0f, GridPlaceGraph.GetDistanceToLineSegment((0, 0), (1, 0), (0.5f, -10)), 1e-6f);

            // Vertical line
            Assert.AreEqual(10.0f, GridPlaceGraph.GetDistanceToLineSegment((0, 0), (0, 1), (10, 0.5f)), 1e-6f);
            Assert.AreEqual(10.0f, GridPlaceGraph.GetDistanceToLineSegment((0, 0), (0, 1), (-10, 0.5f)), 1e-6f);

            // 45 degree line
            // Point (0, 1) should be at distance 1/sqrt(2)
            Assert.AreEqual(1/MathF.Sqrt(2.0f), GridPlaceGraph.GetDistanceToLineSegment((0, 0), (1, 1), (0, 1)), 1e-6f);

            // Line points are same
            Assert.AreEqual(5.0f, GridPlaceGraph.GetDistanceToLineSegment((0, 0), (0, 0), (3, 4)), 1e-6f);
            
            ////////////// PROJECTION OF POINT IS BEYOND THE LINE SEGMENT
            Assert.AreEqual(MathF.Sqrt(2.0f), GridPlaceGraph.GetDistanceToLineSegment((0, 0), (1, 0), (2, 1)), 1e-6f);
            Assert.AreEqual(MathF.Sqrt(2.0f), GridPlaceGraph.GetDistanceToLineSegment((0, 0), (1, 0), (-1, 1)), 1e-6f);
        }
        
        [TestMethod]
        public void TestSmoothPathAroundBlockages()
        {
            float pathfinderSize = 0.9f;
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector(),
                new HashSet<float>{pathfinderSize});
            sut.BuildFromFile("../../../Resources/excel_mazes/walls_test.csv");
        
            List<GridPlace> originalPath = new()
            {
                new GridPlace((0, 2)), new GridPlace((0, 3)), new GridPlace((1, 4)), new GridPlace((1, 5)),
                new GridPlace((1, 6)), new GridPlace((2, 7)), new GridPlace((2, 8)), new GridPlace((3, 8)),
                new GridPlace((4, 8)), new GridPlace((5, 8)), new GridPlace((6, 8)), new GridPlace((7, 8)),
                new GridPlace((8, 8)), new GridPlace((9, 8)), new GridPlace((10, 9)), new GridPlace((11, 9)),
                new GridPlace((12, 9)), new GridPlace((13, 9)), new GridPlace((14, 9)), new GridPlace((15, 9)),
                new GridPlace((16, 9)), new GridPlace((17, 9)), new GridPlace((18, 9)), new GridPlace((19, 9)),
                new GridPlace((20, 9)), new GridPlace((21, 9)), new GridPlace((22, 9)), new GridPlace((22, 10)),
                new GridPlace((22, 11)), new GridPlace((22, 12)), new GridPlace((21, 12)), new GridPlace((20, 12)),
                new GridPlace((19, 13))
            };
            
            List<(float, float)> expectedSmoothPath = new()
            {
                (0f, 2f), (2f, 8f), (22f, 9f), (22f, 12f), (19f, 13f)
            };
            
            List<(float, float)> occupiablePath = sut.GetOccupiablePath(originalPath, pathfinderSize);
            List<(float, float)> actualSmoothPath = sut.SmoothPath(occupiablePath, originalPath, pathfinderSize);
        
            CollectionAssert.AreEqual(expectedSmoothPath, actualSmoothPath);
        }
        
        [TestMethod]
        public void TestSmoothPathAroundBlockages2()
        {
            float pathfinderSize = 1.9f;
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector(),
                new HashSet<float>{pathfinderSize});
            sut.BuildFromFile("../../../Resources/excel_mazes/walls_test.csv");
            
            List<GridPlace> originalPath = new()
            {
                new GridPlace((26, 6)), new GridPlace((26, 5)), new GridPlace((26, 4)), new GridPlace((27, 4)),
                new GridPlace((28, 4)), new GridPlace((29, 5)), new GridPlace((30, 6)), new GridPlace((30, 7)),
                new GridPlace((31, 8))
            };
            
            List<(float, float)> expectedSmoothPath = new()
            {
                (25.5f, 5.5f), (25.5f, 3.5f), (28.5f, 3.5f), (31.0f, 8.0f)
            };
            
            List<(float, float)> occupiablePath = sut.GetOccupiablePath(originalPath, pathfinderSize);
            List<(float, float)> actualSmoothPath = sut.SmoothPath(occupiablePath, originalPath, pathfinderSize);
            
            CollectionAssert.AreEqual(expectedSmoothPath, actualSmoothPath);
        }
        
        // Refer to grid_path_smoothing_swamp_issue.png and walls_and_swamps_test.csv
        [TestMethod]
        public void TestSmoothPathAroundSwamps()
        {
            float pathfinderSize = 0.9f;
            sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector(),
                new HashSet<float>{pathfinderSize});
            sut.BuildFromFile("../../../Resources/excel_mazes/walls_and_swamps_test.csv");
        
            List<GridPlace> originalPath = new()
            {
                new GridPlace((4, 4)), new GridPlace((5, 5)), new GridPlace((6, 5)), new GridPlace((7, 5)),
                new GridPlace((8, 5)), new GridPlace((9, 6)), new GridPlace((9, 7)), new GridPlace((8, 8))
            };
            
            List<(float, float)> expectedSmoothPath = new()
            {
                (4f, 4f), (8f, 5f), (9f, 6f), (8f, 8f)
            };
        
            List<(float, float)> occupiablePath = sut.GetOccupiablePath(originalPath, pathfinderSize);
            List<(float, float)> actualSmoothPath = sut.SmoothPath(occupiablePath, originalPath, pathfinderSize);
            
            CollectionAssert.AreEqual(expectedSmoothPath, actualSmoothPath);
        }

        // [TestMethod]
        // public void TestGetThirdPointThatMinimisesAcuteAngle()
        // {
        //     sut = new GridPlaceGraph(true, new PathfinderObstacleIntersector());
        //
        //     // Case 1: Simple alignment
        //     // A=(0,1), B=(0,0). vBA = (0,1) [Up]
        //     // Candidates:
        //     // 1. (0, 2) -> vBC=(0,2). Aligned. Score 1.
        //     // 2. (1, 0) -> vBC=(1,0). 90 deg. Score 0.
        //     // 3. (0, -1) -> vBC=(0,-1). 180 deg. Score -1.
        //     var result = sut.GetThirdPointThatMinimisesAcuteAngle((0, 1), (0, 0), new List<(float, float)>
        //     {
        //         (0, 2), (1, 0), (0, -1)
        //     });
        //     Assert.AreEqual((0f, 2f), result);
        //
        //     // Case 2: 45 degrees vs 90 degrees
        //     // A=(1,1), B=(0,0). vBA = (1,1) [Top-Right]
        //     // Candidates:
        //     // 1. (0, 1) -> vBC=(0,1). 45 deg.
        //     // 2. (-1, 1) -> vBC=(-1,0). 90 deg.
        //     result = sut.GetThirdPointThatMinimisesAcuteAngle((1, 1), (0, 0), new List<(float, float)>
        //     {
        //         (0, 1), (-1, 1)
        //     });
        //     Assert.AreEqual((0f, 1f), result);
        //
        //     // Case 3: Tie-breaking (first one is kept if scores are equal)
        //     // A=(0,1), B=(0,0). vBA = Up.
        //     // Cand1: (1, 1) -> Top-Right (45 deg)
        //     // Cand2: (-1, 1) -> Top-Left (45 deg)
        //     // Both have cos = 1/sqrt(2).
        //     result = sut.GetThirdPointThatMinimisesAcuteAngle((0, 1), (0, 0), new List<(float, float)>
        //     {
        //         (1, 1), (-1, 1)
        //     });
        //     Assert.AreEqual((1f, 1f), result);
        //
        //     // Check reverse order for tie-break
        //     result = sut.GetThirdPointThatMinimisesAcuteAngle((0, 1), (0, 0), new List<(float, float)>
        //     {
        //         (-1, 1), (1, 1)
        //     });
        //     Assert.AreEqual((-1f, 1f), result);
        // }
        
        private IPathfinderObstacleIntersector getMockIntersector()
        {
            PathfinderObstacleIntersector concreteIntersector = new()
            {
                GridTerrainCosts = gridTerrainCosts
            };
            
            var mockIntersector = Substitute.For<IPathfinderObstacleIntersector>();
            mockIntersector
                .CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<float>())
                .Returns(callInfo =>
                {
                    int x = callInfo.ArgAt<int>(0);
                    int y = callInfo.ArgAt<int>(1);
                    float size = callInfo.ArgAt<float>(2);
                    return concreteIntersector.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(x, y, size);
                });

            return mockIntersector;
        }
    }
}