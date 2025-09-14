using System.Collections.Generic;

namespace AStarNickNS
{
    public interface IPathfindingSolver<out TPlace, TCoord> where TPlace : IPlace<TCoord>
    {
        public void Solve(IPlace<TCoord> start, IPlace<TCoord> target);

        public IEnumerable<TPlace> ReconstructPath(IPlace<TCoord> start, IPlace<TCoord> target);
    }
}