using System.Collections.Generic;

namespace AStarNickNS {
    public class GridPlaceGraph : PlaceGraph<(int, int)> {
        public GridPlaceGraph() {

        }

        //public override Dictionary<Place<(int, int)>, double> GetImplicitNeighboursWithCosts(Place<(int, int)> place) {
        //    return new Dictionary<Place<(int, int)>, double>();
        //}

        public override void Build(string dataFile) {
            throw new System.NotImplementedException();
        }
    }
}
