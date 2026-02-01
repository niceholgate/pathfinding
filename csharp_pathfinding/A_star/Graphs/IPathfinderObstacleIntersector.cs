using System.Collections.Generic;

namespace AStarNickNS
{

    public interface IPathfinderObstacleIntersector
    {
        public OccupiableCellCoordinates CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(int x, int y,
            double pathfinderSize);

        public double[,] GridTerrainCosts { get; set; }
    }
}