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

        private float _sub2Sqrt2 = 2*MathF.Sqrt(2.0f) - 0.01f;
        private float _sup2Sqrt2 = 2*MathF.Sqrt(2.0f) + 0.01f;

        [TestInitialize]
        public void Initialize() {
            gridTerrainCosts = gridTerrainCosts.Transpose();
        }

        private static void SetupBlockagesFromTerrainCosts(GridPlaceGraph graph, string layerName = "default")
        {
            int width = graph.GetWidth();
            int height = graph.GetHeight();
            bool[,] blockages = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    blockages[x, y] = graph.GetTerrainCost((x, y)) <= 0;        
                }
            }
            graph.SetBlockageLayer(layerName, blockages);
        }

        [TestMethod]
        public void TestSetBlockage()
        {
            sut = new GridPlaceGraph(true, new HashSet<float> { 0.9f, 1.6f, 2.1f });
            sut.BuildFromArray(gridTerrainCosts);
            SetupBlockagesFromTerrainCosts(sut);

            PathfinderAttributes attrs = new(0.9f, "default");

            // --- Block a walkable cell, then unblock it ---
            sut.SetBlockage("default", (0, 0), true);
            Assert.IsTrue(sut.IsBlocked((0, 1), (0, 0), attrs));

            sut.SetBlockage("default", (0, 0), false);
            Assert.IsFalse(sut.IsBlocked((0, 1), (0, 0), attrs));

            // --- Non-existent layer is created automatically, places are unblocked by default ---
            sut.SetBlockage("newLayer", (5, 5), true);
            Assert.IsTrue(sut.IsBlocked((4, 5), (5, 5), new PathfinderAttributes(0.9f, "newLayer")));
            Assert.IsFalse(sut.IsBlocked((3, 3), (3, 4), new PathfinderAttributes(0.9f, "newLayer")));

            // --- Setting the same value is a no-op ---
            sut.SetBlockage("newLayer", (5, 5), true);
            Assert.IsTrue(sut.IsBlocked((5, 5), (6, 5), new PathfinderAttributes(0.9f, "newLayer")));

            // --- Out of bounds throws ArgumentOutOfRangeException ---
            AssertThrowsException<ArgumentOutOfRangeException>(
                () => sut.SetBlockage("default", (-1, 0), true));
            AssertThrowsException<ArgumentOutOfRangeException>(
                () => sut.SetBlockage("default", (0, -1), true));
            AssertThrowsException<ArgumentOutOfRangeException>(
                () => sut.SetBlockage("default", (sut.GetWidth(), 0), true));
            AssertThrowsException<ArgumentOutOfRangeException>(
                () => sut.SetBlockage("default", (0, sut.GetHeight()), true));
            
            // --- Un/setting blockage in one coord affects large pathfinders trying to fit in neighbour coords
            // Initially not blocked
            sut.SetBlockage("newLayer2", (5, 5), false);
            Assert.IsFalse(sut.IsBlocked((3, 5), (4, 5), new PathfinderAttributes(2.1f, "newLayer2")));
            
            // Blocked by a neighbour cell becoming blocked
            sut.SetBlockage("newLayer2", (5, 5), true);
            Assert.IsTrue(sut.IsBlocked((3, 5), (4, 5), new PathfinderAttributes(2.1f, "newLayer2")));
            // (but a small pathfinder is not blocked by that neighbour cell)
            Assert.IsFalse(sut.IsBlocked((3, 5), (4, 5), new PathfinderAttributes(0.9f, "newLayer2")));
            
            // Unblocked again when that neighbour cell changes back to unblocked
            sut.SetBlockage("newLayer2", (5, 5), false);
            Assert.IsFalse(sut.IsBlocked((3, 5), (4, 5), new PathfinderAttributes(2.1f, "newLayer2")));
        }

        [TestMethod]
        public void TestIsBlocked()
        {
            sut = new GridPlaceGraph(true, new HashSet<float>{0.9f, 1.6f, 2.1f});
            sut.BuildFromArray(gridTerrainCosts);
            SetupBlockagesFromTerrainCosts(sut);

            // Moving to a non-existent place is blocked
            Assert.IsTrue(sut.IsBlocked((0, 0), (-1, 0), new PathfinderAttributes(0.9f, "default")));

            // Moving into a >0 cost cell is not blocked
            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1), new PathfinderAttributes(0.9f, "default")));
            Assert.IsFalse(sut.IsBlocked((0, 1), (1, 1), new PathfinderAttributes(0.9f, "default")));

            // Moving into a <=0 cost cell is blocked
            Assert.IsTrue(sut.IsBlocked((0, 0), (1, 0), new PathfinderAttributes(0.9f, "default")));
            Assert.IsTrue(sut.IsBlocked((2, 0), (1, 0), new PathfinderAttributes(0.9f, "default")));

            // Moving diagonally right past a corner is blocked
            Assert.IsTrue(sut.IsBlocked((2, 7), (3, 8), new PathfinderAttributes(0.9f, "default")));
            Assert.IsTrue(sut.IsBlocked((3, 8), (2, 7), new PathfinderAttributes(0.9f, "default")));

            // Otherwise moving diagonally is not blocked
            Assert.IsFalse(sut.IsBlocked((0, 7), (0, 8), new PathfinderAttributes(0.9f, "default")));
            Assert.IsFalse(sut.IsBlocked((0, 8), (0, 7), new PathfinderAttributes(0.9f, "default")));

            // Moving to a place where the pathfinder can't fit is blocked      
            Assert.IsFalse(sut.IsBlocked((1, 3), (1, 2), new PathfinderAttributes(2.1f, "default")));
            Assert.IsTrue(sut.IsBlocked((1, 2), (2, 1), new PathfinderAttributes(2.1f, "default")));

            // Squeezing between corners (but not edges) that are closer together than the pathfinder's size is blocked
            Assert.IsFalse(sut.IsBlocked((1, 11), (1, 12), new PathfinderAttributes(0.9f, "default")));
            Assert.IsTrue(sut.IsBlocked((1, 11), (1, 12), new PathfinderAttributes(1.6f, "default")));

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
            GridPlaceGraph sut2 = new(true, new HashSet<float>{_sub2Sqrt2, _sup2Sqrt2});
            sut2.BuildFromArray(gridTerrainCosts2);
            SetupBlockagesFromTerrainCosts(sut2);
            Assert.IsFalse(sut2.IsBlocked((2, 2), (3, 3), new PathfinderAttributes(_sub2Sqrt2, "default")));
            Assert.IsTrue(sut2.IsBlocked((2, 2), (3, 3), new PathfinderAttributes(_sup2Sqrt2, "default")));
        }

        [TestMethod]
        public void TestBuild_SucceedsForGoodGraphWithDiagonals()
        {
            sut = new GridPlaceGraph(true);
            sut.BuildFromFile("../../../Resources/excel_mazes/3x3_test.csv");   
            SetupBlockagesFromTerrainCosts(sut);

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
                Assert.IsTrue(sut.IsBlocked(placeLabel, (1, 1), new PathfinderAttributes(0.9f, "default")));
            }

            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1), new PathfinderAttributes(0.9f, "default")));
        }

        [TestMethod]
        public void TestBuild_SucceedsForGoodGraphWithoutDiagonals()
        {
            sut = new GridPlaceGraph(false);
            sut.BuildFromFile("../../../Resources/excel_mazes/3x3_test.csv");   
            SetupBlockagesFromTerrainCosts(sut);

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
                Assert.IsTrue(sut.IsBlocked(placeLabel, (1, 1), new PathfinderAttributes(0.9f, "default")));
            }

            Assert.IsFalse(sut.IsBlocked((0, 0), (0, 1), new PathfinderAttributes(0.9f, "default")));
        }

        [TestMethod]
        public void TestBuild_ExceptionOnBadFileType()
        {
            sut = new GridPlaceGraph(false);
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(    
                () => sut.BuildFromFile("../../../Resources/excel_mazes/3x3_test.txt"),
                "GridPlaceGraph only supports building from .csv files");       
        }

        [TestMethod]
        public void TestBuild_ExceptionNonRectangularGrid()
        {
            sut = new GridPlaceGraph(true);
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(    
                () => sut.BuildFromFile("../../../Resources/excel_mazes/non-rectangular_test.csv"),
                "Cannot have a non-rectangular grid (row 0 has length 3 but row 1 has length 2).");
        }

        [TestMethod]
        public void TestBuild_ExceptionOnNegativeCost()
        {
            sut = new GridPlaceGraph(true);
            TestHelpers.AssertThrowsExceptionWithMessage<ArgumentException>(    
                () => sut.BuildFromFile("../../../Resources/excel_mazes/negative_cost_test.csv"),
                "Cannot have a negative cost: -6 for (1, 2)");
        }

        // [TestMethod]
        // public void TestPathfinderCanFit()
        // {
        //     sut = new GridPlaceGraph(true, new HashSet<float>{0.9f, 1.1f, 2.9f, 3.1f, _sub2Sqrt2, _sup2Sqrt2});
        //     sut.BuildFromArray(gridTerrainCosts);
        //     SetupBlockagesFromTerrainCosts(sut);
        //
        //     // Inside a size 1 square
        //     Assert.IsTrue(sut.PathfinderCanFit(0, 0, new PathfinderAttributes(0.9f, "default")));
        //     Assert.IsFalse(sut.PathfinderCanFit(0, 0, new PathfinderAttributes(1.1f, "default")));
        //
        //     // Inside a size 3 square
        //     Assert.IsTrue(sut.PathfinderCanFit(1, 2, new PathfinderAttributes(2.9f, "default")));
        //     Assert.IsFalse(sut.PathfinderCanFit(1, 2, new PathfinderAttributes(3.1f, "default")));
        //
        //     // Overlap with a corner
        //     Assert.IsTrue(sut.PathfinderCanFit(2, 8, new PathfinderAttributes(0.9f, "default")));
        //     Assert.IsTrue(sut.PathfinderCanFit(2, 8, new PathfinderAttributes(_sub2Sqrt2, "default")));
        //     Assert.IsFalse(sut.PathfinderCanFit(2, 8, new PathfinderAttributes(_sup2Sqrt2, "default")));
        // }
        //
        // [TestMethod]
        // public void TestPathfinderCanFit_SelectivelyCalledWhenBlockageUpdated()
        // {
        //     sut = new GridPlaceGraph(true,new HashSet<float>{0.9f, _sup2Sqrt2});
        //     sut.BuildFromArray(gridTerrainCosts);
        //     SetupBlockagesFromTerrainCosts(sut);
        //
        //     // Initially, a collision
        //     Assert.IsFalse(sut.PathfinderCanFit(2, 8, new PathfinderAttributes(_sup2Sqrt2, "default")));
        //
        //     sut.SetBlockage("default", (3, 7), false);
        //
        //     // No more collision
        //     Assert.IsTrue(sut.PathfinderCanFit(2, 8, new PathfinderAttributes(_sup2Sqrt2, "default")));
        // }
        //
        // [TestMethod]
        // public void TestPathfinderCanFit_FitsWhenSizeAndGapAreEqualAndEven()
        // {
        //     sut = new GridPlaceGraph(true, new HashSet<float>{0.9f, 1.9f});  
        //     sut.BuildFromArray(gridTerrainCosts);
        //     SetupBlockagesFromTerrainCosts(sut);
        //
        //     // Size 2 pathfinder can fit on either of the cells in a 2-width tunnel (by standing in the middle)
        //     // The results for PathfinderFitsCoords are deterministic the ordering of GRID_CORNER_DELTAS
        //     PathfinderAttributes attrs = new(1.9f, "default");
        //     Assert.IsTrue(sut.PathfinderCanFit(4, 8, attrs));
        //     Assert.IsTrue(sut.PathfinderFitsCoords(4, 8, attrs).CornersFarthestFromBlockages.Contains((4.5f, 8.5f)));
        //     Assert.IsTrue(sut.PathfinderCanFit(4, 9, attrs));
        //     Assert.IsTrue(sut.PathfinderFitsCoords(4, 9, attrs).CornersFarthestFromBlockages.Contains((3.5f, 8.5f)));
        //     Assert.IsTrue(sut.PathfinderCanFit(5, 8, attrs));
        //     Assert.IsTrue(sut.PathfinderFitsCoords(5, 8, attrs).CornersFarthestFromBlockages.Contains((4.5f, 8.5f)));
        //     Assert.IsTrue(sut.PathfinderCanFit(5, 9, attrs));
        //     Assert.IsTrue(sut.PathfinderFitsCoords(5, 9, attrs).CornersFarthestFromBlockages.Contains((4.5f, 8.5f)));
        // }

        private static void AssertThrowsException<T>(Action action) where T : Exception
        {
            Assert.ThrowsException<T>(action);
        }

        [TestMethod]
        public void TestSmoothPathAroundBlockages()
        {
            float pathfinderSize = 0.9f;
            sut = new GridPlaceGraph(true, new HashSet<float>{pathfinderSize}); 
            sut.BuildFromFile("../../../Resources/excel_mazes/walls_test.csv"); 
            SetupBlockagesFromTerrainCosts(sut);

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
                (0.5f, 1.5f),
                (1.5f, 8.5f),
                (21.5f, 8.5f),
                (22f, 10f),
                (22f, 11f),
                (21.5f, 12.5f),
                (19f, 13f)
            };
            // List<(float, float)> expectedSmoothPath = new()
            // {
            //     (0f, 2f), (2f, 8f), (22f, 9f), (22f, 12f), (19f, 13f)
            // };

            PathfinderAttributes attrs = new(pathfinderSize, "default");        
            List<(float, float)> occupiablePath = sut.GetOccupiablePath(originalPath, attrs);
            List<(float, float)> actualSmoothPath = sut.SmoothPath(occupiablePath, originalPath, attrs);

            CollectionAssert.AreEqual(expectedSmoothPath, actualSmoothPath);    
        }

        [TestMethod]
        public void TestSmoothPathAroundBlockages2()
        {
            float pathfinderSize = 1.9f;
            sut = new GridPlaceGraph(true, new HashSet<float>{pathfinderSize}); 
            sut.BuildFromFile("../../../Resources/excel_mazes/walls_test.csv"); 
            SetupBlockagesFromTerrainCosts(sut);

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

            PathfinderAttributes attrs = new(pathfinderSize, "default");        
            List<(float, float)> occupiablePath = sut.GetOccupiablePath(originalPath, attrs);
            List<(float, float)> actualSmoothPath = sut.SmoothPath(occupiablePath, originalPath, attrs);

            CollectionAssert.AreEqual(expectedSmoothPath, actualSmoothPath);    
        }

        [TestMethod]
        public void TestSmoothPathAroundSwamps()
        {
            float pathfinderSize = 0.9f;
            sut = new GridPlaceGraph(true, new HashSet<float>{pathfinderSize}); 
            sut.BuildFromFile("../../../Resources/excel_mazes/walls_and_swamps_test.csv");
            SetupBlockagesFromTerrainCosts(sut);

            List<GridPlace> originalPath = new()
            {
                new GridPlace((4, 4)), new GridPlace((5, 5)), new GridPlace((6, 5)), new GridPlace((7, 5)),
                new GridPlace((8, 5)), new GridPlace((9, 6)), new GridPlace((9, 7)), new GridPlace((8, 8))
            };

            // List<(float, float)> expectedSmoothPath = new()
            // {
            //     (4f, 4f), (8f, 5f), (9f, 6f), (8f, 8f)
            // };
            List<(float, float)> expectedSmoothPath = new()
            {
                (4f, 4f), (8.5f, 5.5f), (8f, 8f)
            };

            PathfinderAttributes attrs = new(pathfinderSize, "default");        

            List<(float, float)> occupiablePath = sut.GetOccupiablePath(originalPath, attrs);
            List<(float, float)> actualSmoothPath = sut.SmoothPath(occupiablePath, originalPath, attrs);

            CollectionAssert.AreEqual(expectedSmoothPath, actualSmoothPath);    
        }
    }
}
