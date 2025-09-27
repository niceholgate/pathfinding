using NicUtils;
using System;
using System.Collections.Generic;

namespace AStarNickNS
{
    public class GridPlaceGraph : PlaceGraph<(int, int)>
    {
        private bool DiagonalNeighbours { get; init; }

        public GridPlaceGraph(bool diagonalNeighbours)
        {
            DiagonalNeighbours = diagonalNeighbours;
        }

        //public override Dictionary<Place<(int, int)>, double> GetImplicitNeighboursWithCosts(Place<(int, int)> place) {
        //    return new Dictionary<Place<(int, int)>, double>();
        //}

        private double[,] _gridTerrainCosts;

        public override double CostToLeave((int, int) from, (int, int) to)
        {
            return GetTerrainCost(to);
        }

        // Better to precompute this over the grid for pathfinders of various sizes.
        // Only need to recompute it around newly inaccessible cells (within a radius equal to half the largest pathfinder size).
        public bool PathfinderCanFit((int, int) label, double pathfinderSize)
        {
            double halfWidth = pathfinderSize / 2;
            double radiusSq = halfWidth * halfWidth;
            (double cx, double cy) = (label.Item1, label.Item2);
            int radius = (int)Math.Ceiling(halfWidth);

            for (int cellX = label.Item1 - radius; cellX <= label.Item1 + radius; cellX++)
            {
                for (int cellY = label.Item2 - radius; cellY <= label.Item2 + radius; cellY++)
                {
                    if (GetTerrainCost((cellX, cellY)) <= 0)
                    {
                        // This cell is an obstacle. Check for intersection with the pathfinder's circle.

                        // Find the closest point on the cell's square to the circle's center.
                        double closestX = Math.Max(cellX - 0.5, Math.Min(cx, cellX + 0.5));
                        double closestY = Math.Max(cellY - 0.5, Math.Min(cy, cellY + 0.5));

                        // Calculate the distance squared from the circle's center to this closest point.
                        double deltaX = cx - closestX;
                        double deltaY = cy - closestY;
                        double distanceSq = (deltaX * deltaX) + (deltaY * deltaY);

                        // If the distance is less than the circle's radius squared, there is an intersection.
                        if (distanceSq < radiusSq)
                        {
                            return false; // Collision detected.
                        }
                    }
                }
            }

            return true; // No collisions found.
        }

        public override double GetTerrainCost((int, int) label)
        {
            (int x, int y) = label;
            if (x < 0 || x >= _gridTerrainCosts.GetLength(0)
                || y < 0 || y >= _gridTerrainCosts.GetLength(1)) return 0;
            return _gridTerrainCosts[x, y];
        }

        public override void SetTerrainCost((int, int) label, double cost)
        {
            (int x, int y) = label;
            _gridTerrainCosts[x, y] = cost;
        }

        protected override void BuildCore(string dataFile)
        {
            List<List<double>> gridCosts = new CSVReader(dataFile, false).GetData<double>();
            int height = gridCosts.Count;
            int width = gridCosts[0].Count;
            _gridTerrainCosts = new double[width, height];

            for (int y = 0; y < height; y++)
            {
                List<double> row = gridCosts[y];

                // The grid must be rectangular (holes or perimeters must be represented with zero terrain costs).
                if (row.Count != width)
                {
                    throw new ArgumentException($"Cannot have a non-rectangular grid " +
                                                $"(row 0 has length {width} but row {y} has length {row.Count}).");
                }

                for (int x = 0; x < width; x++)
                {
                    // Create this Place
                    GridPlace here = GetPlaceOrCreate((x, y));

                    // Set this Place's cost (error if the cost is negative)
                    if (row[x] < 0.0)
                    {
                        throw new ArgumentException($"Cannot have a negative cost: {row[x]} for {here.Label}");
                    }

                    SetTerrainCost(here.Label, row[x]);

                    // Position bools
                    bool isFstRow = y == 0;
                    bool isLstRow = y == height - 1;
                    bool isFstCol = x == 0;
                    bool isLstCol = x == width - 1;

                    // Link neighbours
                    if (!isFstRow) here.Neighbours.Add(GetPlaceOrCreate(here.N));
                    if (!isLstRow) here.Neighbours.Add(GetPlaceOrCreate(here.S));
                    if (!isFstCol) here.Neighbours.Add(GetPlaceOrCreate(here.W));
                    if (!isLstCol) here.Neighbours.Add(GetPlaceOrCreate(here.E));

                    if (DiagonalNeighbours)
                    {
                        if (!isFstRow && !isFstCol) here.Neighbours.Add(GetPlaceOrCreate(here.NW));
                        if (!isFstRow && !isLstCol) here.Neighbours.Add(GetPlaceOrCreate(here.NE));
                        if (!isLstRow && !isFstCol) here.Neighbours.Add(GetPlaceOrCreate(here.SW));
                        if (!isLstRow && !isLstCol) here.Neighbours.Add(GetPlaceOrCreate(here.SE));
                    }
                }
            }
        }

        public GridPlace GetPlaceOrCreate((int, int) label)
        {
            if (Places.TryGetValue(label, out var place))
            {
                return (GridPlace)place;
            }

            Places[label] = new GridPlace(label);
            return (GridPlace)Places[label];
        }
    }
}