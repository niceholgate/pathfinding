using NicUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using NicUtils.ExtensionMethods;

namespace AStarNickNS
{
    public class GridPlaceGraph : PlaceGraph<(int, int)>
    {
        private static readonly double SQRT2 = Math.Sqrt(2);
        
        private bool DiagonalNeighbours { get; set; }
        
        // null bool means not yet calculated/cache invalidated
        private Dictionary<double, bool?[,]> PathfinderObstacleIntersectionsCache { get; set; }
        
        // don't need to worry about caching invalidation on this one
        // - just stores the last seen coordinate where a pathfinder fits
        public Dictionary<double, (double, double)?[,]> PathfinderFitsCoords { get; set; }

        private double[,] _gridTerrainCosts = new double[1,1];

        private readonly IPathfinderObstacleIntersector _intersector;

        private readonly List<double> _descendingOrderedPathfinderSizes;

        public int GetWidth()
        {
            return _gridTerrainCosts.GetLength(0);
        }
        
        public int GetHeight()
        {
            return _gridTerrainCosts.GetLength(1);
        }
        
        public GridPlaceGraph(bool diagonalNeighbours, IPathfinderObstacleIntersector pathfinderObstacleIntersector)
        {
            DiagonalNeighbours = diagonalNeighbours;
            _intersector = pathfinderObstacleIntersector;
            PathfinderObstacleIntersectionsCache = new Dictionary<double, bool?[,]> { { 0.9, null } };
            PathfinderFitsCoords = new Dictionary<double, (double, double)?[,]> { { 0.9, null } };
            _descendingOrderedPathfinderSizes = PathfinderObstacleIntersectionsCache.Keys
                .OrderByDescending(k => k).ToList();
        }

        public GridPlaceGraph(bool diagonalNeighbours, IPathfinderObstacleIntersector pathfinderObstacleIntersector,
            HashSet<double> pathfinderSizes)
        {
            DiagonalNeighbours = diagonalNeighbours;
            _intersector = pathfinderObstacleIntersector;
            PathfinderObstacleIntersectionsCache = new Dictionary<double, bool?[,]>();
            PathfinderFitsCoords = new Dictionary<double, (double, double)?[,]>();
            foreach (double pathfinderSize in pathfinderSizes)
            {
                PathfinderObstacleIntersectionsCache.Add(pathfinderSize, null);
                PathfinderFitsCoords.Add(pathfinderSize, null);
            }
            _descendingOrderedPathfinderSizes = PathfinderObstacleIntersectionsCache.Keys
                .OrderByDescending(k => k).ToList();
        }
        
        public override double CostToLeave((int, int) from, (int, int) to)
        {
            int dx = from.Item1 - to.Item1;
            int dy = from.Item2 - to.Item2;
            bool isDiagonal = dx * dx + dy * dy == 2;
            if (isDiagonal) return GetTerrainCost(to) * SQRT2;
            return GetTerrainCost(to);
        }
        
        public bool PathfinderCanFitCached(int x, int y, double pathfinderSize)
        {
            if (PathfinderObstacleIntersectionsCache[pathfinderSize][x, y] == null)
            {
                PathfinderFitsCoords[pathfinderSize][x, y] =
                    _intersector.CoordinateWherePathfinderDoesNotIntersectAnyObstacles(x, y, pathfinderSize);
                PathfinderObstacleIntersectionsCache[pathfinderSize][x, y] =
                    PathfinderFitsCoords[pathfinderSize][x, y] == null;
            }
            return !PathfinderObstacleIntersectionsCache[pathfinderSize][x, y].Value;
        }
        
        protected override bool PlaceAccessible((int, int) from, (int, int) to, double pathfinderSize)
        {
            (int xTo, int yTo) = to;
            (int xFrom, int yFrom) = from;
            
            // Prevent weird corner cutting for diagonal movements near to obstacle corners
            int diagType = (xTo - xFrom) * (yTo - yFrom);
            bool principalDiag = diagType == 1;
            bool secondaryDiag = diagType == -1;
            if (principalDiag &&
                (_gridTerrainCosts[Math.Max(xTo, xFrom), Math.Min(yTo, yFrom)] <= 0
                    || _gridTerrainCosts[Math.Min(xTo, xFrom), Math.Max(yTo, yFrom)] <= 0))
            {
                return false;
            }
            if (secondaryDiag &&
                (_gridTerrainCosts[Math.Max(xTo, xFrom), Math.Max(yTo, yFrom)] <= 0
                 || _gridTerrainCosts[Math.Min(xTo, xFrom), Math.Min(yTo, yFrom)] <= 0))
            {
                return false;
            }
            
            return PlaceExists(to) && PathfinderCanFitCached(xTo, yTo, pathfinderSize);
        }

        public double GetTerrainCost((int, int) label)
        {
            (int x, int y) = label;
            // if (!PlaceExists(label)) return 0;
            return _gridTerrainCosts[x, y];
        }
        
