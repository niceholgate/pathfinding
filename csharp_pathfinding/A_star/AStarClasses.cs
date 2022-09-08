using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using FibonacciHeap;
using NicUtils;

// https://stackoverflow.com/questions/150479/order-of-items-in-classes-fields-properties-constructors-methods
namespace AStarNickNS {

    // Ability to get the terrain cost should depend on how smart the agent finding a path is. i.e. should they be planning according to player's slow debuffs?
    public abstract class PlaceGraph<LabelType> {
        private Dictionary<LabelType, double> _terrainCosts;

        public Dictionary<LabelType, Place<LabelType>> Places;

        public bool IsBlocked(LabelType label) { return GetTerrainCost(label) < 0;  }

        public double GetTerrainCost(LabelType label) { return _terrainCosts[label]; }

        public void SetTerrainCost(LabelType label, double cost) { _terrainCosts[label] = cost; }
        
        // public abstract void Build(string dataFile);
    }

    public class GenericPlaceGraph : PlaceGraph<string> {

    }

    public class GridPlaceGraph : PlaceGraph<(int, int)> {

    }

    public interface IPlace {
        Dictionary<IPlace, double> NeighboursWithCosts { get; }
    }

    /*
     * A place with a descriptive type (e.g. a string name, or a grid coordinate) and a terrain cost for moving onto it.
     * It can check if a peer is a neighbour and get the cost of going to a neighbour.
     */
    public interface IPlace<LabelType> : IPlace {
        LabelType Label { get; }

        double TerrainCost { get; }

        bool IsNeighbour(IPlace<LabelType> other);
        // TODO: this could also be a function of the 'agent' and their speed, terrain/flying capabilties etc.
        // Or these corrections might happen elsewhere. Game-specific logic doesn't belong here.
        double GetCostToLeave(IPlace<LabelType> other);
    }

    public interface IPlaceAStar<LabelType> : IPlace<LabelType> {
        double GetHeuristicDist(IPlaceAStar<LabelType> other, Distances2D.HeuristicType heuristicType);
    }

    public abstract class Place<LabelType> : IPlace<LabelType> {

        protected readonly PlaceGraph<LabelType> _graph;

        protected Place(LabelType label, PlaceGraph<LabelType> graph) : this(label, graph, new Dictionary<IPlace<LabelType>, double>()) { }

        protected Place(LabelType label, PlaceGraph<LabelType> graph, Dictionary<IPlace<LabelType>, double> explicitNeighboursWithCosts) {
            Label = label;
            _graph = graph;
            ExplicitNeighboursWithCosts = explicitNeighboursWithCosts;
        }

        public LabelType Label { get; }

        public double TerrainCost {
            get {
                return _graph.GetTerrainCost(Label);
            }
        }

        public Dictionary<IPlace, double> ExplicitNeighboursWithCosts { get; }

        public Dictionary<IPlace, double> NeighboursWithCosts {
            get {
                var merge = new Dictionary<TKey, TValue>(ExplicitNeighboursWithCosts);
                foreach (var item in keyValuePairs) {
                    dictionaryMerge[item.Key] = item.Value;
                }
                return merge;
            }
        }

        public abstract IEnumerable<IPlace<LabelType>> ImplicitNeighbours { get; }

        public abstract bool IsNeighbour(IPlace<LabelType> other);

        public abstract double GetCostToLeave(IPlace<LabelType> neighbour);

        public abstract override string ToString();

        protected bool IsNeighbourExplicit(IPlace<LabelType> other) { return ExplicitNeighboursWithCosts.Keys.Contains(other); }
    }

    /*
     * A place with no defining geometry, just explicitly specified neighbours
     */
    public class GenericPlace : Place<string> {
        public GenericPlace(string label, GenericPlaceGraph graph) : base(label, graph) { }

        public GenericPlace(string label, GenericPlaceGraph graph, Dictionary<IPlace<string>, double> explicitNeighboursWithCosts)
            : base(label, graph, explicitNeighboursWithCosts) { }

        public override List<IPlace<string>> ImplicitNeighbours { get { return new List<IPlace<string>>(); } }

        public override bool IsNeighbour(IPlace<string> other) { return IsNeighbourExplicit(other); }

        public override double GetCostToLeave(IPlace<string> neighbour) {
            return ExplicitNeighboursWithCosts[neighbour];
        }

        public override string ToString() => Label;
    }

    /*
     * A place on a 2D grid
     */
    public class GridPlace : Place<(int, int)>, IPlaceAStar<(int, int)> {
        public static readonly double SQRT2 = Math.Sqrt(2.0);

        public GridPlace((int, int) label, GridPlaceGraph graph) : base(label, graph) { }

        public GridPlace((int, int) label, GridPlaceGraph graph, Dictionary<IPlace<(int, int)>, double> explicitNeighboursWithCosts)
            : base(label, graph, explicitNeighboursWithCosts) { }

