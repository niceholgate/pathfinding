using System.Collections.Generic;
using System.IO;
using System.Linq;
using AStarNickNS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NicUtils;

namespace AStarTests {
    [TestClass]
    public class DijkstraSolverTests {

        // private static GenericPlaceGraph _genericPlaceGraph = new();
        // private readonly GenericPlace A = new("A", _genericPlaceGraph);
        // private readonly GenericPlace B = new("B", _genericPlaceGraph);
        // private readonly GenericPlace C = new("C", _genericPlaceGraph);
        // private readonly GenericPlace D = new("D", _genericPlaceGraph);
        // private readonly GenericPlace E = new("E", _genericPlaceGraph);
        // private readonly GenericPlace F = new("F", _genericPlaceGraph);
        // private readonly GenericPlace G = new("G", _genericPlaceGraph);

        // /*               E -3- F -3- G (END)
        //  *               |
        //  *               2
        //  *               |
        //  * A (START) -2- B -5- C -2- D 
        //  * 
        //  * A->B->E->F->G takes 10 (should pick this path)
        //  * A->B->C->D->G takes 11
        //  */
        // [TestInitialize]
        // public void TestFixtureSetup() {
        //     A.ExplicitNeighboursWithCosts.Add(B, 2.0);
        //     B.ExplicitNeighboursWithCosts.Add(A, 2.0);
        //     B.ExplicitNeighboursWithCosts.Add(C, 5.0);
        //     B.ExplicitNeighboursWithCosts.Add(E, 2.0);
        //     C.ExplicitNeighboursWithCosts.Add(B, 5.0);
        //     C.ExplicitNeighboursWithCosts.Add(D, 2.0);
        //     D.ExplicitNeighboursWithCosts.Add(C, 2.0);
        //     D.ExplicitNeighboursWithCosts.Add(G, 2.0);
        //     E.ExplicitNeighboursWithCosts.Add(B, 2.0);
        //     E.ExplicitNeighboursWithCosts.Add(F, 3.0);
        //     F.ExplicitNeighboursWithCosts.Add(E, 3.0);
        //     F.ExplicitNeighboursWithCosts.Add(G, 3.0);
        //     G.ExplicitNeighboursWithCosts.Add(D, 2.0);
        //     G.ExplicitNeighboursWithCosts.Add(F, 3.0);
        //     _sut = new(A, G);
        // }
        
        // [TestMethod]
        // public void TestFindsBestPath() {
        //    
        //     string[] expectedOptimalPathLabels = new string[5] { "A", "B", "E", "F", "G" };
        //
        //     _sut.Solve();
        //     IEnumerable<IPlace<string>> optimalPath = _sut.ReconstructPath();
        //     //IEnumerable<GenericPlace> optimalPath = _sut.ReconstructPath();
        //     List<String> optimalPathLabels = optimalPath.Select(place => place.Label).ToList();
        //     Assert.IsTrue(optimalPathLabels.SequenceEqual(expectedOptimalPathLabels));
        // }
        //
        // [TestMethod]
        // public void TestNullPathIfNoSolutionFound() {
        //     /*               E     F -3- G (END)
        //         *               |
        //         *               2
        //         *               |
        //         * A (START) -2- B -5- C     D 
        //         * 
        //         * There should be no path
        //         */
        //     C.ExplicitNeighboursWithCosts.Remove(D);
        //     D.ExplicitNeighboursWithCosts.Remove(C);
        //     E.ExplicitNeighboursWithCosts.Remove(F);
        //     F.ExplicitNeighboursWithCosts.Remove(E);
        //
        //     _sut.Solve();
        //     Assert.IsNull(_sut.ReconstructPath());
        // }

        
        private DijkstraSolver<GenericPlace, string> _sutGenericPlace;
        
        private DijkstraSolver<GridPlace, (int, int)> _sutGridPlace;
        
        [TestMethod]
        public void TestFindsShortestPath()
        {
            GenericPlaceGraph graph = new GenericPlaceGraph();
            graph.BuildFromFile("../../../Resources/mermaid_networks/net1.mmd");
            _sutGenericPlace = new DijkstraSolver<GenericPlace, string>(graph);
            var startPlace = (GenericPlace)graph.Places["A"];
            var targetPlace = (GenericPlace)graph.Places["G"];

            List<IPlace<string>> expected = new List<IPlace<string>>
            {
                graph.Places["A"],
                graph.Places["C"],
                graph.Places["B"],
                graph.Places["E"],
                graph.Places["F"],
                graph.Places["G"],
            };
            
            List<GenericPlace> actual = _sutGenericPlace.SolvePath(startPlace, targetPlace).ToList();
            
            CollectionAssert.AreEqual(expected, actual);
        }
        
