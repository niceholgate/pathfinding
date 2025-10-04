using System.Collections.Generic;
using System.IO;
using System.Linq;
using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;

namespace AStarTests
{
    [TestClass]
    public class AStarSolverTests
    {
        private AStarSolver<GridPlace, (int, int)> _sut;

        public static IEnumerable<object[]> PathfinderTestData()
        {
            // Test various mazes and diagonality
            yield return new object[] { "spiral_test.csv", (0, 0), (9, 9), 52.14, true };
            yield return new object[] { "spiral_test.csv", (0, 0), (9, 9), 58, false };
            yield return new object[] { "spiral_hole1_test.csv", (0, 0), (9, 9), 19.66, true };
            yield return new object[] { "spiral_hole1_test.csv", (0, 0), (9, 9), 22, false };
            yield return new object[] { "spiral_hole2_test.csv", (0, 0), (9, 9), 46.73, true };
            yield return new object[] { "spiral_hole2_test.csv", (0, 0), (9, 9), 52, false };
            yield return new object[] { "spiral_hole3_test.csv", (0, 0), (9, 9), 36.49, true };
            yield return new object[] { "spiral_hole3_test.csv", (0, 0), (9, 9), 40, false };
            yield return new object[] { "walls_test.csv", (7, 12), (26, 15), 37.07, true };
            yield return new object[] { "walls_test.csv", (7, 12), (26, 15), 40, false };
            yield return new object[] { "walls_and_swamps_test.csv", (4, 1), (6, 7), 14.24, true };
            yield return new object[] { "walls_and_swamps_test.csv", (4, 1), (6, 7), 18, false };
        }

        [DataTestMethod]
        [DynamicData(nameof(PathfinderTestData), DynamicDataSourceType.Method)]
        public void TestFindsShortestPathIncludingWallsAndSwamps(string mazeFile, (int, int) start, (int, int) target,
            double expectedPathCost, bool diagonalNeighbours)
        {
            GridPlaceGraph graph = new(diagonalNeighbours, new PathfinderObstacleIntersector());
            graph.BuildFromFile($"../../../Resources/excel_mazes/{mazeFile}");
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            var startPlace = (GridPlace)graph.Places[start];
            var targetPlace = (GridPlace)graph.Places[target];

            List<GridPlace> path = _sut.SolvePath(startPlace, targetPlace).ToList();

            TestHelpers.AssertEqualWithinTolerance(
                expectedPathCost,
                graph.GetPathCost(path.Select(place => place.Label).ToList()),
                0.01);
        }

        [TestMethod]
        public void TestExceptionIfGraphDisjoint()
        {
            GridPlaceGraph graph = new GridPlaceGraph(false, new PathfinderObstacleIntersector());
            GridPlace A = new GridPlace((0, 0));
            GridPlace B = new GridPlace((0, 2));
            graph.Places.Add((0, 0), A);
            graph.Places.Add((0, 2), B);
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => _sut.SolvePath(A, B),
                "Cannot support a disjoint Graph!");
        }

        [TestMethod]
        public void TestExceptionIfStartNotOnGraph()
        {
            GridPlaceGraph graph = new GridPlaceGraph(false, new PathfinderObstacleIntersector());
            graph.BuildFromFile("../../../Resources/excel_mazes/spiral_test.csv");
            GridPlace notOnGraph = new GridPlace((200, 200));
            var targetPlace = (GridPlace)graph.Places[(9, 9)];
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => _sut.SolvePath(notOnGraph, targetPlace),
                "The start place (\"(200, 200)\") is not on the graph!");
        }

        [TestMethod]
        public void TestExceptionIfTargetNotOnGraph()
        {
            GridPlaceGraph graph = new GridPlaceGraph(false, new PathfinderObstacleIntersector());
            graph.BuildFromFile("../../../Resources/excel_mazes/spiral_test.csv");
            GridPlace notOnGraph = new GridPlace((200, 200));
            var startPlace = (GridPlace)graph.Places[(0, 0)];
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => _sut.SolvePath(startPlace, notOnGraph),
                $"The target place (\"(200, 200)\") is not on the graph!");
        }
    }
}