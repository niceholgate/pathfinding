using System.Collections.Generic;

namespace AStarNickNS {
    public interface IPathfindingSolver<out TPlace> where TPlace : IPlace {
        public void Solve();

        public IEnumerable<TPlace> ReconstructPath();
    }
}
