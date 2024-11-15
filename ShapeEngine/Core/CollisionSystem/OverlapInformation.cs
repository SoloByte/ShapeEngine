using ShapeEngine.Core.CollisionSystem;

namespace ShapeEngine.Core.CollisionSystem;

public class OverlapInformation : List<Overlap>
{
    public readonly CollisionObject Other;
    public readonly CollisionObject Self;
        
    public OverlapInformation(CollisionObject self, CollisionObject other)
    {
        Self = self;
        Other = other;
    }

    public OverlapInformation(CollisionObject self, CollisionObject other, List<Overlap> overlaps)
    {
        Self = self;
        Other = other;
        AddRange(overlaps);
    }

    public Overlap? PopOverlap(Collider self, Collider other)
    {
        foreach (var overlap in this)
        {
            if (overlap.Self == self && overlap.Other == other)
            {
                Remove(overlap);
                return overlap;
            }
        }
        return null;
    }
}