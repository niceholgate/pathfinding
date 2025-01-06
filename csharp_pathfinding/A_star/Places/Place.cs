using System;
using System.Collections.Generic;
using System.Linq;

namespace AStarNickNS {
    public abstract class Place<TCoord> : IPlace<TCoord> {

        public PlaceGraph<TCoord> Graph { get; }

        protected Place(TCoord label, PlaceGraph<TCoord> graph) : this(label, graph, new Dictionary<Place<TCoord>, double>()) { }

        protected Place(TCoord label, PlaceGraph<TCoord> graph, Dictionary<Place<TCoord>, double> explicitNeighboursWithCosts) {
            Label = label;
            Graph = graph;
            ExplicitNeighboursWithCosts = explicitNeighboursWithCosts;
        }

        public TCoord Label { get; }

        public double Cost {
            get {
                return Graph.GetTerrainCost(Label);
            }
        }

        // Not using this yet. Perhaps don't want to distinguish between "normal" and "hacky" neighbours at this level.
        public Dictionary<Place<TCoord>, double> ExplicitNeighboursWithCosts { get; }

        //public Dictionary<IPlace<TCoord>, double> NeighboursWithCosts {
        //    get {
        //        var merge = new Dictionary<IPlace<TCoord>, double>(ExplicitNeighboursWithCosts);
        //        foreach (var item in Graph.GetImplicitNeighboursWithCosts(this)) {
        //            merge[item.Key] = item.Value;
        //        }
        //        return merge;
        //    }
        //}

        public ISet<IPlace<TCoord>> Neighbours {
            get {
                return (ISet<IPlace<TCoord>>)Graph.GetImplicitNeighboursWithCosts(this).Keys.ToHashSet();
            }
        }

        public abstract ISet<IPlace<TCoord>> ImplicitNeighbours { get; }

        public abstract bool IsNeighbourImplicit(IPlace<TCoord> other);

        public bool IsNeighbour(IPlace<TCoord> other) {
            return IsNeighbourExplicit(other) || IsNeighbourImplicit(other);
        }

        // Planning to just deal with "cost to enter" initially, then implement "transitions contexts" which inform cost functions
        //public abstract double GetCostToLeave(IPlace<TCoord> neighbour);

        public abstract override string ToString();

        protected bool IsNeighbourExplicit(IPlace<TCoord> other) { return ExplicitNeighboursWithCosts.Keys.Contains(other); }
    }
}
