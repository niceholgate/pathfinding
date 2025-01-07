using System;
using System.Collections.Generic;
using NicUtils;

namespace AStarNickNS {
    /*
     * A place on a 2D grid
     */
    public class GridPlace : Place<(int x, int y)>, IPlaceAStar<(int x, int y)> {
        //public static readonly double SQRT2 = Math.Sqrt(2.0);

        public GridPlace((int, int) label, GridPlaceGraph graph) : base(label, graph) { }

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

        public double HeuristicDistanceFrom(IPlaceAStar<(int x, int y)> other, Distances2D.HeuristicType heuristicType) {
            return Distances2D.GetDistance(((double, double))Label, ((double, double))other.Label, heuristicType);
            //double[] thisLabelAsDoubles = { Label.Item1, Label.Item2 };
            //double[] otherLabelAsDoubles = { other.Label.Item1, other.Label.Item2 };
            //return Distances2D.GetDistance(thisLabelAsDoubles, otherLabelAsDoubles, heuristicType);
        }

        public (int, int) DeltaFrom(GridPlace other) { return (Label.x - other.Label.x, Label.y - other.Label.y ); }

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
    }
}