        // [TestMethod]
        // public void TestFindsShortestPath2()
        // {
        //     GenericPlaceGraph graph = new GenericPlaceGraph();
        //     graph.BuildFromFile("../../../Resources/mermaid_networks/net1.mmd");
        //     _sut = new DijkstraSolver<GenericPlace, string>(graph);
        //     var startPlace = (GenericPlace)graph.Places["A"];
        //     var targetPlace = (GenericPlace)graph.Places["G"];
        //
        //     List<IPlace<string>> expected = new List<IPlace<string>>
        //     {
        //         graph.Places["A"],
        //         graph.Places["C"],
        //         graph.Places["B"],
        //         graph.Places["E"],
        //         graph.Places["F"],
        //         graph.Places["G"],
        //     };
        //     
        //     List<GenericPlace> actual = _sut.SolvePath(startPlace, targetPlace).ToList();
        //     
        //     CollectionAssert.AreEqual(expected, actual);
        // }
        
        public static IEnumerable<object[]> PathfinderTestData()
        {
            yield return new object[] { "spiral_test.csv", (0, 0), (9, 9), 48, true };
            yield return new object[] { "spiral_test.csv", (0, 0), (9, 9), 58, false };
            yield return new object[] { "spiral_hole1_test.csv", (0, 0), (9, 9), 18, true };
            yield return new object[] { "spiral_hole1_test.csv", (0, 0), (9, 9), 22, false };
            yield return new object[] { "spiral_hole2_test.csv", (0, 0), (9, 9), 43, true };
            yield return new object[] { "spiral_hole2_test.csv", (0, 0), (9, 9), 52, false };
            yield return new object[] { "spiral_hole3_test.csv", (0, 0), (9, 9), 34, true };
            yield return new object[] { "spiral_hole3_test.csv", (0, 0), (9, 9), 40, false };
            yield return new object[] { "walls_test.csv", (7, 12), (26, 15), 35, true };
            yield return new object[] { "walls_test.csv", (7, 12), (26, 15), 40, false };
            yield return new object[] { "walls_and_swamps_test.csv", (4, 1), (6, 7), 13, true };
            yield return new object[] { "walls_and_swamps_test.csv", (4, 1), (6, 7), 18, false };
        }

        [DataTestMethod]
        [DynamicData(nameof(PathfinderTestData), DynamicDataSourceType.Method)]
        public void TestFindsShortestPathIncludingWallsAndSwamps(string mazeFile, (int, int) start, (int, int) target,
            double expectedPathCost, bool diagonalNeighbours)
        {
            GridPlaceGraph graph = new(diagonalNeighbours, new PathfinderObstacleIntersector());
            graph.BuildFromFile($"../../../Resources/excel_mazes/{mazeFile}");
            _sutGridPlace = new DijkstraSolver<GridPlace, (int, int)>(graph);
            var startPlace = (GridPlace)graph.Places[start];
            var targetPlace = (GridPlace)graph.Places[target];

            List<GridPlace> path = _sutGridPlace.SolvePath(startPlace, targetPlace).ToList();

            Assert.AreEqual(expectedPathCost, graph.GetPathCost(path.Select(place => place.Label).ToList()));
        }
        
        [TestMethod]
        public void TestExceptionIfGraphDisjoint()
        {
            GenericPlaceGraph graph = new GenericPlaceGraph();
            GenericPlace A = new GenericPlace("A");
            GenericPlace B = new GenericPlace("B");
            graph.Places.Add("A", A);
            graph.Places.Add("B", B);
            _sutGenericPlace = new DijkstraSolver<GenericPlace, string>(graph);
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => _sutGenericPlace.SolvePath(A, B),
                "Cannot support a disjoint Graph!");
        }
        
        [TestMethod]
        public void TestExceptionIfStartNotOnGraph()
        {
            GenericPlaceGraph graph = new GenericPlaceGraph();
            graph.BuildFromFile("../../../Resources/mermaid_networks/net1.mmd");
            GenericPlace notOnGraph = new GenericPlace("H");
            var targetPlace = (GenericPlace)graph.Places["G"];
            _sutGenericPlace = new DijkstraSolver<GenericPlace, string>(graph);
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => _sutGenericPlace.SolvePath(notOnGraph, targetPlace),
                "The start place (\"H\") is not on the graph!");
        }
        
        [TestMethod]
        public void TestExceptionIfTargetNotOnGraph()
        {
            GenericPlaceGraph graph = new GenericPlaceGraph();
            graph.BuildFromFile("../../../Resources/mermaid_networks/net1.mmd");
            GenericPlace notOnGraph = new GenericPlace("H");
            var startPlace = (GenericPlace)graph.Places["A"];
            _sutGenericPlace = new DijkstraSolver<GenericPlace, string>(graph);
            TestHelpers.AssertThrowsExceptionWithMessage<IOException>(
                () => _sutGenericPlace.SolvePath(startPlace, notOnGraph),
                "The target place (\"H\") is not on the graph!");
        }
    }
}
