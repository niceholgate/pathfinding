using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using NicUtils;

namespace AStarNickNS {
    public partial class GenericPlaceGraph : PlaceGraph<string> {

        private readonly Dictionary<PlacePair, double> _costs = new();

        public GenericPlaceGraph() {

        }

        //public override Dictionary<Place<string>, double> GetImplicitNeighboursWithCosts(Place<string> place) {
        //    return new Dictionary<Place<string>, double>();
        //}

        public override void Build(string dataFile) {
            List<string> mermaidLines = new TextLineReader(dataFile).GetData();
            BuildFromMermaid(mermaidLines);
        }

        private void BuildFromMermaid(List<string> mermaidLines) {
            List<(PlacePair, double)> placePairsWithCosts = mermaidLines
                // TODO: can probably replace this with a single regex pattern that grabs the 3 variables if it succeeds
                .Where(str => {
                    return ArrowRegex().Matches(str).Count == 1 && ColonRegex().Matches(str).Count == 1;
                })
                .Select(str => {
                    string[] split1 = str.Split(ArrowRegex().ToString());
                    string place1 = split1[0].Trim();
                    string[] split2 = split1[1].Split(ColonRegex().ToString());
                    string place2 = split2[0].Trim();
                    double cost = Double.Parse(split2[1].Trim());
                    return (new PlacePair(place1, place2), cost);
                })
                .ToList();

            foreach ((PlacePair placePair, double cost) placePairWithCost in placePairsWithCosts) {
                if (placePairWithCost.cost < 0.0) {
                    throw new ArgumentException($"Cannot have a negative cost: {placePairWithCost.cost} for {placePairWithCost.placePair}");
                }
                if (GetCost(placePairWithCost.placePair) > 0.0) {
                    throw new ArgumentException($"Cannot specify the same pair of places more than once: {placePairWithCost.placePair}");
                }
                _costs[placePairWithCost.placePair] = placePairWithCost.cost;
                GenericPlace place1 = GetPlaceOrCreate(placePairWithCost.placePair.Place1);
                GenericPlace place2 = GetPlaceOrCreate(placePairWithCost.placePair.Place2);

                place1.Neighbours.Add(place2);
                place2.Neighbours.Add(place1);
            }
        }

        public GenericPlace GetPlaceOrCreate(string label) {
            if (Places.ContainsKey(label)) {
                return (GenericPlace)Places[label];
            } else {
                Places[label] = new GenericPlace(label, this);
                return (GenericPlace)Places[label];
            }
        }

        public double GetCost(string place1, string place2) {
            return GetCost(new PlacePair(place1, place2));
        }

        private double GetCost(PlacePair placePair) {
            return _costs.GetValueOrDefault(placePair, -1.0);
        }

        // Ensure there is no duplication due to reversed labels by sorting pairs upon creation
        private readonly struct PlacePair {

            public string Place1 { get; init; }
            public string Place2 { get; init; }

            public PlacePair(string place1, string place2) {
                if (place1 == null || place2 == null || place1 == place2) {
                    throw new Exception("GenericPlace labels must be unique and non-null.");
                } else if (place1.CompareTo(place2) > 0) {
                    Place2 = place1;
                    Place1 = place2;
                } else {
                    Place1 = place1;
                    Place2 = place2;
                }
            }

            public override string ToString() => $"({Place1}, {Place2})";
        }

        [GeneratedRegex("-->")]
        private static partial Regex ArrowRegex();
        [GeneratedRegex(":")]
        private static partial Regex ColonRegex();
    }
}
