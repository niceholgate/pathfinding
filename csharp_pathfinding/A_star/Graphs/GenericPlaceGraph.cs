using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NicUtils;

namespace AStarNickNS
{
    public partial class GenericPlaceGraph : PlaceGraph<string>
    {
        private readonly Dictionary<PlacePair, double> costs = new();

        //public override Dictionary<Place<string>, double> GetImplicitNeighboursWithCosts(Place<string> place) {
        //    return new Dictionary<Place<string>, double>();
        //}

        protected override bool PlaceAccessible(string from, string to, double pathfinderSize)
        {
            return PlaceExists(to);
        }

        protected override void BuildFromFileCore(string dataFile)
        {
            if (!dataFile.EndsWith(".mmd"))
            {
                throw new ArgumentException("GenericPlaceGraph only supports building from .mmd (Mermaid) files");
            }
            List<string> mermaidLines = new TextLineReader(dataFile).GetData();
            BuildFromMermaid(mermaidLines);
        }

        private void BuildFromMermaid(List<string> mermaidLines)
        {
            var placePairsWithCosts = mermaidLines
                .Select(line => MermaidRegex().Match(line))
                .Where(match => match.Success)
                .Select(match =>
                {
                    var place1 = match.Groups[1].Value;
                    var place2 = match.Groups[2].Value;
                    var cost = double.Parse(match.Groups[3].Value);
                    return (new PlacePair(place1, place2), cost);
                })
                .ToList();

            foreach (var (placePair, cost) in placePairsWithCosts)
            {
                if (cost < 0.0)
                {
                    throw new ArgumentException($"Cannot have a negative cost: {cost} for {placePair}");
                }

                if (GetCost(placePair) > 0.0)
                {
                    throw new ArgumentException($"Cannot specify the same pair of places more than once: {placePair}");
                }

                costs[placePair] = cost;
                var place1 = GetPlaceOrCreate(placePair.Place1);
                var place2 = GetPlaceOrCreate(placePair.Place2);

                place1.Neighbours.Add(place2);
                place2.Neighbours.Add(place1);
            }
        }

        private GenericPlace GetPlaceOrCreate(string label)
        {
            if (Places.TryGetValue(label, out var place))
            {
                return (GenericPlace)place;
            }

            Places[label] = new GenericPlace(label);
            return (GenericPlace)Places[label];
        }

        public override double CostToLeave(string place1, string place2)
        {
            return GetCost(new PlacePair(place1, place2));
        }

        private double GetCost(PlacePair placePair)
        {
            return costs.GetValueOrDefault(placePair, -1.0);
        }

        // Ensure there is no duplication due to reversed labels by sorting pairs upon creation
        private readonly struct PlacePair
        {
            public string Place1 { get; init; }
            public string Place2 { get; init; }

            public PlacePair(string place1, string place2)
            {
                if (place1 == null || place2 == null || place1 == place2)
                {
                    throw new Exception("GenericPlace labels must be unique and non-null.");
                }
                if (place1.CompareTo(place2) > 0)
                {
                    Place2 = place1;
                    Place1 = place2;
                }
                else
                {
                    Place1 = place1;
                    Place2 = place2;
                }
            }

            public override string ToString() => $"({Place1}, {Place2})";
        }

        private static Regex MermaidRegex() => new Regex(@"^\s*(\w+)\s*-->\s*(\w+)\s*:\s*(-?[\d\.]+)\s*$", RegexOptions.Compiled);
    }
}