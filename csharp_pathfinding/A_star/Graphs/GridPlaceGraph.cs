using System.Collections.Generic;

namespace AStarNickNS {
    public class GridPlaceGraph : PlaceGraph<(int, int)> {

        public GridPlaceGraph(string dataFile) : base(dataFile) {}

        //public override Dictionary<Place<(int, int)>, double> GetImplicitNeighboursWithCosts(Place<(int, int)> place) {
        //    return new Dictionary<Place<(int, int)>, double>();
        //}

        protected override void Build(string dataFile) {
            
        }
    }
}
