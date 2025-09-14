using System;
using System.Collections;
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

        public static IEnumerable<object[]> GetTestData()
        {
            yield return new object[] { "spiral_test.csv", (0, 0), (9, 9), 49, true };
            yield return new object[] { "spiral_test.csv", (0, 0), (9, 9), 59, false };
            yield return new object[] { "spiral_hole1_test.csv", (0, 0), (9, 9), 19, true };
            yield return new object[] { "spiral_hole1_test.csv", (0, 0), (9, 9), 23, false };
            yield return new object[] { "spiral_hole2_test.csv", (0, 0), (9, 9), 44, true };
            yield return new object[] { "spiral_hole2_test.csv", (0, 0), (9, 9), 53, false };
            yield return new object[] { "spiral_hole3_test.csv", (0, 0), (9, 9), 35, true };
            yield return new object[] { "spiral_hole3_test.csv", (0, 0), (9, 9), 41, false };
            yield return new object[] { "walls_test.csv", (7, 12), (26, 15), 36, true };
            yield return new object[] { "walls_test.csv", (7, 12), (26, 15), 41, false };
            yield return new object[] { "walls_and_swamps_test.csv", (4, 1), (6, 7), 10, true };
            yield return new object[] { "walls_and_swamps_test.csv", (4, 1), (6, 7), 17, false };
        }

        [DataTestMethod]
        [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
        public void TestFindsShortestPathIncludingWallsAndSwamps(string mazeFile, (int, int) start, (int, int) target,
            int expectedPathLength, bool diagonalNeighbours)
        {
            GridPlaceGraph graph = new(diagonalNeighbours);
            graph.Build($"../../../Resources/excel_mazes/{mazeFile}");
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            var startPlace = (GridPlace)graph.Places[start];
            var targetPlace = (GridPlace)graph.Places[target];

            _sut.Solve(startPlace, targetPlace);
            List<GridPlace> actual = _sut.ReconstructPath(startPlace, targetPlace).ToList();

            Assert.AreEqual(expectedPathLength, actual.Count);
        }

        [TestMethod]
        public void TestNullPathIfNotYetRun()
        {
            GridPlaceGraph graph = new GridPlaceGraph(false);
            graph.Build("../../../Resources/excel_mazes/spiral_test.csv");
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            var startPlace = (GridPlace)graph.Places[(0, 0)];
            var targetPlace = (GridPlace)graph.Places[(9, 9)];
            Assert.IsNull(_sut.ReconstructPath(startPlace, targetPlace));
        }

        [TestMethod]
        public void TestExceptionIfGraphDisjoint()
        {
            GridPlaceGraph graph = new GridPlaceGraph(false);
            GridPlace A = new GridPlace((0, 0));
            GridPlace B = new GridPlace((0, 2));
            graph.Places.Add((0, 0), A);
            graph.Places.Add((0, 2), B);
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => _sut.Solve(A, B),
                "Cannot support a disjoint Graph!");
        }

        [TestMethod]
        public void TestExceptionIfStartNotOnGraph()
        {
            GridPlaceGraph graph = new GridPlaceGraph(false);
            graph.Build("../../../Resources/excel_mazes/spiral_test.csv");
            GridPlace notOnGraph = new GridPlace((200, 200));
            var targetPlace = (GridPlace)graph.Places[(9, 9)];
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => _sut.Solve(notOnGraph, targetPlace),
                "The start place ((200, 200)) is not on the graph!");
            Assert.IsNull(_sut.ReconstructPath(notOnGraph, targetPlace));
        }

        [TestMethod]
        public void TestExceptionIfTargetNotOnGraph()
        {
            GridPlaceGraph graph = new GridPlaceGraph(false);
            graph.Build("../../../Resources/excel_mazes/spiral_test.csv");
            GridPlace notOnGraph = new GridPlace((200, 200));
            var startPlace = (GridPlace)graph.Places[(0, 0)];
            _sut = new AStarSolver<GridPlace, (int, int)>(graph);
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => _sut.Solve(startPlace, notOnGraph),
                $"The target place (\"(200, 200)\") is not on the graph!");
            Assert.IsNull(_sut.ReconstructPath(startPlace, notOnGraph));
        }
    }
}