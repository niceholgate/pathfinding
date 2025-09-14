using System.Collections.Generic;

namespace AStarNickNS
{
    public interface IPathfindingSolver<out TPlace, TCoord> where TPlace : IPlace<TCoord>
    {
        public void Solve();

        public IEnumerable<TPlace> ReconstructPath();
    }
}