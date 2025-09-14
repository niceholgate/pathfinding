using NicUtils;

namespace AStarNickNS;

/*
 * A place supporting AStar (can define a heuristic distance between any two of these with the same coordinate type).
 */
public interface IPlaceAStar<TCoord> : IPlace<TCoord>
{
    double DistanceFrom(IPlaceAStar<TCoord> other, Distances2D.HeuristicType heuristicType);
}