using NicUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NicUtils.ExtensionMethods;

namespace AStarNickNS
{
    public class GridPlaceGraph : PlaceGraph<(int, int)>
    {
        private bool DiagonalNeighbours { get; set; }
        
        private float[,] _gridTerrainCosts = new float[1,1];
        private bool[,] _blockages = new bool[1,1];

        private CachingPathfinderObstacleIntersector _intersector;

        private readonly List<float> _descendingOrderedPathfinderSizes;

        public int GetWidth()
        {
            return _gridTerrainCosts.GetLength(0);
        }
        
        public int GetHeight()
        {
            return _gridTerrainCosts.GetLength(1);
        }
        
        public GridPlaceGraph(bool diagonalNeighbours)
            : this(diagonalNeighbours, new HashSet<float> { 0.9f })
        {
        }

        public GridPlaceGraph(bool diagonalNeighbours, HashSet<float> pathfinderSizes)
        {
            DiagonalNeighbours = diagonalNeighbours;
            _descendingOrderedPathfinderSizes = pathfinderSizes.OrderByDescending(k => k).ToList();
        }
        
        public override float CostToLeave((int, int) from, (int, int) to)
        {
            int dx = from.Item1 - to.Item1;
            int dy = from.Item2 - to.Item2;
            bool isDiagonal = dx * dx + dy * dy == 2;
            if (isDiagonal) return GetTerrainCost(to) * GeometryUtils.SQRT2;
            return GetTerrainCost(to);
        }
        
        public bool PathfinderCanFitCached(int x, int y, float pathfinderSize)
        {
            return _intersector.IsOccupiable(pathfinderSize, x, y, _blockages);
        }
        
        public OccupiableCellCoordinates PathfinderFitsCoords(int x, int y, float pathfinderSize)
        {
            return _intersector.GetOccupiableCellCoordinates(x, y, pathfinderSize, _blockages);
        }
        
        protected override bool PlaceAccessible((int, int) from, (int, int) to, float pathfinderSize)
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

            if (!GeometryUtils.CircleFitsOnBoundary(diagType, xFrom, yFrom, xTo, yTo, pathfinderSize, _blockages))
            {
                return false;
            }
            
            
            return PlaceExists(to) && PathfinderCanFitCached(xTo, yTo, pathfinderSize);
        }

        public float GetTerrainCost((int, int) label)
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
        public void SetTerrainCost((int, int) label, float cost)
        {
            (int x, int y) = label;

            float oldCost = _gridTerrainCosts[x, y];
            _gridTerrainCosts[x, y] = cost;
            _blockages[x, y] = cost <= 0;
            
            // Only need to recompute PathfinderCanFitCached if there's a change in accessibility.
            if ((oldCost <= 0 && cost > 0) || (cost <= 0 && oldCost > 0))
            {
                // Assess pathfinders in descending order. If the next biggest pathfinder can fit in a certain place, so can the current one.
                foreach (float pathfinderSize in _descendingOrderedPathfinderSizes)
                {
                    float halfWidth = pathfinderSize / 2;
                    int radius = (int)MathF.Ceiling(halfWidth);
                    for (int cellX = x - radius; cellX <= x + radius; cellX++)
                    {
                        if (cellX < 0 || cellX >= _gridTerrainCosts.GetLength(0)) continue;
                        for (int cellY = y - radius; cellY <= y + radius; cellY++)
                        {
                            if (cellY < 0 || cellY >= _gridTerrainCosts.GetLength(1)) continue;
                            if (pathfinderSize.Equals(_descendingOrderedPathfinderSizes[0])) _intersector.Invalidate(cellX, cellY);
                            PathfinderCanFitCached(cellX, cellY, pathfinderSize);
                        }
                    }
                }
            }
        }

        public void BuildFromString(string csvString)
        {
            float[,] gridCosts = ParseCsvToFloatArray(csvString);
            BuildFromArray(gridCosts);
        }
        
