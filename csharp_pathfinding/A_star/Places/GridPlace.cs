using System;
using System.Collections.Generic;
using NicUtils;

namespace AStarNickNS {
    /*
     * A place on a 2D grid
     */
    public class GridPlace : IPlaceAStar<(int x, int y)> {
        //public static readonly double SQRT2 = Math.Sqrt(2.0);

        public (int x, int y) Label { get; init; }
        private ISet<IPlaceAStar<(int x, int y)>> AStarNeighbours { get; }
        public ISet<IPlace<(int x, int y)>> Neighbours { get; }

        public double CostToLeave(IPlace<(int x, int y)> to, PlaceGraph<(int x, int y)> graph)
        {
            return graph.CostToLeave(Label, to.Label);
        }

        public GridPlace((int, int) label)
        {
            Label = label;
            AStarNeighbours = new HashSet<IPlaceAStar<(int x, int y)>>();
            Neighbours = new HashSet<IPlace<(int x, int y)>>(AStarNeighbours);
        }

        public double DistanceFrom(IPlaceAStar<(int x, int y)> other, Distances2D.HeuristicType heuristicType) {
            return Distances2D.GetDistance(Label, other.Label, heuristicType);
            //double[] thisLabelAsDoubles = { Label.Item1, Label.Item2 };
            //double[] otherLabelAsDoubles = { other.Label.Item1, other.Label.Item2 };
            //return Distances2D.GetDistance(thisLabelAsDoubles, otherLabelAsDoubles, heuristicType);
        }

        public (int, int) N { get { return (Label.x, Label.y - 1); } }
        public (int, int) NE { get { return (Label.x + 1, Label.y - 1); } }
        public (int, int) E { get { return (Label.x + 1, Label.y); } }
        public (int, int) SE { get { return (Label.x + 1, Label.y + 1); } }
        public (int, int) S { get { return (Label.x, Label.y + 1); } }
        public (int, int) SW { get { return (Label.x - 1, Label.y + 1); } }
        public (int, int) W { get { return (Label.x - 1, Label.y); } }
        public (int, int) NW { get { return (Label.x - 1, Label.y - 1); } }

        public (int, int) DeltaFrom(GridPlace other) { return (Label.x - other.Label.x, Label.y - other.Label.y); }

        public bool IsDiagonalTo(GridPlace other) {
            (int x, int y) = DeltaFrom(other);
            return x * x + y * y == 2;
        }

        public bool IsAdjacentTo(GridPlace other) {
            (int x, int y) = DeltaFrom(other);
            if (x == 0) {
                if (y == -1 || y == 1) return true;
            } else if (y == 0) {
                if (x == -1 || x == 1) return true;
            }
            return false;
        }

        public override string ToString() => Label.ToString();

        //public GridPlace((int, int) label, GridPlaceGraph graph, Dictionary<Place<(int, int)>, double> explicitNeighboursWithCosts)
        //    : base(label, graph) { }

        // Initially will treat all adjacent squares as neighbours, and check downstream if it's blocked, off the map, or diagonal moves not allowed etc.
        // TODO: this should just make a call to the Graph which contains logic on neighbours and cost calculations - in which case this method can probably be defined on the base class
        //public override HashSet<IPlace<(int, int)>> ImplicitNeighbours {
        //    get {
        //        HashSet<IPlace<(int, int)>> gridNeighbours = new HashSet<IPlace<(int, int)>>();
        //        // TODO: use ranges?
        //        foreach (int i in new int[3] { -1, 0, 1 }) {
        //            foreach (int j in new int[3] { -1, 0, 1 }) {
        //                if (!(i == 0 && j == 0)) {
        //                    gridNeighbours.Add(Graph.Places[(i, j)]);
        //                }
        //            }
        //        }
        //        return gridNeighbours;
        //    }
        //}

        //public override bool IsNeighbourImplicit(IPlace<(int, int)> other) {
        //    if (other.Graph != this.Graph) return false;
        //    return IsDiagonalNeighbour((GridPlace)other) || IsStraightNeighbour((GridPlace)other);
        //}

        //public override double GetCostToLeave(IPlace<(int, int)> other) {
        //    if (IsNeighbourImplicit(other)) {
        //        double distance = IsDiagonalNeighbour(other) ? SQRT2 : 1.0;
        //        return other.TerrainCost * distance;
        //    } else if (IsNeighbourExplicit(other)) {
        //        return ExplicitNeighboursWithCosts[other];
        //    } else {
        //        // Return a nonsense cost if the place is not actually a neighbour
        //        return -1.0;
        //    }
        //}
        
    }
}
