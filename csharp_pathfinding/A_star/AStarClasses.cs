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
    /*
     * A place which can check if any other place is a neighbour
     */
    public interface IPlace {
        bool IsNeighbour(IPlace other);
    }

    /*
     * A place with a descriptive type e.g. a string name, or a grid coordinate
     */
    public interface IPlace<out LabelType> : IPlace {
        LabelType Label { get; }
    }

    /*
     * A node which can find the cost of getting to any neighbour (even if the neighbour has a different place type)
     */
    public interface INode {
        double GetCostToLeave(INode neighbour);
    }

    /*
     * A node with a particular place type
     */
    public interface INode<CoordType> : INode where CoordType : IPlace {
        CoordType Coord { get; }
    }

    /*
     * An AStar node - has heuristic distance calculation ability for its place type
     */
    public interface INodeAStar<CoordType> : INode<CoordType> where CoordType : IPlace {
        double GetHeuristicDist(INodeAStar<CoordType> other, Distances2D.HeuristicType heuristicType);
    }

    public abstract class Place : IPlace {
        public HashSet<IPlace> ExplicitNeighbours { get; }

        protected Place(HashSet<IPlace> explicitNeighbours) {
            ExplicitNeighbours = explicitNeighbours;
        }

        public abstract bool IsNeighbour(IPlace other);

        protected bool IsNeighbourExplicit(IPlace other) { return ExplicitNeighbours.Contains(other); }
    }

    /*
     * A place with no defining geometry, just explicitly specified neighbours
     */
    public class GenericPlace : Place, IPlace<string> {
        public string Label { get; }

        public GenericPlace(string label) : this(label, new HashSet<IPlace>()) { }

        public GenericPlace(string label, HashSet<IPlace> explicitNeighbours) : base(explicitNeighbours) {
            Label = label;
        }

        public override bool IsNeighbour(IPlace other) { return IsNeighbourExplicit(other); }

        public override string ToString() => Label;
    }

    /*
     * A place on a 2D grid
     */
    public class GridCoords2D : Place, IPlace<(int, int)> {
        public GridCoords2D((int, int) label) : this(label, new HashSet<IPlace>()) { }

        public GridCoords2D((int, int) label, HashSet<IPlace> explicitNeighbours) : base(explicitNeighbours) {
            Label = label;
        }

        public (int, int) Label { get; }

        public override bool IsNeighbour(IPlace other) {
            if (IsNeighbourExplicit(other)) { return true; }
            if (other is GridCoords2D) { return IsNeighbourGrid((GridCoords2D)other); }
            return false;
        }

        private bool IsNeighbourExplicit(IPlace other) {
            return ExplicitNeighbours.Contains(other);
        }

        private bool IsNeighbourGrid(GridCoords2D other) { return IsDiagonalNeighbour(other) || IsStraightNeighbour(other); }

        private bool IsDiagonalNeighbour(GridCoords2D other) {
            int[] delta = DeltaFrom(other);
            return delta[0] * delta[0] + delta[1] * delta[1] == 2;
        }

        private bool IsStraightNeighbour(GridCoords2D other) {
            int[] delta = DeltaFrom(other);
            return Math.Abs(delta[0]) + Math.Abs(delta[1]) == 1;
        }

        public int[] DeltaFrom(GridCoords2D other) { return new int[] { this.Label.Item1 - other.Label.Item1, this.Label.Item2 - other.Label.Item2 }; }

        public override string ToString() => Label.ToString();
    }

    //abstract class Node<CoordType> : INode<CoordType> where CoordType : IPlace {
    //    public Node(CoordType coord) { this.Coord = coord; }

    //    public CoordType Coord { get; }

    //    public IDictionary<INode<CoordType>, double> GetNeighboursCosts() { return this.neighboursCosts; }

    //    public virtual double GetCostToLeave(INode neighbour) { return neighboursCosts[neighbour]; }
    //}

    //public class Square : Node<GridCoords2D>, INodeAStar<GridCoords2D> {
    //    public static readonly double SQRT2 = Math.Sqrt(2.0);

    //    public Square(GridCoords2D coord) : base(coord) { }

    //    public double GetCostToLeave(Square neighbour) {
    //        double distance = neighbour.IsDiagonalNeighbour(this) ? SQRT2 : 1.0;
    //        return neighboursCosts[neighbour] * distance;
    //    }

    //    private bool IsDiagonalNeighbour(Square other) { return Coord.IsDiagonalNeighbour(other.Coord); }

    //    public double GetHeuristicDist(INodeAStar<GridCoords2D> other, Distances2D.HeuristicType heuristicType) {
    //        double[] thisLabelAsDoubles = { Coord.Label.Item1 , Coord.Label.Item2 };
    //        double[] otherLabelAsDoubles = { other.Coord.Label.Item1, other.Coord.Label.Item2 };
    //        return Distances2D.GetDistance(thisLabelAsDoubles, otherLabelAsDoubles, heuristicType);
    //    }
    //}


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
