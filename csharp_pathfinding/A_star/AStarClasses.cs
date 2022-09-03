using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using NicUtils;

namespace AStarNickNS
{

    //public abstract class Place<LabelType> {
    //    LabelType Label { get; }

    //    bool IsNeighbour(IPlace<LabelType> other);
    //}

    //////////// INTERFACES

    public interface IPlace {

    }

    /*
     * A place
     */
    public interface IPlace<LabelType> : IPlace {
        LabelType Label { get; }

        bool IsNeighbour(IPlace<LabelType> other);
    }

    public interface INode {
        double GetCostToLeave(INode neighbour);
    }

    /*
     * A node
     */
    public interface INode<CoordType> : INode where CoordType : IPlace {
        CoordType Coord { get; }
    }

    /*
     * An AStar node
     */
    public interface INodeAStar<CoordType> : INode<CoordType> where CoordType : IPlace {
        double GetHeuristicDist(INodeAStar<CoordType> other, Distances2D.HeuristicType heuristicType);
    }

    /*
     * A place with no defining geometry, just explicitly specified neighbours
     */
    public struct GenericPlace : IPlace<string> {
        public GenericPlace(string label, HashSet<IPlace<string>> neighbours) {
            Label = label;
            Neighbours = neighbours;
        }

        public string Label { get; }

        public bool IsNeighbour(IPlace<string> other) { return Neighbours.Contains(other); }

        public HashSet<IPlace<string>> Neighbours { get; }

        public override string ToString() => Label;
    }

    /*
     * A place on a 2D grid
     */
    public struct GridCoords2D : IPlace<(int, int)> {
        public GridCoords2D(int x, int y) { Label = (x, y); }

        public (int, int) Label { get; }

        public bool IsDiagonalNeighbour(IPlace<(int, int)> other) {
            int[] delta = DeltaFrom(other);
            return delta[0] * delta[0] + delta[1] * delta[1] == 2;
        }

        public bool IsStraightNeighbour(IPlace<(int, int)> other) {
            int[] delta = DeltaFrom(other);
            return Math.Abs(delta[0]) + Math.Abs(delta[1]) == 1;
        }

        public bool IsNeighbour(IPlace<(int, int)> other) { return IsDiagonalNeighbour(other) || IsStraightNeighbour(other); }

        public int[] DeltaFrom(IPlace<(int, int)> other) { return new int[] { this.Label.Item1 - other.Label.Item1, this.Label.Item2 - other.Label.Item2 }; }

        public override string ToString() => Label.ToString();
    }

    abstract class Node<CoordType> : INode<CoordType> where CoordType : IPlace {
        public Node(CoordType coord) { this.Coord = coord; }

        public CoordType Coord { get; }

        public IDictionary<INode<CoordType>, double> GetNeighboursCosts() { return this.neighboursCosts; }

        public virtual double GetCostToLeave(INode neighbour) { return neighboursCosts[neighbour]; }
    }

    public class Square : Node<GridCoords2D>, INodeAStar<GridCoords2D> {
        public static readonly double SQRT2 = Math.Sqrt(2.0);

        public Square(GridCoords2D coord) : base(coord) { }

        public double GetCostToLeave(Square neighbour) {
            double distance = neighbour.IsDiagonalNeighbour(this) ? SQRT2 : 1.0;
            return neighboursCosts[neighbour] * distance;
        }

        private bool IsDiagonalNeighbour(Square other) { return Coord.IsDiagonalNeighbour(other.Coord); }

        public double GetHeuristicDist(INodeAStar<GridCoords2D> other, Distances2D.HeuristicType heuristicType) {
            double[] thisLabelAsDoubles = { Coord.Label.Item1 , Coord.Label.Item2 };
            double[] otherLabelAsDoubles = { other.Coord.Label.Item1, other.Coord.Label.Item2 };
            return Distances2D.GetDistance(thisLabelAsDoubles, otherLabelAsDoubles, heuristicType);
        }

    }
    //class Hex : Node<HexCoords2D>, INodeAStar<HexCoords2D>
    //{
    //    interface_method(Hex hex)
    //}

    //class A_star
    //{
    //    static void Main(string[] args)
    //    {
    //        List<int[]> listInt = new List<int[]> { new int[] { 1, 2 }, new int[] { 0, -1 }, new int[] { 0, 1 }, new int[] { 1, 0 } };
    //        int[] searchArrayInt = { 1, 2 };

    //        List<double[]> listDouble = new List<double[]> { new double[] { 1, 2 }, new double[] { 0, -1 }, new double[] { 0, 1 }, new double[] { 1, 0 } };
    //        double[] searchArrayDouble = { 1.1, 2.0 };

    //        List<int>[] arrayInt = new List<int>[] { new List<int> { 1, 2 }, new List<int> { 0, -1 }, new List<int> { 0, 1 }, new List<int> { 1, 0 } };
    //        List<int> searchListInt = new List<int> { 1, 2 };

    //        bool cont = Enumerables<int>.ContainsEnumerable(listInt, searchArrayInt);
    //        bool cont2 = Enumerables<double>.ContainsEnumerable(listDouble, searchArrayDouble);
    //        bool cont3 = Enumerables<int>.ContainsEnumerable(arrayInt, searchListInt);

    //        double d = Distances2D.GetDistance(new double[2] { 3.0, 1.0 }, new double[2] { 6.0, 5.0 }, Distances2D.HeuristicType.Euclidian);
    //        Console.WriteLine(d);

    //        Square squareTest1 = new Square(new GridCoords2D(0, 0));
    //        Square squareTestDiag = new Square(new GridCoords2D(1, 1));
    //        Square squareTestStra = new Square(new GridCoords2D(1, 0));
    //        double hd = squareTest1.GetHeuristicDist(squareTestDiag, Distances2D.HeuristicType.Euclidian);

    //        var costsDict = new Dictionary<INode<GridCoords2D>, double>();
    //        costsDict.Add(squareTestDiag, 1);
    //        costsDict.Add(squareTestStra, 1);
    //        var costsDict2 = new Dictionary<INode<GridCoords2D>, double> { { squareTestDiag, 2 }, { squareTestStra, 4 } };

    //        squareTest1.neighboursCosts = costsDict;
    //        double costDiag = squareTest1.GetCostToLeave(squareTestDiag);
    //        double costStra = squareTest1.GetCostToLeave(squareTestStra);

    //        Console.WriteLine(hd);
    //    }
    //}
}
