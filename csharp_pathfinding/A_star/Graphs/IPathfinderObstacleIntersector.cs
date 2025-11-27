namespace AStarNickNS;

public interface IPathfinderObstacleIntersector
{
    public (double, double)? CoordinateWherePathfinderDoesNotIntersectAnyObstacles(int x, int y, double pathfinderSize);
    
    public double[,] GridTerrainCosts { get; set; }
}