using System.Collections.Generic;

namespace AStarNickNS
{

    public interface IPathfinderObstacleIntersector
    {
        public OccupiableCellCoordinates CoordinatesWherePathfinderDoesNotIntersectAnyObstacles(int x, int y,
            float pathfinderSize);

        public float[,] GridTerrainCosts { get; set; }
    }
}