        // If the grid changes, recompute PathfinderCanFitCached.
        // Only need to recompute it around cells with newly changed accessibility within a radius equal to half the largest pathfinder size.
        // TODO: if multiple updates are happening nearby to each other, it would be more efficient to make one bigger bounding box
        // and do just a single intersections update to avoid rework. If a player is just placing building in series, can neglect this.
        // But it would be significant if a map underwent a significant terrain change e.g. from an earthquake or flood.
        public void SetTerrainCost((int, int) label, double cost)
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
                    double halfWidth = pathfinderSize / 2;
                    int radius = (int)Math.Ceiling(halfWidth);
                    for (int cellX = x - radius; cellX <= x + radius; cellX++)
                    {
                        if (cellX < 0 || cellX >= _gridTerrainCosts.GetLength(0)) continue;
                        for (int cellY = y - radius; cellY <= y + radius; cellY++)
                        {
                            if (cellY < 0 || cellY >= _gridTerrainCosts.GetLength(1)) continue;
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

        public void BuildFromString(string csvString)
        {
            double[,] gridCosts = ParseCsvToDoubleArray(csvString);
            BuildFromArray(gridCosts);
        }
        
        private double[,] ParseCsvToDoubleArray(string csvString)
        {
            // Split lines (trim to remove empty lines)
            string[] lines = csvString.Trim().Split('\n');

            int rows = lines.Length;
            int cols = lines[0].Split(',').Length;

            double[,] result = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                string[] cells = lines[i].Trim().Split(',');
                for (int j = 0; j < cols; j++)
                {
                    // Handle possible whitespace or empty entries
                    string value = cells[j].Trim();
                    if (double.TryParse(value, out double parsed))
                        result[i, j] = parsed;
                    else
                        result[i, j] = double.NaN; // or 0 if you prefer
                }
            }

            return result;
        }

        public void BuildFromArray(double[,] gridCosts)
        {
            _gridTerrainCosts = gridCosts;
            int height = gridCosts.GetLength(1);
            int width = gridCosts.GetLength(0);
            
            _intersector.GridTerrainCosts = _gridTerrainCosts;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Create this Place
                    GridPlace here = GetPlaceOrCreate((x, y));

                    // Set this Place's cost (error if the cost is negative)
                    if (_gridTerrainCosts[x, y] < 0.0)
                    {
                        throw new ArgumentException($"Cannot have a negative cost: {_gridTerrainCosts[x, y]} for {here.Label}");
                    }

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
                PathfinderFitsCoords[pathfinderSize] = new (double, double)?[width, height];
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
        
        protected override void BuildFromFileCore(string dataFile)
        {
            if (!dataFile.EndsWith(".csv"))
            {
                throw new ArgumentException("GridPlaceGraph only supports building from .csv files");
            }
            
            List<List<double>> gridCosts = new CSVReader(dataFile, false).GetData<double>();
            BuildFromArray(gridCosts.ToRectangularArray());
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
            // public List<GridPlace> SmoothPath(List<GridPlace> originalPath, double pathfinderSize)
            // {
            //     // The smoothed path start at the same place as the original path 
            //     int latestNodeIdx = 0;
            //     List<GridPlace> smoothedPath = new() { originalPath[0] };
            //
            //     int idx = 1;
            //     while (idx < originalPath.Count)
            //     {
            //         GridPlace here = originalPath[idx];
            //         
            //         // The smoothed path ends at the same place as the original path 
            //         if (idx == originalPath.Count - 1)
            //         {
            //             smoothedPath.Add(here);
            //             break;
            //         }
            //         
            //         // Get the intersections data (2 coordinates and 1 terrain cost value)
            //         // for each cell intersected by the line segment between 'here' and the last node
            //         List<CellIntersectionData> intersections = GetCellIntersectionsWithLineSegment(start, end);
            //         
            //         
            //         // If the line segment between 'here' and the last node is blocked,
            //         // the previous path location needs to become a node on the smoothed path ...
            //         if (IsLineSegmentBlocked(originalPath[latestNodeIdx], here, pathfinderSize))
            //         {
            //             latestNodeIdx = idx - 1;
            //             smoothedPath.Add(originalPath[latestNodeIdx]);
            //         }
            //         // Likewise if the line segment becomes slower (due to terrain costs) than the
            //         // ... otherwise continue
            //         
            //         idx++;
            //     }
            //     
            //     return smoothedPath;
            // }
            
            // public bool IsLineSegmentBlocked(GridPlace p1, GridPlace p2, double pathfinderSize)
            // {
            //     // Get the intersected cells
            //     
            //     // Loop over the cells, checking for inaccessible cells
            //     for (GridPlace gridPlace : intersected)
            //     {
            //         // Need to take slow terrain into account too
            //         // Calculate the original path speed, expected smoothed speed,
            //         // and consider it blocked if smoothed is slower?
            //         if (!PathfinderCanFitCached(x, y, pathfinderSize))
            //         {
            //             return true;
            //         }
            //     }
            //     return false;
            // }
            //
            // private List<CellIntersectionData> GetCellIntersectionsWithLineSegment(GridPlace start, GridPlace end)
            // {
            //     
            // }
            
            
        }
    }
    