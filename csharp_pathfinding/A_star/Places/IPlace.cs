using System.Collections.Generic;
using NicUtils;

namespace AStarNickNS {
    public interface IPlace {
        // The neighbours and costs to transition into each - costs could depend on many factors like
        // - this terrain
        // - neighbour's terrain
        // - agent's speed for this transition (e.g. can it fly? can it use a portal? can it fit in small gaps?)
        // So the cost could become a delegate that turns a "transition context" into a double
        Dictionary<IPlace, double> NeighboursWithCosts { get; }

        // The Graph that this Place belongs to.
    }

    /*
     * A place with a descriptive type (e.g. a string name, or a grid coordinate).
     */
    public interface IPlace<TCoord> : IPlace {

        PlaceGraph<TCoord> Graph { get; }

        TCoord Label { get; }

        double TerrainCost { get; }

    }

    public interface IPlaceAStar<TCoord> : IPlace<TCoord> {
        double GetHeuristicDist(IPlaceAStar<TCoord> other, Distances2D.HeuristicType heuristicType);
    }
}
