using NicUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using NicUtils.ExtensionMethods;

namespace AStarNickNS
{
    public class GridPlaceGraph : PlaceGraph<(int, int)>
    {
        private bool DiagonalNeighbours { get; init; }
        
        private Dictionary<double, bool?[,]> PathfinderObstacleIntersectionsCache { get; init; }

        private double[,] _gridTerrainCosts;

        private IPathfinderObstacleIntersector _intersector;

        private List<double> _descendingOrderedPathfinderSizes;
        
        public GridPlaceGraph(bool diagonalNeighbours, IPathfinderObstacleIntersector pathfinderObstacleIntersector)
        {
            DiagonalNeighbours = diagonalNeighbours;
            _intersector = pathfinderObstacleIntersector;
            PathfinderObstacleIntersectionsCache = new Dictionary<double, bool?[,]> { { 0.9, null } };
            _descendingOrderedPathfinderSizes = PathfinderObstacleIntersectionsCache.Keys
                .OrderByDescending(k => k).ToList();
        }

        public GridPlaceGraph(bool diagonalNeighbours, IPathfinderObstacleIntersector pathfinderObstacleIntersector,
            HashSet<double> pathfinderSizes)
        {
            DiagonalNeighbours = diagonalNeighbours;
            _intersector = pathfinderObstacleIntersector;
            PathfinderObstacleIntersectionsCache = new Dictionary<double, bool?[,]>();
            foreach (var pathfinderSize in pathfinderSizes)
                PathfinderObstacleIntersectionsCache.Add(pathfinderSize, null);
            _descendingOrderedPathfinderSizes = PathfinderObstacleIntersectionsCache.Keys
                .OrderByDescending(k => k).ToList();
        }

        public override double CostToLeave((int, int) from, (int, int) to)
        {
            return GetTerrainCost(to);
        }
        
        public bool PathfinderCanFitCached(int x, int y, double pathfinderSize)
        {
            PathfinderObstacleIntersectionsCache[pathfinderSize][x, y] ??=
                _intersector.PathfinderIntersectsWithObstacles(x, y, pathfinderSize);
            return !PathfinderObstacleIntersectionsCache[pathfinderSize][x, y].Value;
        }
                
        public override double GetTerrainCost((int, int) label)
        {
            (int x, int y) = label;
            if (CoordinateOutOfBounds(x, y)) return 0;
            return _gridTerrainCosts[x, y];
        }

        private bool CoordinateOutOfBounds(int x, int y)
        {
            return x < 0 || x >= _gridTerrainCosts.GetLength(0)
                || y < 0 || y >= _gridTerrainCosts.GetLength(1);
        }
        
        // If the grid changes, recompute PathfinderCanFitCached.
        // Only need to recompute it around cells with newly changed accessibility within a radius equal to half the largest pathfinder size.
        // TODO: if multiple updates are happening nearby to each other, it would be more efficient to make one bigger bounding box
        // and do just a single intersections update to avoid rework.
        public override void SetTerrainCost((int, int) label, double cost)
        {
            (int x, int y) = label;

            double oldCost = _gridTerrainCosts[x, y];
            _gridTerrainCosts[x, y] = cost;
            
            // Only need to recompute PathfinderCanFitCached if there's a change in accessibility.
            if ((oldCost <= 0 && cost > 0) || (cost <= 0 && oldCost > 0))
            {
                double? previousPathfinderSize = null;
                foreach (double pathfinderSize in _descendingOrderedPathfinderSizes)
                {
                    double halfWidth = _descendingOrderedPathfinderSizes.Max() / 2;
                    int radius = (int)Math.Ceiling(halfWidth);
                    for (int cellX = x - radius; cellX <= x + radius; cellX++)
                    {
                        for (int cellY = y - radius; cellY <= y + radius; cellY++)
                        {
                            if (previousPathfinderSize != null &&
                                !PathfinderObstacleIntersectionsCache[previousPathfinderSize.Value][cellX, cellY].Value)
                            {
                                PathfinderObstacleIntersectionsCache[pathfinderSize][cellX, cellY] = false;
                                continue;
                            }

                            PathfinderObstacleIntersectionsCache[pathfinderSize][cellX, cellY] = null;
                            PathfinderCanFitCached(cellX, cellY, pathfinderSize);
                        }
                    }

                    previousPathfinderSize = pathfinderSize;
                }
            }
        }

        // public void BuildFromArray(double[,] array)
        // {
        //     
        // }
        
        protected override void BuildFromFileCore(string dataFile)
        {
            if (!dataFile.EndsWith(".csv"))
            {
                throw new ArgumentException("GridPlaceGraph only supports building from .csv files");
            }
            
            List<List<double>> gridCosts = new CSVReader(dataFile, false).GetData<double>();
            int height = gridCosts.Count;
            int width = gridCosts[0].Count;
            // _gridTerrainCosts = new double[width, height];
            _gridTerrainCosts = gridCosts.ToRectangularArray();
            _intersector.GridTerrainCosts = _gridTerrainCosts;
            
            for (int y = 0; y < height; y++)
            {
                List<double> row = gridCosts[y];

                for (int x = 0; x < width; x++)
                {
                    // Create this Place
                    GridPlace here = GetPlaceOrCreate((x, y));

                    // Set this Place's cost (error if the cost is negative)
                    if (row[x] < 0.0)
                    {
                        throw new ArgumentException($"Cannot have a negative cost: {row[x]} for {here.Label}");
                    }

                    // _gridTerrainCosts[x, y] = row[x];

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
            
            // Assess pathfinders in descending order. If the next biggest pathfinder can fit in a certain place, so can the current one.
            double? previousPathfinderSize = null;
            foreach (double pathfinderSize in _descendingOrderedPathfinderSizes)
            {
                PathfinderObstacleIntersectionsCache[pathfinderSize] = new bool?[width, height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (previousPathfinderSize != null &&
                            !PathfinderObstacleIntersectionsCache[previousPathfinderSize.Value][x, y].Value)
                        {
                            PathfinderObstacleIntersectionsCache[pathfinderSize][x, y] = false;
                            continue;
                        }
                        PathfinderCanFitCached(x, y, pathfinderSize);
                    }
                }

                previousPathfinderSize = pathfinderSize;
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

        public double[,] GetGridTerrainCosts()
        {
            return (double[,])_gridTerrainCosts.Clone();
        }
    }
}