        // Initially will treat all adjacent squares as neighbours, and check downstream if it's blocked, off the map, or diagonal moves not allowed etc.
        public override List<IPlace<(int, int)>> ImplicitNeighbours { 
            get {
                List<IPlace<(int, int)>> gridNeighbours = new List<IPlace<(int, int)>>();
                foreach (int i in new int[3] { -1, 0, 1 }) {
                    foreach (int j in new int[3] { -1, 0, 1 }) {
                        if (!(i == 0 && j == 0)) {
                            gridNeighbours.Add(_graph.Places[(i, j)]);
                        }
                    }
                }
                return gridNeighbours;
            }
        }

        public override bool IsNeighbour(IPlace<(int, int)> other) {
            return IsNeighbourExplicit(other) || IsNeighbourGrid(other);
        }

        public override double GetCostToLeave(IPlace<(int, int)> other) {
            if (IsNeighbourGrid(other)) {
                double distance = IsDiagonalNeighbour(other) ? SQRT2 : 1.0;
                return other.TerrainCost * distance;
            } else if (IsNeighbourExplicit(other)) {
                return ExplicitNeighboursWithCosts[other];
            } else {
                // Return a nonsense cost if the place is not actually a neighbour
                return -1.0;
            }
        }

        public double GetHeuristicDist(IPlaceAStar<(int, int)> other, Distances2D.HeuristicType heuristicType) {
            double[] thisLabelAsDoubles = { Label.Item1, Label.Item2 };
            double[] otherLabelAsDoubles = { other.Label.Item1, other.Label.Item2 };
            return Distances2D.GetDistance(thisLabelAsDoubles, otherLabelAsDoubles, heuristicType);
        }

        public override string ToString() => Label.ToString();

        public int[] DeltaFrom(IPlace<(int, int)> other) { return new int[] { Label.Item1 - other.Label.Item1, Label.Item2 - other.Label.Item2 }; }

        private bool IsNeighbourGrid(IPlace<(int, int)> other) {
            return IsDiagonalNeighbour(other) || IsStraightNeighbour(other);
        }

        private bool IsDiagonalNeighbour(IPlace<(int, int)> other) {
            int[] delta = DeltaFrom(other);
            return delta[0] * delta[0] + delta[1] * delta[1] == 2;
        }

        private bool IsStraightNeighbour(IPlace<(int, int)> other) {
            int[] delta = DeltaFrom(other);
            return Math.Abs(delta[0]) + Math.Abs(delta[1]) == 1;
        }
    }


    // TODO: can the generics and interfaces be improved?
    // TODO: catch path not found appropriately and test
    // TODO: should not refer to ExplicitNeighbours directly - then can remove from interface
    //interface IDijkstraSolver<out IPlace> {
    //    IEnumerable<IPlace> ReconstructPath();
    //}

    public class DijkstraSolver<TPlace> where TPlace : class, IPlace { 
        private readonly TPlace _start;
        private readonly TPlace _target;
        private TPlace _current;
        private double _newCost;
        private Dictionary<TPlace, TPlace> _cameFrom;
        private Dictionary<TPlace, double> _costSoFar;
        private bool _hasRun = false;
        private bool _foundPath = false;

        public DijkstraSolver(TPlace start, TPlace target) {
            _start = start;
            _target = target;
        }

        public void Solve() {
            _hasRun = true;
            FibonacciHeap<TPlace, double> frontier = new(0);
            frontier.Insert(new FibonacciHeapNode<TPlace, double>(_start, 0));
            _cameFrom = new Dictionary<TPlace, TPlace>() { { _start, null } };
            _costSoFar = new Dictionary<TPlace, double>() { { _start, 0.0 } };

            while (!frontier.IsEmpty()) {
                _current = frontier.RemoveMin().Data;
                if (_current.Equals(_target)) {
                    _foundPath = true;
                    break;
                }
                foreach (TPlace neighbour in _current.NeighboursWithCosts.Keys) {
                    _newCost = _costSoFar[_current] + _current.NeighboursWithCosts[neighbour];
                    if (!_costSoFar.ContainsKey(neighbour) || _newCost < _costSoFar[neighbour]) {
                        _costSoFar[neighbour] = _newCost;
                        frontier.Insert(new FibonacciHeapNode<TPlace, double>(neighbour, _newCost));
                        _cameFrom[neighbour] = _current;
                    }
                }
            }
        }

        public IEnumerable<TPlace> ReconstructPath() {
            if (_hasRun && _foundPath) {
                _current = _target;
                List<TPlace> path = new List<TPlace>();
                while (!_current.Equals(_start)) {
                    path.Add(_current);
                    _current = _cameFrom[_current];
                }
                path.Add(_start);
                path.Reverse();
                return path;
            }
            return null;
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
