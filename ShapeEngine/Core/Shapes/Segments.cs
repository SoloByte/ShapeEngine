using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;
using ShapeEngine.Random;

namespace ShapeEngine.Core.Shapes;

public class Segments : ShapeList<Segment>
{
    #region Constructors
    public Segments() { }
    //public Segments(IShape shape) { AddRange(shape.GetEdges()); }
    public Segments(IEnumerable<Segment> edges) { AddRange(edges); }
    #endregion

    #region Equals & HashCode
    public bool Equals(Segments? other)
    {
        if (other == null) return false;
        if (Count != other.Count) return false;
        for (var i = 0; i < Count; i++)
        {
            if (this[i] != other[i]) return false;
        }
        return true;
    }
    public override int GetHashCode() { return SUtils.GetHashCode(this); }
    #endregion

    #region Public
    public ClosestSegment GetClosest(Vector2 p)
    {
        if (Count <= 0) return new();

        float minDisSquared = float.PositiveInfinity;
        Segment closestSegment = new();
        Vector2 closestSegmentPoint = new();
        for (var i = 0; i < Count; i++)
        {
            var seg = this[i];
            var closestPoint = seg.GetClosestCollisionPoint(p).Point;
            float disSquared = (closestPoint - p).LengthSquared();
            if(disSquared < minDisSquared)
            {
                minDisSquared = disSquared;
                closestSegment = seg;
                closestSegmentPoint = closestPoint;
            }
        }

        return new(closestSegment, closestSegmentPoint, MathF.Sqrt(minDisSquared));
    }
    public CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        float minD = float.PositiveInfinity;
            
        CollisionPoint closest = new();

        for (int i = 0; i < Count; i++)
        {
            CollisionPoint c = this[i].GetClosestCollisionPoint(p);
            float d = (c.Point - p).LengthSquared();
            if (d < minD)
            {
                closest = c;
                minD = d;
            }
        }
        return closest;
    }
    public int GetClosestIndexOnEdge(Vector2 p)
    {
        if (Count <= 0) return -1;
        if (Count == 1) return 0;

        float minD = float.PositiveInfinity;
        int closestIndex = -1;

        for (var i = 0; i < Count; i++)
        {
            var edge = this[i];
            var closest = edge.GetClosestCollisionPoint(p).Point;
            float d = (closest - p).LengthSquared();
            if (d < minD)
            {
                closestIndex = i;
                minD = d;
            }
        }
        return closestIndex;
    }
        
        
    public Points GetUniquePoints()
    {
        var uniqueVertices = new HashSet<Vector2>();
        for (int i = 0; i < Count; i++)
        {
            var seg = this[i];
            uniqueVertices.Add(seg.Start);
            uniqueVertices.Add(seg.End);
        }

        return new(uniqueVertices);
    }
    public Segments GetUniqueSegments()
    {
        var uniqueSegments = new HashSet<Segment>();
        for (int i = 0; i < Count; i++)
        {
            var seg = this[i];
            uniqueSegments.Add(seg);
        }

        return new(uniqueSegments);
    }

    public Segment GetRandomSegment()
    {
        var items = new WeightedItem<Segment>[Count];
        for (var i = 0; i < Count; i++)
        {
            var seg = this[i];
            items[i] = new(seg, (int)seg.LengthSquared);
        }
        return SRNG.PickRandomItem(items);
    }
    public Vector2 GetRandomPoint() => GetRandomSegment().GetRandomPoint();
    public Points GetRandomPoints(int amount)
    {
        var items = new WeightedItem<Segment>[Count];
        for (var i = 0; i < Count; i++)
        {
            var seg = this[i];
            items[i] = new(seg, (int)seg.LengthSquared);
        }
        var pickedSegments = SRNG.PickRandomItems(amount, items);
        var randomPoints = new Points();
        foreach (var seg in pickedSegments)
        {
            randomPoints.Add(seg.GetRandomPoint());
        }
        return randomPoints;
    }
        
    /// <summary>
    /// Counts how often the specified segment appears in the list.
    /// </summary>
    /// <param name="seg"></param>
    /// <returns></returns>
    public int GetCount(Segment seg) { return this.Count((s) => s.Equals(seg)); }

    /// <summary>
    /// Counts how often the specified segment appears in the list disregarding the direction of each segment.
    /// </summary>
    /// <param name="seg"></param>
    /// <returns></returns>
    public int GetCountSimilar(Segment seg) { return this.Count((s) => s.IsSimilar(seg)); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seg"></param>
    /// <returns>Returns true if seg is already in the list.</returns>
    public bool ContainsSegment(Segment seg)
    {
        foreach (var segment in this) { if (segment.Equals(seg)) return true; }
        return false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="seg"></param>
    /// <returns>Returns true if similar segment is already in the list.</returns>
    public bool ContainsSegmentSimilar(Segment seg)
    {
        foreach (var segment in this) { if (segment.IsSimilar(seg)) return true; }
        return false;
    }
    #endregion

    #region Overlap
    public bool OverlapShape(Segments b)
    {
        foreach (var segA in this)
        {
            if (segA.OverlapShape(b)) return true;
        }
        return false;
    }
    public bool OverlapShape(Segment s) { return s.OverlapShape(this); }
    public bool OverlapShape(Circle c) { return c.OverlapShape(this); }
    public bool OverlapShape(Triangle t) { return t.OverlapShape(this); }
    public bool OverlapShape(Rect r) { return r.OverlapShape(this); }
    public bool OverlapShape(Polyline pl) { return pl.OverlapShape(this); }

    #endregion

    #region Intersection
    public CollisionPoints IntersectShape(Segment s)
    {
        CollisionPoints points = new();

        foreach (var seg in this)
        {
            var collisionPoints = seg.IntersectShape(s);
            if (collisionPoints.Valid)
            {
                points.AddRange(collisionPoints);
            }
        }
        return points;
    }
    public CollisionPoints IntersectShape(Circle c)
    {
        CollisionPoints points = new();
        foreach (var seg in this)
        {
            var intersectPoints = SGeometry.IntersectSegmentCircle(seg.Start, seg.End, c.Center, c.Radius);
            foreach (var p in intersectPoints)
            {
                var n = SVec.Normalize(p - c.Center);
                points.Add(new(p, n));
            }
        }
        return points;
    }
    public CollisionPoints IntersectShape(Segments b)
    {
        CollisionPoints points = new();
        foreach (var seg in this)
        {
            var collisionPoints = seg.IntersectShape(b);
            if (collisionPoints.Valid)
            {
                points.AddRange(collisionPoints);
            }
        }
        return points;
    }

    #endregion

    /*
        /// <summary>
        /// Only add the segment if it not already contained in the list.
        /// </summary>
        /// <param name="seg"></param>
        public void AddUnique(Segment seg)
        {
            if (!ContainsSegment(seg)) Add(seg);
        }
        /// <summary>
        /// Only add the segments that are not already contained in the list.
        /// </summary>
        /// <param name="edges"></param>
        public void AddUnique(IEnumerable<Segment> edges)
        {
            foreach (var edge in edges)
            {
                AddUnique(edge);
            }
        }
        */
}