using System.Collections.Generic;

namespace AStarNickNS
{
    public interface IPathfindingSolver<out TPlace, TCoord> where TPlace : IPlace<TCoord>
    {
        public IEnumerable<TPlace> SolvePath(IPlace<TCoord> start, IPlace<TCoord> target);

    }
}