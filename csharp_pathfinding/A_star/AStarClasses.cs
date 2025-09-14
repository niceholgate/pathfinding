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
//     //    public IDictionary<INode<CoordType>, double> GetNeighboursCosts() { return this.neighboursCosts; }
//
//     //    public virtual double GetCostToLeave(INode neighbour) { return neighboursCosts[neighbour]; }
//     //}
//
//     //public class GenericNode : INode<GenericPlace> {
//     //    public GenericPlace Coord { get; }
//
//     //    private Dictionary<INode, double> costsToReachExplicitNeighbours = new Dictionary<INode, double>();
//
//     //    public GenericNode(GenericPlace coord) {
//     //        Coord = coord;
//     //        foreach (IPlace neighbour in Coord.ExplicitNeighbours) {
//     //            costsToReachExplicitNeighbours[neighbour] = 10;
//     //        }
//     //    }
//
//     //    public double GetCostToLeave(INode neighbour) {
//     //        return costsToReachExplicitNeighbours[neighbour];
//     //    }
//
//
//     //}
//
//     //public class Square : Node<GridCoords2D>, INodeAStar<GridCoords2D> {
//     //    public static readonly double SQRT2 = Math.Sqrt(2.0);
//
//     //    public Square(GridCoords2D coord) : base(coord) { }
//
//     //    public double GetCostToLeave(Square neighbour) {
//     //        double distance = neighbour.IsDiagonalNeighbour(this) ? SQRT2 : 1.0;
//     //        return neighboursCosts[neighbour] * distance;
//     //    }
//
//     //    private bool IsDiagonalNeighbour(Square other) { return Coord.IsDiagonalNeighbour(other.Coord); }
//
//     //    public double GetHeuristicDist(INodeAStar<GridCoords2D> other, Distances2D.HeuristicType heuristicType) {
//     //        double[] thisLabelAsDoubles = { Coord.Label.Item1 , Coord.Label.Item2 };
//     //        double[] otherLabelAsDoubles = { other.Coord.Label.Item1, other.Coord.Label.Item2 };
//     //        return Distances2D.GetDistance(thisLabelAsDoubles, otherLabelAsDoubles, heuristicType);
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
//     //        //List<double[]> listDouble = new List<double[]> { new double[] { 1, 2 }, new double[] { 0, -1 }, new double[] { 0, 1 }, new double[] { 1, 0 } };
//     //        //double[] searchArrayDouble = { 1.1, 2.0 };
//
//     //        //List<int>[] arrayInt = new List<int>[] { new List<int> { 1, 2 }, new List<int> { 0, -1 }, new List<int> { 0, 1 }, new List<int> { 1, 0 } };
//     //        //List<int> searchListInt = new List<int> { 1, 2 };
//
//     //        //bool cont = Enumerables<int>.ContainsEnumerable(listInt, searchArrayInt);
//     //        //bool cont2 = Enumerables<double>.ContainsEnumerable(listDouble, searchArrayDouble);
//     //        //bool cont3 = Enumerables<int>.ContainsEnumerable(arrayInt, searchListInt);
//
//     //        //double d = Distances2D.GetDistance(new double[2] { 3.0, 1.0 }, new double[2] { 6.0, 5.0 }, Distances2D.HeuristicType.Euclidian);
//     //        //Console.WriteLine(d);
//
//     //        //Square squareTest1 = new Square(new GridCoords2D(0, 0));
//     //        //Square squareTestDiag = new Square(new GridCoords2D(1, 1));
//     //        //Square squareTestStra = new Square(new GridCoords2D(1, 0));
//     //        //double hd = squareTest1.GetHeuristicDist(squareTestDiag, Distances2D.HeuristicType.Euclidian);
//
//     //        //var costsDict = new Dictionary<INode<GridCoords2D>, double>();
//     //        //costsDict.Add(squareTestDiag, 1);
//     //        //costsDict.Add(squareTestStra, 1);
//     //        //var costsDict2 = new Dictionary<INode<GridCoords2D>, double> { { squareTestDiag, 2 }, { squareTestStra, 4 } };
//
//     //        //squareTest1.neighboursCosts = costsDict;
//     //        //double costDiag = squareTest1.GetCostToLeave(squareTestDiag);
//     //        //double costStra = squareTest1.GetCostToLeave(squareTestStra);
//
//     //        //Console.WriteLine(hd);
//     //    }
//     //}
// }