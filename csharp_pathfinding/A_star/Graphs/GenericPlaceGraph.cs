using System.Collections.Generic;
using NicUtils;

namespace AStarNickNS {
    public class GenericPlaceGraph : PlaceGraph<string> {

        private Dictionary<(string, string), double> _costs = new Dictionary<(string, string), double>();

        public GenericPlaceGraph() {

        }

        public override Dictionary<Place<string>, double> GetImplicitNeighboursWithCosts(Place<string> place) {
            return new Dictionary<Place<string>, double>();
        }

        public bool BuildFromTables(List<(string, string, double)> neighbourPairsCosts) {
            for (int i = 0; i < neighbourPairsCosts.Count; i++) {
                (string, string) neighbourPair = (neighbourPairsCosts[i].Item1, neighbourPairsCosts[i].Item2);
                double cost = neighbourPairsCosts[i].Item3;
                // cannot have a negative input cost
                if (cost < 0.0) return false; 
                // cannot add a cost if it has already been added 
                if (GetCost(neighbourPair) > 0.0) { return false; }
                _costs[neighbourPair] = cost;
                Places[neighbourPair.Item1] = new GenericPlace(neighbourPair.Item1, this);
                Places[neighbourPair.Item2] = new GenericPlace(neighbourPair.Item2, this);
            }
            return true;
        }
        
        private double GetCost((string, string) neighbourPair) {
            if (_costs.ContainsKey((neighbourPair.Item1, neighbourPair.Item2))) return _costs[(neighbourPair.Item1, neighbourPair.Item2)];
            if (_costs.ContainsKey((neighbourPair.Item2, neighbourPair.Item1))) return _costs[(neighbourPair.Item2, neighbourPair.Item1)];
            return -1.0;
        }

        public override void Build(string dataFile) {
            List<string[]> nodes = new NicUtils.CSVReader(dataFile).GetData();
            List<int[]> costs = NicUtils.CSVReader.ReadCSV(dataFile);
            int a = 2;
        }
    }
}
