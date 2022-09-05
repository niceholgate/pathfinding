using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using FibonacciHeap;
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
        // TODO: this could also be a function of the 'agent' and their speed, terrain/flying capabilties etc.
        // Or these corrections might happen elsewhere. Game-specific logic doesn't belong here.
        double GetCostToLeave(IPlace neighbour);
    }

    /*
     * A place with a descriptive type e.g. a string name, or a grid coordinate
     */
    public interface IPlace<LabelType> : IPlace {
        LabelType Label { get; }

        double TerrainCost { get; }
    }

    public interface IPlaceAStar<LabelType> : IPlace<LabelType>{
        double GetHeuristicDist(IPlaceAStar<LabelType> other, Distances2D.HeuristicType heuristicType);
    }

    ///*
    // * A node which can find the cost of getting to any neighbour (even if the neighbour has a different place type)
    // */
    //public interface INode {
    //    double GetCostToLeave(INode neighbour);
    //}

    ///*
    // * A node with a particular place type
    // */
    //public interface INode<CoordType> : INode where CoordType : IPlace {
    //    CoordType Coord { get; }
    //}

    ///*
    // * An AStar node - has heuristic distance calculation ability for its place type
    // */
    //public interface INodeAStar<CoordType> : INode<CoordType> where CoordType : IPlace {
    //    double GetHeuristicDist(INodeAStar<CoordType> other, Distances2D.HeuristicType heuristicType);
    //}

    public abstract class Place : IPlace {
        public Dictionary<IPlace, double> ExplicitNeighboursWithCosts { get; }

        protected Place(Dictionary<IPlace, double> explicitNeighboursWithCosts) {
            ExplicitNeighboursWithCosts = explicitNeighboursWithCosts;
        }

        public abstract bool IsNeighbour(IPlace other);

        public abstract double GetCostToLeave(IPlace neighbour);

        public abstract override string ToString();

        protected bool IsNeighbourExplicit(IPlace other) { return ExplicitNeighboursWithCosts.Keys.Contains(other); }
    }

    /*
     * A place with no defining geometry, just explicitly specified neighbours
     */
    public class GenericPlace : Place, IPlace<string> {
        public string Label { get; }

        public double TerrainCost { get {
                return 1.0; // TODO: replace with access to a terrain map for GenericPlace (e.g. Dictionary<string, double>)
            }
        }

        public GenericPlace(string label) : this(label, new Dictionary<IPlace, double>()) { }

        public GenericPlace(string label,Dictionary<IPlace, double> explicitNeighboursWithCosts) : 
            base(explicitNeighboursWithCosts) {
            Label = label;
        }

        public override bool IsNeighbour(IPlace other) { return IsNeighbourExplicit(other); }

        public override double GetCostToLeave(IPlace neighbour) {
            return ExplicitNeighboursWithCosts[neighbour];
        }

        public override string ToString() => Label;
    }

    /*
     * A place on a 2D grid
     */
    public class GridCoords2D : Place, IPlace<(int, int)> {
        public (int, int) Label { get; }

        public double TerrainCost {
            get {
                return 1.0; // TODO: replace with access to a terrain map for GenericPlace (e.g. Dictionary<(int, int), double>)
                //return TerrainMap[Label];
            }
        }

        public static readonly double SQRT2 = Math.Sqrt(2.0);

        public GridCoords2D((int, int) label) : this(label, new Dictionary<IPlace, double>()) { }

        public GridCoords2D((int, int) label, Dictionary<IPlace, double> explicitNeighboursWithCosts) : 
            base(explicitNeighboursWithCosts) {
            Label = label;
        }

        public override bool IsNeighbour(IPlace other) {
            return IsNeighbourExplicit(other) || IsNeighbourGrid(other);
        }

        public override double GetCostToLeave(IPlace other) {
            if (IsNeighbourGrid(other)) {
                GridCoords2D otherAsGrid = (GridCoords2D)other;
                double distance = IsDiagonalNeighbour(otherAsGrid) ? SQRT2 : 1.0;
                return otherAsGrid.TerrainCost * distance;
            } else if (IsNeighbourExplicit(other)) {
                return ExplicitNeighboursWithCosts[other];
            } else {
                // Return a nonsense cost if the place is not actually a neighbour
                return -1.0;
            }
        }

        public override string ToString() => Label.ToString();

        public int[] DeltaFrom(GridCoords2D other) { return new int[] { this.Label.Item1 - other.Label.Item1, this.Label.Item2 - other.Label.Item2 }; }

        private bool IsNeighbourGrid(IPlace other) {
            if (other is GridCoords2D otherAsGridCoords2D) {
                return IsDiagonalNeighbour(otherAsGridCoords2D) || IsStraightNeighbour(otherAsGridCoords2D);
            }
            return false;
        }

        private bool IsDiagonalNeighbour(GridCoords2D other) {
            int[] delta = DeltaFrom(other);
            return delta[0] * delta[0] + delta[1] * delta[1] == 2;
        }

        private bool IsStraightNeighbour(GridCoords2D other) {
            int[] delta = DeltaFrom(other);
            return Math.Abs(delta[0]) + Math.Abs(delta[1]) == 1;
        }
    }

    //abstract class Node<CoordType> : INode<CoordType> where CoordType : IPlace {
    //    public Node(CoordType coord) { this.Coord = coord; }

    //    public CoordType Coord { get; }

    //    public IDictionary<INode<CoordType>, double> GetNeighboursCosts() { return this.neighboursCosts; }

    //    public virtual double GetCostToLeave(INode neighbour) { return neighboursCosts[neighbour]; }
    //}

    //public class GenericNode : INode<GenericPlace> {
    //    public GenericPlace Coord { get; }

    //    private Dictionary<INode, double> costsToReachExplicitNeighbours = new Dictionary<INode, double>();

    //    public GenericNode(GenericPlace coord) {
    //        Coord = coord;
    //        foreach (IPlace neighbour in Coord.ExplicitNeighbours) {
    //            costsToReachExplicitNeighbours[neighbour] = 10;
    //        }
    //    }

    //    public double GetCostToLeave(INode neighbour) {
    //        return costsToReachExplicitNeighbours[neighbour];
    //    }



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

    //class A_star {
    //    static void Main(string[] args) {
    //        var heap = new FibonacciHeap<string, int>(0);
    //        heap.Insert(new FibonacciHeapNode<string, int>("hello", 5));
    //        FibonacciHeapNode<string, int> min = heap.Min();
    //        //List<int[]> listInt = new List<int[]> { new int[] { 1, 2 }, new int[] { 0, -1 }, new int[] { 0, 1 }, new int[] { 1, 0 } };
    //        //int[] searchArrayInt = { 1, 2 };

    //        //List<double[]> listDouble = new List<double[]> { new double[] { 1, 2 }, new double[] { 0, -1 }, new double[] { 0, 1 }, new double[] { 1, 0 } };
    //        //double[] searchArrayDouble = { 1.1, 2.0 };

    //        //List<int>[] arrayInt = new List<int>[] { new List<int> { 1, 2 }, new List<int> { 0, -1 }, new List<int> { 0, 1 }, new List<int> { 1, 0 } };
    //        //List<int> searchListInt = new List<int> { 1, 2 };

    //        //bool cont = Enumerables<int>.ContainsEnumerable(listInt, searchArrayInt);
    //        //bool cont2 = Enumerables<double>.ContainsEnumerable(listDouble, searchArrayDouble);
    //        //bool cont3 = Enumerables<int>.ContainsEnumerable(arrayInt, searchListInt);

    //        //double d = Distances2D.GetDistance(new double[2] { 3.0, 1.0 }, new double[2] { 6.0, 5.0 }, Distances2D.HeuristicType.Euclidian);
    //        //Console.WriteLine(d);

    //        //Square squareTest1 = new Square(new GridCoords2D(0, 0));
    //        //Square squareTestDiag = new Square(new GridCoords2D(1, 1));
    //        //Square squareTestStra = new Square(new GridCoords2D(1, 0));
    //        //double hd = squareTest1.GetHeuristicDist(squareTestDiag, Distances2D.HeuristicType.Euclidian);

    //        //var costsDict = new Dictionary<INode<GridCoords2D>, double>();
    //        //costsDict.Add(squareTestDiag, 1);
    //        //costsDict.Add(squareTestStra, 1);
    //        //var costsDict2 = new Dictionary<INode<GridCoords2D>, double> { { squareTestDiag, 2 }, { squareTestStra, 4 } };

    //        //squareTest1.neighboursCosts = costsDict;
    //        //double costDiag = squareTest1.GetCostToLeave(squareTestDiag);
    //        //double costStra = squareTest1.GetCostToLeave(squareTestStra);

    //        //Console.WriteLine(hd);
    //    }
    //}
}
