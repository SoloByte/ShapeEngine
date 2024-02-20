using System.Numerics;
using ShapeEngine.Core.Collision;
using ShapeEngine.Core.Structs;
using ShapeEngine.Lib;

namespace ShapeEngine.Core.Shapes;

/// <summary>
/// Points should be in CCW order (A -> B -> C -> D)
/// </summary>
public readonly struct Quad : IEquatable<Quad>
{
    #region Members
    public readonly Vector2 A;
    public readonly Vector2 B;
    public readonly Vector2 C;
    public readonly Vector2 D;
    #endregion

    #region Getters
    public Vector2 AB => B - A;
    public Vector2 BC => C - B;
    public Vector2 CD => D - C;
    public Vector2 DA => A - D;

    public Segment SegmentAToB => new Segment(A, B);
    public Segment SegmentBToC => new Segment(B, C);
    public Segment SegmentCToD => new Segment(C, D);
    public Segment SegmentDToA => new Segment(D, A);

    public float Area
    {
        get
        {
            Triangle abc = new(A,B,C);
            Triangle cda= new(C,D,A);
            return abc.GetArea() + cda.GetArea();
        }
    }
    #endregion

    #region Constructor
    internal Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        this.A = a;
        this.B = b;
        this.C = c;
        this.D = d;
    }
    public Quad(Vector2 topLeft, Vector2 bottomRight)
    {
        this.A = topLeft;
        this.C = bottomRight;
        this.B = new(topLeft.X, bottomRight.Y);
        this.D = new(bottomRight.X, topLeft.Y);
    }
    public Quad(Vector2 topLeft, float width, float height)
    {
        this.A = topLeft;
        this.B = topLeft + new Vector2(0f, height);
        this.C = topLeft + new Vector2(width, height);
        this.D = topLeft + new Vector2(width, 0f);
    }
    public Quad(Rect rect)
    {
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;
        this.A = topLeft;
        this.C = bottomRight;
        this.B = new(topLeft.X, bottomRight.Y);
        this.D = new(bottomRight.X, topLeft.Y);
    }
    public Quad(Rect rect, float rotRad, Vector2 pivot)
    {
        var pivotPoint = rect.GetPoint(pivot);
        var topLeft = rect.TopLeft;
        var bottomRight = rect.BottomRight;

        this.A = (topLeft - pivotPoint).Rotate(rotRad);
        this.B = (new Vector2(topLeft.X, bottomRight.Y) - pivotPoint).Rotate(rotRad);
        this.C = (bottomRight - pivotPoint).Rotate(rotRad);
        this.D = (new Vector2(bottomRight.X, topLeft.Y) - pivotPoint).Rotate(rotRad);
    }
    public Quad(Vector2 pos, Vector2 size, float rotRad, Vector2 alignement)
    {
        var offset = size * alignement;
        var topLeft = pos - offset;
        
        var a = topLeft - pos;
        var b = topLeft + new Vector2(0f, size.Y);
        var c = topLeft + size;
        var d = topLeft + new Vector2(size.X, 0f);

        this.A = pos + (a - pos).Rotate(rotRad);
        this.B = pos + (b - pos).Rotate(rotRad);
        this.C = pos + (c - pos).Rotate(rotRad);
        this.D = pos + (d - pos).Rotate(rotRad);
        
    }
    #endregion
    
    #region Public

    //TODO implement
    public Rect GetBoundingBox()
    {
        return new();
    }
    public readonly CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        return new();
    }

    public Segments GetEdges() => new() { SegmentAToB, SegmentBToC, SegmentCToD, SegmentDToA };

    public Polygon ToPolygon() => new() { A, B, C, D };
    public Vector2 GetPoint(Vector2 alignement)
    {

        var ab = A.Lerp(B, alignement.Y); // B - A;
        var cd = C.Lerp(D, alignement.Y);
        return ab.Lerp(cd, alignement.X);
    }
    public Quad RotateRad(float angle, Vector2 pivot)
    {
        var pivotPoint = GetPoint(pivot);
        var a = (A - pivotPoint).Rotate(angle);
        var b = (B - pivotPoint).Rotate(angle);
        var c = (C - pivotPoint).Rotate(angle);
        var d = (D - pivotPoint).Rotate(angle);
        return new(a,b,c,d);
    }

    public Quad RotateDeg(float angle, Vector2 pivot) => RotateRad(angle * ShapeMath.DEGTORAD, pivot);

    public Triangulation Triangulate()
    {
        Triangle abc = new(A,B,C);
        Triangle cda= new(C,D,A);
        return new Triangulation() { abc, cda };
    }

    public Quad MoveBy(Vector2 amount)
    {
        return new
        (
            A + amount,
            B + amount,
            C + amount,
            D + amount
        );
    }
    public Quad MoveTo(Vector2 target, Vector2 alignement)
    {
        var p = GetPoint(alignement);
        var translation = target - p;
        return new
        (
            A + translation,
            B + translation,
            C + translation,
            D + translation
        );
    }

    public Quad ScaleBy(float amount, Vector2 alignement)
    {
        var p = GetPoint(alignement);
        return new
            (
                A + (A - p) * amount,
                B + (B - p) * amount,
                C + (C - p) * amount,
                D + (D - p) * amount
            );

    }
    public Quad ScaleBy(Vector2 amount, Vector2 alignement)
    {
        var p = GetPoint(alignement);
        return new
        (
            A + (A - p) * amount,
            B + (B - p) * amount,
            C + (C - p) * amount,
            D + (D - p) * amount
        );
    }

    // private bool ContainsPointCheck(Vector2 a, Vector2 b, Vector2 pointToCheck)
    // {
    //     if (a.Y < pointToCheck.Y && b.Y >= pointToCheck.Y || b.Y < pointToCheck.Y && a.Y >= pointToCheck.Y)
    //     {
    //         if (a.X + (pointToCheck.Y - a.Y) / (b.Y - a.Y) * (b.X - a.X) < pointToCheck.X)
    //         {
    //             return true;
    //         }
    //     }
    //     return false;
    // }
    
    public bool ContainsPoint(Vector2 p)
    {
        var oddNodes = false;

        if (Polygon.ContainsPointCheck(D, A, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(A, B, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(B, C, p)) oddNodes = !oddNodes;
        if (Polygon.ContainsPointCheck(D, D, p)) oddNodes = !oddNodes;
        
        return oddNodes;
    }
    #endregion
    
    #region Operators
    public static Quad operator +(Quad left, Quad right)
    {
        return new
        (
            left.A + right.A,
            left.B + right.B,
            left.C + right.C,
            left.D + right.D
        );
    }
    public static Quad operator -(Quad left, Quad right)
    {
        return new
        (
            left.A - right.A,
            left.B - right.B,
            left.C - right.C,
            left.D - right.D
        );
    }
    public static Quad operator *(Quad left, Quad right)
    {
        return new
        (
            left.A * right.A,
            left.B * right.B,
            left.C * right.C,
            left.D * right.D
        );
    }
    public static Quad operator /(Quad left, Quad right)
    {
        return new
        (
            left.A / right.A,
            left.B / right.B,
            left.C / right.C,
            left.D / right.D
        );
    }
    public static Quad operator +(Quad left, Vector2 right)
    {
        return new
        (
            left.A + right,
            left.B + right,
            left.C + right,
            left.D + right
        );
    }
    public static Quad operator -(Quad left, Vector2 right)
    {
        return new
        (
            left.A - right,
            left.B - right,
            left.C - right,
            left.D - right
        );
    }
    public static Quad operator *(Quad left, Vector2 right)
    {
        return new
        (
            left.A * right,
            left.B * right,
            left.C * right,
            left.D * right
        );
    }
    public static Quad operator /(Quad left, Vector2 right)
    {
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right,
            left.D / right
        );
    }
    public static Quad operator *(Quad left, float right)
    {
        return new
        (
            left.A * right,
            left.B * right,
            left.C * right,
            left.D * right
        );
    }
    public static Quad operator /(Quad left, float right)
    {
        return new
        (
            left.A / right,
            left.B / right,
            left.C / right,
            left.D / right
        );
    }
    public static bool operator ==(Quad left, Quad right) => left.Equals(right);
    public static bool operator !=(Quad left, Quad right) => !(left == right);
    #endregion

    #region Equality
    public bool Equals(Quad other) => A.Equals(other.A) && B.Equals(other.B) && C.Equals(other.C) && D.Equals(other.D);

    public override bool Equals(object? obj) => obj is Quad other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(A, B, C, D);
    #endregion

    #region Overlap

    public bool OverlapShape(Segments segments)
    {
        var ab = SegmentAToB;
        var bc = SegmentBToC;
        var cd = SegmentCToD;
        var da = SegmentDToA;
        
        foreach (var seg in segments)
        {
            if (ContainsPoint(seg.Start)) return true;
            if (ContainsPoint(seg.End)) return true;

            if (seg.OverlapShape(ab)) return true;
        
            if (seg.OverlapShape(bc)) return true;
        
            if (seg.OverlapShape(cd)) return true;

            if (seg.OverlapShape(da)) return true;
        }
        return false;
    }

    public bool OverlapShape(Segment s) => s.OverlapShape(this);

    public bool OverlapShape(Circle c) => c.OverlapShape(this);
    public bool OverlapShape(Triangle t) => t.OverlapShape(this);
    public bool OverlapShape(Quad q)
    {
        if (ContainsPoint(q.A)) return true;
        if (ContainsPoint(q.B)) return true;
        if (ContainsPoint(q.C)) return true;
        if (ContainsPoint(q.D)) return true;

        var segAb = new Segment(A, B);
        var segBc = new Segment(B, C);
        var segCd = new Segment(C, D);
        var segDa = new Segment(D, A);

        var qSegment = q.SegmentAToB;
        if (segAb.OverlapShape(qSegment)) return true;
        if (segBc.OverlapShape(qSegment)) return true;
        if (segCd.OverlapShape(qSegment)) return true;
        if (segDa.OverlapShape(qSegment)) return true;
        
        qSegment = q.SegmentBToC;
        if (segAb.OverlapShape(qSegment)) return true;
        if (segBc.OverlapShape(qSegment)) return true;
        if (segCd.OverlapShape(qSegment)) return true;
        if (segDa.OverlapShape(qSegment)) return true;
        
        qSegment = q.SegmentCToD;
        if (segAb.OverlapShape(qSegment)) return true;
        if (segBc.OverlapShape(qSegment)) return true;
        if (segCd.OverlapShape(qSegment)) return true;
        if (segDa.OverlapShape(qSegment)) return true;
        
        qSegment = q.SegmentDToA;
        if (segAb.OverlapShape(qSegment)) return true;
        if (segBc.OverlapShape(qSegment)) return true;
        if (segCd.OverlapShape(qSegment)) return true;
        return segDa.OverlapShape(qSegment);
    }
    public bool OverlapShape(Rect r)
    {
        if (r.ContainsPoint(A)) return true;
        if (r.ContainsPoint(B)) return true;
        if (r.ContainsPoint(C)) return true;
        if (r.ContainsPoint(D)) return true;

        var segAb = new Segment(A, B);
        var segBc = new Segment(B, C);
        var segCd = new Segment(C, D);
        var segDa = new Segment(D, A);

        var qSegment = r.LeftSegment;
        if (segAb.OverlapShape(qSegment)) return true;
        if (segBc.OverlapShape(qSegment)) return true;
        if (segCd.OverlapShape(qSegment)) return true;
        if (segDa.OverlapShape(qSegment)) return true;
        
        qSegment = r.BottomSegment;
        if (segAb.OverlapShape(qSegment)) return true;
        if (segBc.OverlapShape(qSegment)) return true;
        if (segCd.OverlapShape(qSegment)) return true;
        if (segDa.OverlapShape(qSegment)) return true;
        
        qSegment = r.RightSegment;
        if (segAb.OverlapShape(qSegment)) return true;
        if (segBc.OverlapShape(qSegment)) return true;
        if (segCd.OverlapShape(qSegment)) return true;
        if (segDa.OverlapShape(qSegment)) return true;
        
        qSegment = r.TopSegment;
        if (segAb.OverlapShape(qSegment)) return true;
        if (segBc.OverlapShape(qSegment)) return true;
        if (segCd.OverlapShape(qSegment)) return true;
        return segDa.OverlapShape(qSegment);
    }
    public bool OverlapShape(Polygon poly) => OverlapShape(poly.GetEdges());
    public bool OverlapShape(Polyline pl) => OverlapShape(pl.GetEdges());


    #endregion

    
    // #region Intersection
    //
    // public readonly CollisionPoints? Intersect(Collider collider)
    // {
    //     if (!collider.Enabled) return null;
    //
    //     switch (collider.GetShapeType())
    //     {
    //         case ShapeType.Circle:
    //             var c = collider.GetCircleShape();
    //             return IntersectShape(c);
    //         case ShapeType.Segment:
    //             var s = collider.GetSegmentShape();
    //             return IntersectShape(s);
    //         case ShapeType.Triangle:
    //             var t = collider.GetTriangleShape();
    //             return IntersectShape(t);
    //         case ShapeType.Rect:
    //             var r = collider.GetRectShape();
    //             return IntersectShape(r);
    //         case ShapeType.Poly:
    //             var p = collider.GetPolygonShape();
    //             return IntersectShape(p);
    //         case ShapeType.PolyLine:
    //             var pl = collider.GetPolylineShape();
    //             return IntersectShape(pl);
    //     }
    //
    //     return null;
    // }
    // public readonly CollisionPoints? IntersectShape(Segment s) { return GetEdges().IntersectShape(s); }
    // public readonly CollisionPoints? IntersectShape(Circle c) { return ToPolygon().IntersectShape(c); }
    // public readonly CollisionPoints? IntersectShape(Triangle b) { return ToPolygon().IntersectShape(b.ToPolygon()); }
    // public readonly CollisionPoints? IntersectShape(Rect r) { return ToPolygon().IntersectShape(r.ToPolygon()); }
    // public readonly CollisionPoints? IntersectShape(Polygon p) { return ToPolygon().IntersectShape(p); }
    // public readonly CollisionPoints? IntersectShape(Polyline pl) { return GetEdges().IntersectShape(pl.GetEdges()); }
    //
    // #endregion
    
}