        private float[,] ParseCsvToFloatArray(string csvString)
        {
            // Split lines (trim to remove empty lines)
            string[] lines = csvString.Trim().Split('\n');

            int rows = lines.Length;
            int cols = lines[0].Split(',').Length;

            float[,] result = new float[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                string[] cells = lines[i].Trim().Split(',');
                for (int j = 0; j < cols; j++)
                {
                    // Handle possible whitespace or empty entries
                    string value = cells[j].Trim();
                    if (float.TryParse(value, out float parsed))
                        result[i, j] = parsed;
                    else
                        result[i, j] = float.NaN; // or 0 if you prefer
                }
            }

            return result;
        }

        public void BuildFromArray(float[,] gridCosts)
        {
            _gridTerrainCosts = gridCosts;
            int height = gridCosts.GetLength(1);
            int width = gridCosts.GetLength(0);
            _blockages = new bool[width, height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _blockages[x, y] = _gridTerrainCosts[x, y] <= 0;
                    // Create this Place
                    GridPlace here = GetPlaceOrCreate((x, y));

                    // Set this Place's cost (error if the cost is negative)
                    if (_gridTerrainCosts[x, y] < 0.0f)
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
            
            _intersector = new CachingPathfinderObstacleIntersector(width, height, _descendingOrderedPathfinderSizes);
            
            // Assess pathfinders in descending order. If the next biggest pathfinder can fit in a certain place, so can the current one.
            foreach (float pathfinderSize in _descendingOrderedPathfinderSizes)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        PathfinderCanFitCached(x, y, pathfinderSize);
                    }
                }
            }
        }
        
        protected override void BuildFromFileCore(string dataFile)
        {
            if (!dataFile.EndsWith(".csv"))
            {
                throw new ArgumentException("GridPlaceGraph only supports building from .csv files");
            }
            
            List<List<float>> gridCosts = new CSVReader(dataFile, false).GetData<float>(false);
            BuildFromArray(gridCosts.ToRectangularArray());
        }

        public GridPlace GetPlaceOrCreate((int, int) label)
        {
            if (Places.TryGetValue(label, out var place)) return (GridPlace)place;
            Places[label] = new GridPlace(label);
            return (GridPlace)Places[label];
        }

        /*
         * Turn a Dijkstra path (List<GridPlace> which only indicates which cells to visit - not which corners of those cells can+should be used)
         * into the actual path to follow wrt. where the pathfinder fits. This involves choosing cell corners which are accessible to the pathfinder (owing to its size)
         * and which will help prevent it from sliding along corner obstacles due to over-smoothing - this is achieved by choosing corners that are maximally
         * distant from their nearest obstacle (pre-calculated inside the GridPlaceGraph, according to pathfinder size).
         */
        public List<(float, float)> GetOccupiablePath(List<GridPlace> originalPath, float pathfinderSize,
            CancellationToken token=new())
        {
            // TODO: what to do if the original path is only 1 long?
            List<(float, float)> occPath = new();
            
            (int x, int y) = originalPath[0].Label;
            OccupiableCellCoordinates firstPlace = PathfinderFitsCoords(x, y, pathfinderSize);
            (x, y) = originalPath[0].Label;
            occPath.Add(GetBestNextPathPosition((x, y), firstPlace));
        
            for (int i = 1; i < originalPath.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                (x, y) = originalPath[i].Label;
                OccupiableCellCoordinates nextPlace = PathfinderFitsCoords(x, y, pathfinderSize);
                occPath.Add(GetBestNextPathPosition(occPath[^1], nextPlace));
            }
            
            return occPath;
        }
        
        private (float, float) GetBestNextPathPosition((float, float) refCoords,
            OccupiableCellCoordinates nextPlace)
        {
            if (nextPlace.Centre != null)
            {
                // If every coordinate can be occupied, or if only the center can be occupied, go to the center
                if (nextPlace.AllCoordsOccupiable || nextPlace.CornersFarthestFromBlockages.Count == 0) return nextPlace.Centre.Value;
            }
        
            // If only one corner is farthest from blockages, go to that corner
            if (nextPlace.CornersFarthestFromBlockages.Count == 1) return nextPlace.CornersFarthestFromBlockages[0];
        
            // If two corners are farthest from blockages, go to the one closest to refCoords
            (float, float) c1 = nextPlace.CornersFarthestFromBlockages[0];
            (float, float) c2 = nextPlace.CornersFarthestFromBlockages[1];
            if (MathF.Abs(c1.Item1 - c2.Item1) < 1E-3f)
            {
                if (MathF.Abs(c1.Item2 - refCoords.Item2) < MathF.Abs(c2.Item2 - refCoords.Item2))
                {
                    return c1;
                }
                return c2;
            }
            // These corners should never be diagonally opposed, so the alternative is c1.Item2 == c2.Item2
            if (MathF.Abs(c1.Item1 - refCoords.Item1) < MathF.Abs(c2.Item1 - refCoords.Item1))
            {
                return c1;
            }
            return c2;
        }
        
        public List<(float, float)> SmoothPath(List<(float, float)> occupiablePath, List<GridPlace> originalPath,
            float pathfinderSize, CancellationToken token=new())
        {
            // If the original path has 2 or fewer nodes, it can't be smoothed
            if (occupiablePath.Count <= 2) return new List<(float, float)>(occupiablePath);
            
            // The smoothed path starts at the same place as the original path 
            int latestNodeIdx = 0;
            List<(float, float)> smoothedPath = new() { occupiablePath[0] };
       
            int idx = 0;
            while (idx < occupiablePath.Count)
            {
                token.ThrowIfCancellationRequested();
                
                idx++;
                // The smoothed path ends at the same place as the original path 
                if (idx == occupiablePath.Count - 1)
                {
                    smoothedPath.Add(occupiablePath[idx]);
                    break;
                }
               
                (float, float) start = occupiablePath[latestNodeIdx];
                (float, float) end = occupiablePath[idx];
                List<CellIntersectionData> intersectedCells =
                    GridCellIntersections.GetCellIntersectionsWithLineSegment(start, end);
               
                // If the line segment between 'here' and the last node is blocked,
                // or if the line segment goes too close to a blockage,
                // or if the line segment becomes slower (due to terrain costs) than the original path segment,
                // then the previous path location needs to become a node on the smoothed path...
               
                bool lineSegmentBlocked = intersectedCells.Any(cell => !PathfinderCanFitCached(cell.x, cell.y, pathfinderSize));
                if (lineSegmentBlocked ||
                    LineSegmentGoesTooCloseToBlockage(intersectedCells, pathfinderSize, start, end) ||
                    IsLineSegmentSlowerThanOriginalPathSegment(intersectedCells, originalPath.GetRange(latestNodeIdx, idx - latestNodeIdx + 1)))
                {
                    latestNodeIdx = idx - 1;
                    smoothedPath.Add(occupiablePath[latestNodeIdx]);
                }
                // ... otherwise continue
            }
           
            return smoothedPath;
        }

        private bool LineSegmentGoesTooCloseToBlockage(List<CellIntersectionData> intersectedCells,
            float pathfinderSize, (float, float) start, (float, float) end)
        {
            return intersectedCells.Any(intersectedCell =>
                PathfinderFitsCoords(intersectedCell.x, intersectedCell.y, pathfinderSize)
                    .NearestBlockedCorners
                    .Select(blockedCorner => GeometryUtils.GetDistanceToLineSegment(start, end, blockedCorner))
                    .Any(distanceBetweenLineAndBlockedCorner => distanceBetweenLineAndBlockedCorner < pathfinderSize / 2));
        }

        // TODO: replace originalPathSegment cost with the intersection-data-cost of the occupiablePath?
        private bool IsLineSegmentSlowerThanOriginalPathSegment(
            List<CellIntersectionData> intersectedCells, List<GridPlace> originalPathSegment)
        {
            HashSet<float> terrainCostsSeen = new() {GetTerrainCost(originalPathSegment[0].Label)};
            float originalPathSegmentCost = 0.0f;
            float lineSegmentCost = 0.0f;
            for (int i = 1; i < originalPathSegment.Count; i++)
            {
                terrainCostsSeen.Add(GetTerrainCost(originalPathSegment[i].Label));
                originalPathSegmentCost += CostToLeave(originalPathSegment[i-1].Label, originalPathSegment[i].Label);
            }
            foreach (CellIntersectionData cell in intersectedCells)
            {
                float thisCost = GetTerrainCost((cell.x, cell.y));
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
    