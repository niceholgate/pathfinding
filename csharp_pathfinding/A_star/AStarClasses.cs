// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection.Emit;
// using System.Text;
// using FibonacciHeap;
// using NicUtils;
//
// // https://stackoverflow.com/questions/150479/order-of-items-in-classes-fields-properties-constructors-methods
// namespace AStarNickNS
// {
//     // GRAPHS - link PLACES together according to a shared geometrical concept
//
//     // PLACES - discrete locations within a geometrical concept (coordinate type)
//
//     // SOLVERS - find shortest distance between two discrete locations on a graph
//
//
//     // TODO: can the generics and interfaces be improved?
//     // TODO: catch path not found appropriately and test
//     // TODO: should not refer to ExplicitNeighbours directly - then can remove from interface
//
//
//     //interface IDijkstraSolver<out IPlace> {
//     //    IEnumerable<IPlace> ReconstructPath();
//     //}
//
//
//     //abstract class Node<CoordType> : INode<CoordType> where CoordType : IPlace {
//     //    public Node(CoordType coord) { this.Coord = coord; }
//
//     //    public CoordType Coord { get; }
//
//     //    public IDictionary<INode<CoordType>, float> GetNeighboursCosts() { return this.neighboursCosts; }
//
//     //    public virtual float GetCostToLeave(INode neighbour) { return neighboursCosts[neighbour]; }
//     //}
//
//     //public class GenericNode : INode<GenericPlace> {
//     //    public GenericPlace Coord { get; }
//
//     //    private Dictionary<INode, float> costsToReachExplicitNeighbours = new Dictionary<INode, float>();
//
//     //    public GenericNode(GenericPlace coord) {
//     //        Coord = coord;
//     //        foreach (IPlace neighbour in Coord.ExplicitNeighbours) {
//     //            costsToReachExplicitNeighbours[neighbour] = 10;
//     //        }
//     //    }
//
//     //    public float GetCostToLeave(INode neighbour) {
//     //        return costsToReachExplicitNeighbours[neighbour];
//     //    }
//
//
//     //}
//
//     //public class Square : Node<GridCoords2D>, INodeAStar<GridCoords2D> {
//     //    public static readonly float SQRT2 = MathF.Sqrt(2.0f);
//
//     //    public Square(GridCoords2D coord) : base(coord) { }
//
//     //    public float GetCostToLeave(Square neighbour) {
//     //        float distance = neighbour.IsDiagonalNeighbour(this) ? SQRT2 : 1.0f;
//     //        return neighboursCosts[neighbour] * distance;
//     //    }
//
//     //    private bool IsDiagonalNeighbour(Square other) { return Coord.IsDiagonalNeighbour(other.Coord); }
//
//     //    public float GetHeuristicDist(INodeAStar<GridCoords2D> other, Distances2D.HeuristicType heuristicType) {
//     //        float[] thisLabelAsFloats = { Coord.Label.Item1 , Coord.Label.Item2 };
//     //        float[] otherLabelAsFloats = { other.Coord.Label.Item1, other.Coord.Label.Item2 };
//     //        return (float)Distances2D.GetDistance(thisLabelAsFloats, otherLabelAsFloats, heuristicType);
//     //    }
//     //}
//
//
//     //class Hex : Node<HexCoords2D>, INodeAStar<HexCoords2D>
//     //{
//     //    interface_method(Hex hex)
//     //}
//
//     //class A_star {
//     //    static void Main(string[] args) {
//     //        var heap = new FibonacciHeap<string, int>(0);
//     //        heap.Insert(new FibonacciHeapNode<string, int>("hello", 5));
//     //        FibonacciHeapNode<string, int> min = heap.Min();
//     //        //List<int[]> listInt = new List<int[]> { new int[] { 1, 2 }, new int[] { 0, -1 }, new int[] { 0, 1 }, new int[] { 1, 0 } };
//     //        //int[] searchArrayInt = { 1, 2 };
//
//     //        //List<float[]> listFloat = new List<float[]> { new float[] { 1, 2 }, new float[] { 0, -1 }, new float[] { 0, 1 }, new float[] { 1, 0 } };
//     //        //float[] searchArrayFloat = { 1.1f, 2.0f };
//
//     //        //List<int>[] arrayInt = new List<int>[] { new List<int> { 1, 2 }, new List<int> { 0, -1 }, new List<int> { 0, 1 }, new List<int> { 1, 0 } };
//     //        //List<int> searchListInt = new List<int> { 1, 2 };
//
//     //        //bool cont = Enumerables<int>.ContainsEnumerable(listInt, searchArrayInt);
//     //        //bool cont2 = Enumerables<float>.ContainsEnumerable(listFloat, searchArrayFloat);
//     //        //bool cont3 = Enumerables<int>.ContainsEnumerable(arrayInt, searchListInt);
//
//     //        //float d = (float)Distances2D.GetDistance(new float[2] { 3.0f, 1.0f }, new float[2] { 6.0f, 5.0f }, Distances2D.HeuristicType.Euclidian);
//     //        //Console.WriteLine(d);
//
//     //        //Square squareTest1 = new Square(new GridCoords2D(0, 0));
//     //        //Square squareTestDiag = new Square(new GridCoords2D(1, 1));
//     //        //Square squareTestStra = new Square(new GridCoords2D(1, 0));
//     //        //float hd = squareTest1.GetHeuristicDist(squareTestDiag, Distances2D.HeuristicType.Euclidian);
//
//     //        //var costsDict = new Dictionary<INode<GridCoords2D>, float>();
//     //        //costsDict.Add(squareTestDiag, 1);
//     //        //costsDict.Add(squareTestStra, 1);
//     //        //var costsDict2 = new Dictionary<INode<GridCoords2D>, float> { { squareTestDiag, 2 }, { squareTestStra, 4 } };
//
//     //        //squareTest1.neighboursCosts = costsDict;
//     //        //float costDiag = squareTest1.GetCostToLeave(squareTestDiag);
//     //        //float costStra = squareTest1.GetCostToLeave(squareTestStra);
//
//     //        //Console.WriteLine(hd);
//     //    }
//     //}
// }