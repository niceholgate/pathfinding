using NicUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public Dictionary<double, OccupiableCellCoordinates[,]> PathfinderFitsCoords { get; set; }

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
            PathfinderFitsCoords = new Dictionary<double, OccupiableCellCoordinates[,]>() { { 0.9, null } };
            _descendingOrderedPathfinderSizes = PathfinderObstacleIntersectionsCache.Keys
                .OrderByDescending(k => k).ToList();
        }

        public GridPlaceGraph(bool diagonalNeighbours, IPathfinderObstacleIntersector pathfinderObstacleIntersector,
            HashSet<double> pathfinderSizes)
        {
            DiagonalNeighbours = diagonalNeighbours;
            _intersector = pathfinderObstacleIntersector;
            PathfinderObstacleIntersectionsCache = new Dictionary<double, bool?[,]>();
            PathfinderFitsCoords = new Dictionary<double, OccupiableCellCoordinates[,]>();
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
                OccupiableCellCoordinates fitCoordinates =
                    _intersector.CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(x, y, pathfinderSize);
                PathfinderFitsCoords[pathfinderSize][x, y] = fitCoordinates;
                PathfinderObstacleIntersectionsCache[pathfinderSize][x, y] = !fitCoordinates.Occupiable();
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
                                !PathfinderObstacleIntersectionsCache[previousPathfinderSize.Value][cellX, cellY].Value &&
                                PathfinderFitsCoords[previousPathfinderSize.Value][cellX, cellY].AllCoordsOccupiable)
                            {
                                PathfinderFitsCoords[pathfinderSize][cellX, cellY] =
                                    PathfinderFitsCoords[previousPathfinderSize.Value][cellX, cellY];
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
                PathfinderFitsCoords[pathfinderSize] = new OccupiableCellCoordinates[width, height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        if (previousPathfinderSize != null &&
                            !PathfinderObstacleIntersectionsCache[previousPathfinderSize.Value][x, y].Value &&
                            PathfinderFitsCoords[previousPathfinderSize.Value][x, y].AllCoordsOccupiable)
                        {
                            PathfinderFitsCoords[pathfinderSize][x, y] =
                                PathfinderFitsCoords[previousPathfinderSize.Value][x, y];
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
            
            List<List<double>> gridCosts = new CSVReader(dataFile, false).GetData<double>(false);
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
       
        public List<GridPlace> SmoothPath(List<GridPlace> originalPath, double pathfinderSize)
        {
            // Check that the original path is valid
            // foreach (GridPlace place in originalPath)
            // {
            //     if (GetTerrainCost(place.Label) <= 0)
            //     {
            //         throw new ArgumentException($"Cannot smooth a path that goes through blocked cell/s! (Label = {place.Label})");
            //     }
            // }
            
            // If the original path has 2 or fewer nodes, it can't be smoothed
            if (originalPath.Count <= 2) return new List<GridPlace>(originalPath);
            
           // The smoothed path starts at the same place as the original path 
           int latestNodeIdx = 0;
           List<GridPlace> smoothedPath = new() { originalPath[0] };
       
           int idx = 1;
           while (idx < originalPath.Count)
           {
               int stepback = (int)Math.Ceiling(pathfinderSize) + 1;
               
               // The smoothed path ends at the same place as the original path 
               if (idx == originalPath.Count - 1)
               {
                   // If the last line segment becomes blocked and the previous original path point is not already a node, make it a node too.
                   (float, float) startA = originalPath[latestNodeIdx].Label;
                   (float, float) endA = originalPath[idx].Label;
                   List<CellIntersectionData> intersectedCellsA = GridCellIntersections.GetCellIntersectionsWithLineSegment(
                       startA, endA);
                   bool isLineSegmentBlockedA = intersectedCellsA.Any(cell => !PathfinderCanFitCached(cell.x, cell.y, pathfinderSize));
                   if (isLineSegmentBlockedA && smoothedPath[^1] != originalPath[idx - 1])
                   {
                       smoothedPath.Add(originalPath[idx-1]);
                   }
                   smoothedPath.Add(originalPath[idx]);
                   break;
               }
               // Idea: Since can't rely on stepback logic in line-of-sight check
               // to prevent units sliding on walls at the end of the path,
               //  extend the original path in its final direction by one more cell for purposes of line of sight checking?
               
               // Get the intersections data (2 coordinates and 1 terrain cost value)
               // for each cell intersected by the line segment between 'here' and the last node.
               // On the last node, we should choose the corner of the cell that minimises the angle between the angle between this smooth path line segment and the previous
               // one - minimises the chance of clipping a blockage at the corner.
               // This worked for some situations, but made it worse for others where the acute angle ought to have been minimised.
               // Line segment proximity to to blockages is probably the better solution.
               // if (latestNodeIdx > 0)
               // {
               //     (float, float) prev = originalPath[latestNodeIdx - 1].Label;
               //     List<(float, float)> candidates = new()
               //     {
               //         (end.Item1 - 0.49f, end.Item2 - 0.49f),
               //         (end.Item1 - 0.49f, end.Item2 + 0.49f),
               //         (end.Item1 + 0.49f, end.Item2 - 0.49f),
               //         (end.Item1 + 0.49f, end.Item2 + 0.49f),
               //     };
               //     end = GetThirdPointThatMinimisesAcuteAngle(prev, start, candidates);
               // }
               
               (float, float) start = originalPath[latestNodeIdx].Label;
               (float, float) end = originalPath[idx].Label;
               List<CellIntersectionData> intersectedCells = GridCellIntersections.GetCellIntersectionsWithLineSegment(
                   start, end);
               
               // If the line segment between 'here' and the last node is blocked,
               // or if the line segment goes too close to a blockage, (not yet implemented, see pathsmoothingissue-treat-line-segment-as-blocked-if-passes-near-blockage.png)
               ////// (as a temporary solution, let's try selecting the new smoothed node as "idx - 1 - ceil(pathfinderSize)" instead of "idx - 1".)
               // or if the line segment becomes slower (due to terrain costs) than the original path segment,
               // then the previous path location needs to become a node on the smoothed path...
               bool isLineSegmentBlocked = intersectedCells.Any(cell => !PathfinderCanFitCached(cell.x, cell.y, pathfinderSize));
               List<GridPlace> originalPathSegment = originalPath.GetRange(latestNodeIdx, idx - latestNodeIdx + 1);
               if (isLineSegmentBlocked
                   || IsLineSegmentSlowerThanOriginalPathSegment(intersectedCells, originalPathSegment))
               {
                   latestNodeIdx = Math.Max(idx - stepback, latestNodeIdx + 1);
                   
                   smoothedPath.Add(originalPath[latestNodeIdx]);
                   
                   // idx for next candidate node always resets to 2 ahead of the latest node
                   idx = latestNodeIdx + 2;
               }
               else
               {
                   // ... otherwise continue
                   idx++;
               }
           }
           
           return smoothedPath;
        }
            
        private bool IsLineSegmentSlowerThanOriginalPathSegment(
            List<CellIntersectionData> intersectedCells, List<GridPlace> originalPathSegment)
        {
            HashSet<double> terrainCostsSeen = new() {GetTerrainCost(originalPathSegment[0].Label)};
            double originalPathSegmentCost = 0.0;
            double lineSegmentCost = 0.0;
            for (int i = 1; i < originalPathSegment.Count; i++)
            {
                terrainCostsSeen.Add(GetTerrainCost(originalPathSegment[i].Label));
                originalPathSegmentCost += CostToLeave(originalPathSegment[i-1].Label, originalPathSegment[i].Label);
            }
            foreach (CellIntersectionData cell in intersectedCells)
            {
                double thisCost = GetTerrainCost((cell.x, cell.y));
                terrainCostsSeen.Add(thisCost);
                lineSegmentCost += cell.IntersectedDistance * thisCost;
            }
            // (If the terrain costs are all identical, the new line segment can't be slower, because it is a straight line,
            // whereas the original path may have turns.)
            if (terrainCostsSeen.Count == 1) return false;
            
            return lineSegmentCost > originalPathSegmentCost;
        }

        // public (float, float) GetThirdPointThatMinimisesAcuteAngle((float, float) pointA, (float, float) pointB, List<(float, float)> candidates)
        // {
        //     Vector2 A = new Vector2(pointA.Item1, pointA.Item2);
        //     Vector2 B = new Vector2(pointB.Item1, pointB.Item2);
        //     
        //     List<Vector2> candidateVectors = candidates
        //         .Select(c => new Vector2(c.Item1, c.Item2))
        //         .ToList();
        //     
        //     Vector2 vBA = A - B;
        //     float magBA = vBA.Length(); // Or use vBA.magnitude in Unity
        //
        //     Vector2 bestPoint = candidateVectors[0];
        //     float maxScore = -2f; // Cosine ranges from -1 to 1
        //
        //     foreach (var C in candidateVectors)
        //     {
        //         Vector2 vBC = C - B;
        //         float magBC = vBC.Length();
        //
        //         // Dot product divided by magnitudes gives the Cosine of the angle
        //         float score = Vector2.Dot(vBA, vBC) / (magBA * magBC);
        //
        //         if (score > maxScore)
        //         {
        //             maxScore = score;
        //             bestPoint = C;
        //         }
        //     }
        //     return (bestPoint.X, bestPoint.Y);
        // }
            
    }
}
    