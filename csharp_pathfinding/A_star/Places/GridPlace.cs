using System;
using System.Collections.Generic;
using NicUtils;

namespace AStarNickNS {
    /*
     * A place on a 2D grid
     */
    public class GridPlace : Place<(int, int)>, IPlaceAStar<(int, int)> {
        public static readonly double SQRT2 = Math.Sqrt(2.0);

        public GridPlace((int, int) label, GridPlaceGraph graph) : base(label, graph) { }

        public GridPlace((int, int) label, GridPlaceGraph graph, Dictionary<IPlace, double> explicitNeighboursWithCosts)
            : base(label, graph, explicitNeighboursWithCosts) { }

        // Initially will treat all adjacent squares as neighbours, and check downstream if it's blocked, off the map, or diagonal moves not allowed etc.
        public override List<IPlace<(int, int)>> ImplicitNeighbours {
            get {
                List<IPlace<(int, int)>> gridNeighbours = new List<IPlace<(int, int)>>();
                foreach (int i in new int[3] { -1, 0, 1 }) {
                    foreach (int j in new int[3] { -1, 0, 1 }) {
                        if (!(i == 0 && j == 0)) {
                            gridNeighbours.Add(Graph.Places[(i, j)]);
                        }
                    }
                }
                return gridNeighbours;
            }
        }

        public override bool IsNeighbourImplicit(IPlace<(int, int)> other) {
            if (other.Graph != this.Graph) return false;
            return IsDiagonalNeighbour(other) || IsStraightNeighbour(other);
        }

        public override double GetCostToLeave(IPlace<(int, int)> other) {
            if (IsNeighbourImplicit(other)) {
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

        private bool IsDiagonalNeighbour(IPlace<(int, int)> other) {
            int[] delta = DeltaFrom(other);
            return delta[0] * delta[0] + delta[1] * delta[1] == 2;
        }

        private bool IsStraightNeighbour(IPlace<(int, int)> other) {
            int[] delta = DeltaFrom(other);
            return Math.Abs(delta[0]) + Math.Abs(delta[1]) == 1;
        }
    }
}
