using System;

namespace AStarNickNS;

public readonly struct PathfinderAttributes : IEquatable<PathfinderAttributes>
{
    public float Size { get; }
    public string BlockageLayer { get; }

    public PathfinderAttributes(float size, string blockageLayer)
    {
        Size = size;
        BlockageLayer = blockageLayer;
    }

    public bool Equals(PathfinderAttributes other)
    {
        return Size.Equals(other.Size) && BlockageLayer == other.BlockageLayer;
    }

    public override bool Equals(object obj)
    {
        return obj is PathfinderAttributes other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Size.GetHashCode();
            hash = hash * 23 + (BlockageLayer?.GetHashCode() ?? 0);
            return hash;
        }
    }
}