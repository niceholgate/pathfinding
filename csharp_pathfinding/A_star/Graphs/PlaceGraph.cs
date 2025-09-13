using System.Collections.Generic;
using System.Linq;
using System.IO;

/*
 * Decided there will be no multi-Place-type Graphs.
 * If there is a non-grid graph linking 2 non-adjacent GridPlaces, represent this with a neighbours update on the GridPlacesGraph.
 * If necessary, the cost function for these new "portal neighbours" perform Dijkstra on the linking non-grid graph.
 * 
 * May need an events system so that graphs know when they are updated. Need to recalc if:
 * -Connectivity on a calculates path changes
 * -Costs on a calculated path change
 * An alternative could just be recalc periodically or if requested (e.g. an agent has become confused)
 */

namespace AStarNickNS {
    // Ability to get the terrain cost should depend on how smart the agent finding a path is. i.e. should they be planning according to player's slow debuffs?
    public abstract class PlaceGraph<TCoord> {

        // The cost to enter the specified Place.
        protected readonly Dictionary<TCoord, double> _terrainCosts = new();

        public Dictionary<TCoord, IPlace<TCoord>> Places = new();

        public bool IsBlocked(TCoord from, TCoord to) { return CostToLeave(from, to) < 0; }

        public double GetTerrainCost(TCoord label) { return _terrainCosts[label]; }
        //
        public void SetTerrainCost(TCoord label, double cost) { _terrainCosts[label] = cost; }

        public abstract double CostToLeave(TCoord from, TCoord to);
        
        //public abstract Dictionary<Place<TCoord>, double> GetImplicitNeighboursWithCosts(Place<TCoord> place);

        // getexplicitneighbours ?
        public void Build(string dataFile)
        {
            BuildCore(dataFile);
            CheckDisjoint();
        }
        
        protected virtual void BuildCore(string dataFile) { }

        // TODO: move this to PlaceGraph and test it for every implementation thereof
        protected void CheckDisjoint() {
            // Start at a random Place and traverse the full Graph, stopping when every Place has been visited.
            HashSet<TCoord> placeLabelsToVisit = Places.Keys.ToHashSet();
            if (placeLabelsToVisit.Count == 0) return;
            
            Stack<IPlace<TCoord>> stack = new(Enumerable.Repeat(Places.Values.First(), 1));
            HashSet<IPlace<TCoord>> neighboursHaveBeenAddedToStack = new();
            while (stack.Count > 0) {
                IPlace<TCoord> thisPlace = stack.Pop();
                placeLabelsToVisit.Remove(thisPlace.Label);
                if (placeLabelsToVisit.Count == 0) return;

                if (!neighboursHaveBeenAddedToStack.Contains(thisPlace)) {
                    foreach (IPlace<TCoord> neighbour in thisPlace.Neighbours) stack.Push(neighbour);
                    neighboursHaveBeenAddedToStack.Add(thisPlace);
                }

            }

            // If you cannot visit every Place, then the Graph is disjoint.
            throw new IOException("Cannot support a disjoint Graph!");
        }
    }
}
