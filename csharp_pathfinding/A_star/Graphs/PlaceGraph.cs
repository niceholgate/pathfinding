using System.Collections.Generic;

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
        private Dictionary<TCoord, double> _terrainCosts;

        public Dictionary<TCoord, Place<TCoord>> Places = new Dictionary<TCoord, Place<TCoord>>();

        public bool IsBlocked(TCoord label) { return GetTerrainCost(label) < 0; }

        public double GetTerrainCost(TCoord label) { return _terrainCosts[label]; }

        public void SetTerrainCost(TCoord label, double cost) { _terrainCosts[label] = cost; }

        public abstract Dictionary<Place<TCoord>, double> GetImplicitNeighboursWithCosts(Place<TCoord> place);

        // getexplicitneighbours ?
        public abstract void Build(string dataFile);
    }
}
