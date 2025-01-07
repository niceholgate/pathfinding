using System.Collections.Generic;

namespace AStarNickNS {
    /*
    * A place with no defining geometry, just explicitly specified neighbours
    */
    public class GenericPlace : Place<string> {
        public GenericPlace(string label, GenericPlaceGraph graph) : base(label, graph) { }

        public GenericPlace(string label, GenericPlaceGraph graph, Dictionary<Place<string>, double> explicitNeighboursWithCosts)
            : base(label, graph) { }

        // GenericPlace has no implicit neighbours because it is described by text labels rather than a coordinate system
        //public override HashSet<IPlace<string>> ImplicitNeighbours { get { return new HashSet<IPlace<string>>(); } }

        //public override bool IsNeighbourImplicit(IPlace<string> other) {
        //    return false;
        //}

        // TODO: ExplicitNeighboursWithCosts should be a getter accessing the Graph, not stored on the Place
        //public override double GetCostToLeave(IPlace<string> neighbour) {
        //    return ExplicitNeighboursWithCosts[neighbour];
        //}

        public override string ToString() => Label;
    }
}