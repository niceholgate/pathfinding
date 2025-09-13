using NicUtils;
using System;
using System.Collections.Generic;

namespace AStarNickNS {
    public class GridPlaceGraph : PlaceGraph<(int, int)> {

        private bool DiagonalNeighbours { get; init; }
        
        public GridPlaceGraph(bool diagonalNeighbours) {
            DiagonalNeighbours = diagonalNeighbours;
        }

        //public override Dictionary<Place<(int, int)>, double> GetImplicitNeighboursWithCosts(Place<(int, int)> place) {
        //    return new Dictionary<Place<(int, int)>, double>();
        //}

        public override double CostToLeave((int, int) from, (int, int) to)
        {
            return GetTerrainCost(to);
        }

        protected override void BuildCore(string dataFile) {

            List<List<double>> gridCosts = new CSVReader(dataFile, false).GetData<double>();

            for (int y = 0;  y < gridCosts.Count; y++) {
                List<double> row = gridCosts[y];

                // The grid must be rectangular (holes or perimeters must be represented with zero terrain costs).
                if (row.Count != gridCosts[0].Count) {
                    throw new ArgumentException($"Cannot have a non-rectangular grid " +
                        $"(row 0 has length {gridCosts[0].Count} but row {y} has length {row.Count}).");
                }

                for (int x = 0; x < row.Count; x++) {
                    // Create this Place
                    GridPlace here = GetPlaceOrCreate((x, y));

                    // Set this Place's cost (error if the cost is negative)
                    if (row[x] < 0.0) {
                        throw new ArgumentException($"Cannot have a negative cost: {row[x]} for {here.Label}");
                    }
                    SetTerrainCost(here.Label, row[x]);

                    // Position bools
                    bool isFstRow = y == 0;
                    bool isLstRow = y == gridCosts.Count - 1;
                    bool isFstCol = x == 0;
                    bool isLstCol = x == row.Count - 1;

                    // Link neighbours
                    if (!isFstRow) here.Neighbours.Add(GetPlaceOrCreate(here.N));
                    if (!isLstRow) here.Neighbours.Add(GetPlaceOrCreate(here.S));
                    if (!isFstCol) here.Neighbours.Add(GetPlaceOrCreate(here.W));
                    if (!isLstCol) here.Neighbours.Add(GetPlaceOrCreate(here.E));

                    if (DiagonalNeighbours) {
                        if (!isFstRow && !isFstCol) here.Neighbours.Add(GetPlaceOrCreate(here.NW));
                        if (!isFstRow && !isLstCol) here.Neighbours.Add(GetPlaceOrCreate(here.NE));
                        if (!isLstRow && !isFstCol) here.Neighbours.Add(GetPlaceOrCreate(here.SW));
                        if (!isLstRow && !isLstCol) here.Neighbours.Add(GetPlaceOrCreate(here.SE));
                    }
                }
            }
        }

        public GridPlace GetPlaceOrCreate((int, int) label) {
            if (Places.TryGetValue(label, out var place)) {
                return (GridPlace)place;
            }
            Places[label] = new GridPlace(label);
            return (GridPlace)Places[label];
        }
    }
}
