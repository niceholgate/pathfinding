using System.Collections.Generic;
using System.IO;
using System.Linq;
using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;

namespace AStarTests
{
    [TestClass]
    public class AStarSolverTests : DijkstraSolverTests
    {
        private AStarSolver<GridPlace, (int, int)> _sut;

        [DataTestMethod]
        [DynamicData(nameof(PathfinderTestData), DynamicDataSourceType.Method)]
        public override void TestFindsShortestPathGridPlaceGraph(string mazeFile, (int, int) start, (int, int) target,
            double expectedPathCost, bool diagonalNeighbours, double pathfinderSize)
        {
            GridPlaceGraph graph = new(
                diagonalNeighbours,
                new PathfinderObstacleIntersector(),
                new HashSet<double>{pathfinderSize});
            graph.BuildFromFile($"../../../Resources/excel_mazes/{mazeFile}");
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            var startPlace = (GridPlace)graph.Places[start];
            var targetPlace = (GridPlace)graph.Places[target];

            List<GridPlace> path = _sut.SolvePath(startPlace, targetPlace, pathfinderSize).ToList();

            TestHelpers.AssertEqualWithinTolerance(
                expectedPathCost,
                graph.GetPathCost(path.Select(place => place.Label).ToList()),
                0.01);
        }

        [TestMethod]
        public override void TestExceptionIfGraphDisjoint()
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
        public override void TestExceptionIfStartNotOnGraph()
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
        public override void TestExceptionIfTargetNotOnGraph()
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