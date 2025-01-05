using System;
using System.Collections.Generic;
using System.Linq;

namespace AStarNickNS {
    public abstract class Place<TCoord> : IPlace<TCoord> {

        public PlaceGraph<TCoord> Graph { get; }

        protected Place(TCoord label, PlaceGraph<TCoord> graph) : this(label, graph, new Dictionary<IPlace, double>()) { }

        protected Place(TCoord label, PlaceGraph<TCoord> graph, Dictionary<IPlace, double> explicitNeighboursWithCosts) {
            Label = label;
            Graph = graph;
            ExplicitNeighboursWithCosts = explicitNeighboursWithCosts;
        }

        public TCoord Label { get; }

        public double TerrainCost {
            get {
                return Graph.GetTerrainCost(Label);
            }
        }

        public Dictionary<IPlace, double> ExplicitNeighboursWithCosts { get; }

        public Dictionary<IPlace, double> NeighboursWithCosts {
            get {
                var merge = new Dictionary<IPlace, double>(ExplicitNeighboursWithCosts);
                foreach (var item in Graph.GetImplicitNeighboursWithCosts(this)) {
                    merge[item.Key] = item.Value;
                }
                return merge;
            }
        }

        public abstract IEnumerable<IPlace<TCoord>> ImplicitNeighbours { get; }

        public abstract bool IsNeighbourImplicit(IPlace<TCoord> other);

        public bool IsNeighbour(IPlace<TCoord> other) {
            return IsNeighbourExplicit(other) || IsNeighbourImplicit(other);
        }

        public abstract double GetCostToLeave(IPlace<TCoord> neighbour);

        public abstract override string ToString();

        protected bool IsNeighbourExplicit(IPlace<TCoord> other) { return ExplicitNeighboursWithCosts.Keys.Contains(other); }
    }
}
