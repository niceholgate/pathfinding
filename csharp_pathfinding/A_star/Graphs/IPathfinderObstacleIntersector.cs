using System.Collections.Generic;

namespace AStarNickNS
{

    public interface IPathfinderObstacleIntersector
    {
        public OccupiableCellCoordinates GetOccupiableCellCoordinates(int x, int y,
            float pathfinderSize, bool[,] blockages, string blockageLayer);
    }
}