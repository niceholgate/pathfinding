namespace AStarNickNS;

public interface IPathfinderObstacleIntersector
{
    public bool PathfinderIntersectsWithObstacles(int x, int y, double pathfinderSize);
    
    public double[,] GridTerrainCosts { get; set; }
}