using System.Collections.Generic;
using System.Threading;

namespace AStarNickNS
{
    public interface IPathfindingSolver<out TPlace, TCoord> where TPlace : IPlace<TCoord>
    {
        public IEnumerable<TPlace> SolvePath(IPlace<TCoord> start, IPlace<TCoord> target, CancellationToken token, double pathfinderSize);

    }
}