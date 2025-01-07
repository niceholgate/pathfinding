using System.Collections.Generic;
using NicUtils;

namespace AStarNickNS {

    /*
     * A place with a descriptive type (e.g. a string name, or a grid coordinate).
     */
    public interface IPlace<TCoord> {

        // The Graph that this Place belongs to.
        // TODO: is this needed? - I think so, so that a Place can query its Graph for its Neighbours - Neighbour updates need only happen on Graphs
        // but that should be an impl detail, not part of this interface?
        PlaceGraph<TCoord> Graph { get; }

        TCoord Label { get; }

        ISet<IPlace<TCoord>> Neighbours { get; }

        // The cost to enter this Place from a neighbour. Costs could depend on many factors like
        // - this terrain
        // - neighbour's terrain
        // - agent's speed for this transition (e.g. can it fly? can it use a portal? can it fit in small gaps?)
        // So the cost could become a delegate that turns a "transition context" into a double.
        double Cost { get; }

    }

    /*
     * A place supporting AStar (can define a heuristic distance between any two of these with the same coordinate type).
     */
    public interface IPlaceAStar<TCoord> : IPlace<TCoord> {

        double HeuristicDistanceFrom(IPlaceAStar<TCoord> other, Distances2D.HeuristicType heuristicType);

    }
}
