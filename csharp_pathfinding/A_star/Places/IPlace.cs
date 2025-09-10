using System.Collections.Generic;

namespace AStarNickNS {

    /*
     * A place with a descriptive type (e.g. a string name, or a grid coordinate).
     */
    public interface IPlace<TCoord> {

        // The Graph that this Place belongs to.
        // TODO: is this needed? - I think so, so that a Place can query its Graph for its Neighbours - Neighbour updates need only happen on Graphs
        // but that should be an impl detail, not part of this interface?
        // PlaceGraph<TCoord> Graph { get; }

        TCoord Label { get; init; }

        ISet<IPlace<TCoord>> Neighbours { get; }

        // The cost to enter this Place from a neighbour. Costs could depend on many factors like
        // - this terrain
        // - neighbour's terrain
        // - agent's speed for this transition (e.g. can it fly? can it use a portal? can it fit in small gaps?)
        // TODO: So the cost could become a delegate that turns a "transition context" into a double.
        double Cost { get; set; }

    }
    
}
