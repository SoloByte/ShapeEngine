﻿
using System.Numerics;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;


namespace ShapeEngine.Core.Collision;

public abstract class Shape : IShape
{
    public bool Moves = true;
    public bool Rotates = true;
    public bool Scales = true;
    
    public Transform2D Offset { get; set; }
    public Transform2D CurTransform { get; private set; }
    public Transform2D PrevTransform { get; private set; }
    
    
    protected Shape()
    {
        this.Offset = new(new Vector2(0f), 0f, new Size(0f), 1f);
    }
    protected Shape(Vector2 offset)
    {
        this.Offset = new(offset, 0f, new Size(0f), 1f);
    }
    protected Shape(Transform2D offset)
    {
        this.Offset = offset;
    }


    public abstract void InitializeShape(Transform2D parentTransform);
    public abstract void UpdateShape(float dt, Transform2D parentTransform);
    public abstract void DrawShape();
    public abstract void RecalculateShape();
    
    protected abstract void OnInitialized();
    protected abstract void OnUpdate(float dt);
    protected abstract void OnDraw();
    
    
    /// <summary>
    /// Called each frame after the transform was actualized from the parents
    /// </summary>
    /// <param name="transformChanged"></param>
    protected virtual void OnShapeTransformChanged(bool transformChanged) { }
    protected void UpdateTransform(Transform2D parentTransform)
    {
        PrevTransform = CurTransform;
        if ((!Moves && !Rotates && !Scales) || Offset.IsEmpty())
        {
            CurTransform = parentTransform;
        }
        else CurTransform = Transform2D.UpdateTransform(parentTransform, Offset, Moves, Rotates, Scales);

        OnShapeTransformChanged(PrevTransform != CurTransform);
    }

    
    public abstract ShapeType GetShapeType();
    public virtual Circle GetCircleShape() => new();
    public virtual Segment GetSegmentShape() => new();
    public virtual Triangle GetTriangleShape() => new();
    public virtual Quad GetQuadShape() => new();
    public virtual Rect GetRectShape() => new();
    public virtual Polygon GetPolygonShape() => new();
    public virtual Polyline GetPolylineShape() => new();

    
    /*/// <summary>
    /// Can be used to trigger recalculating the shape.
    /// </summary>
    public virtual void RecalculateShape()
    {

    }
    public virtual void InitializeShape(Transform2D parentTransform)
    {
        //TODO check if it was initialized already
        UpdateTransform(parentTransform);
        OnTransformSetupFinished();
    }
    public virtual void UpdateShape(float dt, Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        Update(dt);
    }

    public virtual void DrawShape()
    {
        Draw();
    }

    protected virtual void Update(float dt) { }
    protected virtual void Draw() { }
    protected virtual void OnTransformSetupFinished() { }
    */

}

public abstract class ShapeContainer : Shape
{
    #region Members

    public ShapeContainer? Parent { get; set; } = null;
    private List<ShapeContainer> children { get; set; } = new();
    
    #endregion
    
    #region Public Functions
    
    public bool ChangeParent(ShapeContainer newParent)
    {
        if (newParent == Parent) return false;
        newParent.AddChild(this);
        return true;
    }
    public bool AddChild(ShapeContainer child)
    {
        if (child.Parent == this) return false;

        if (child.Parent != null) //if child already has a parent
        {
            //remove child from that parent
            child.Parent.RemoveChild(child);
        }
        
        //set new parent
        child.Parent = this;
        
        children.Add(child);
        child.InitializeShape(CurTransform);
        child.OnAddedToParent(this);
        return true;
    }
    public bool RemoveChild(ShapeContainer child)
    {
        if(child.Parent != this) return false;
        
        child.Parent = null;
        var removed = children.Remove(child);
        if(removed) child.OnRemovedFromParent(this);
        return removed;
    }
    
    public override void InitializeShape(Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        foreach (var child in children)
        {
            child.InitializeShape(parentTransform);
            OnChildInitialized(child);
        }
        OnInitialized();
    }
    public override void UpdateShape(float dt, Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        OnUpdate(dt);
        var trans = CurTransform;
        foreach (var child in children)
        {
            child.UpdateShape(dt, trans);
            OnChildUpdated(child);
        }
        OnUpdateFinished();
    }
    public override void DrawShape()
    {
        OnDraw();
        foreach (var child in children)
        {
            child.DrawShape();
            OnChildDrawn(child);
        }
        OnDrawFinished();
    }
    
    #endregion

    #region Protected Functions

    protected abstract void OnChildInitialized(ShapeContainer child);
    protected abstract void OnChildUpdated(ShapeContainer child);
    protected abstract void OnChildDrawn(ShapeContainer child);
    
    protected abstract void OnUpdateFinished();
    protected abstract void OnDrawFinished();
    
    protected abstract void OnAddedToParent(ShapeContainer parent);
    protected abstract void OnRemovedFromParent(ShapeContainer parent);
    
    #endregion
}

public class CircleShape : ShapeContainer
{
    
    public override ShapeType GetShapeType() => ShapeType.Circle;
    public override Circle GetCircleShape() => new(CurTransform.Position, CurTransform.ScaledSize.Radius);
    
    
    //make virtual instead of abstract in shape container!!!
    public override void RecalculateShape() { }
    protected override void OnInitialized() { }
    protected override void OnUpdate(float dt) { }
    protected override void OnDraw() { }
    
    protected override void OnChildInitialized(ShapeContainer child) { }
    protected override void OnChildUpdated(ShapeContainer child) { }
    protected override void OnChildDrawn(ShapeContainer child) { }
    
    protected override void OnUpdateFinished() { }
    protected override void OnDrawFinished() { }

    protected override void OnAddedToParent(ShapeContainer parent) { }
    protected override void OnRemovedFromParent(ShapeContainer parent) { }
    
}

/*
public class SegmentShape : ShapeContainer
{
    
}
public class TriangleShape : ShapeContainer
{
    
}
public class RectShape : ShapeContainer
{
    
}
public class QuadShape : ShapeContainer
{
    
}
public class PolyShape : ShapeContainer
{
    
}
public class PolylineShape : ShapeContainer
{
    
}
*/


public abstract class Collider : Shape
{
    
    /// <summary>
    /// This functions is always called when the collider had at least one collision with another collider this frame
    /// </summary>
    public event Action<Collider, CollisionInformation>? OnCollision;
    
    /// <summary>
    /// This function will always be called when a previous collision with another collider has ended this frame
    /// </summary>
    public event Action<Collider, Collider>? OnCollisionEnded;
    
    /// <summary>
    /// This event will only be invoked when AdvancedCollisionNotifications is enabled and
    /// ComputeIntersection is disabled
    /// If AdvancedCollisionNotification is enabled and ComputeIntersection is enabled OnColliderIntersected will be invoked instead
    /// </summary>
    public event Action<Collider, Collider, bool>? OnColliderOverlapped;
    
    /// <summary>
    /// This event will only be invoked when AdvancedCollisionNotifications is enabled and
    /// ComputeIntersection is enabled
    /// If AdvancedCollisionNotification is enabled and ComputeIntersection is disabled OnColliderOverlapped will be invoked instead
    /// </summary>
    public event Action<Collision>? OnColliderIntersected;
    
    
    private CollisionObject? parent = null;
    public CollisionObject? Parent
    {
        get => parent;
        internal set
        {
            if (parent == null)
            {
                if (value != null)
                {
                    parent = value;
                    OnAddedToCollisionBody(parent);
                }
            }
            else
            {
                if (value == null)
                {
                    OnRemovedFromCollisionBody(parent);
                    parent = null;
                }
                else if (value != parent)
                {
                    OnRemovedFromCollisionBody(parent);
                    parent = value;
                    OnAddedToCollisionBody(parent);
                }
            }
        }
    }

    private bool enabled = true;
    public bool Enabled
    {
        get
        {
            if (parent != null) return parent.Enabled && enabled;
            return enabled;
        }
        set => enabled = value;
    }

    public Vector2 Velocity => parent?.Velocity ?? new(0f);
    // public Vector2 PrevPosition { get; private set; }
    
    // public bool FlippedNormals { get; set; } = false;

    public BitFlag CollisionMask { get; set; } = BitFlag.Empty;
    public uint CollisionLayer { get; set; } = 0;
    public bool ComputeCollision { get; set; } = true;
    /// <summary>
    /// If false only overlaps will be reported but no further details on the intersection.
    /// </summary>
    public bool ComputeIntersections { get; set; } = false;
    /// <summary>
    /// This determines if ColliderIntersected and ColliderOverlapped will be called and if
    /// OnColliderOverlapped and OnColliderIntersected events will be invoked.
    /// Collision and CollisionEnded will always be called.
    /// This is an additional convenience option to get overlap/intersection information in a different way.
    /// </summary>
    public bool AdvancedCollisionNotifications { get; set; } = false;

    // protected bool Dirty = false;
    
    protected Collider()
    {
        this.Offset = new(new Vector2(0f), 0f, new Size(0f), 1f);
    }
    protected Collider(Vector2 offset)
    {
        this.Offset = new(offset, 0f, new Size(0f), 1f);
    }

    protected Collider(Transform2D offset)
    {
        this.Offset = offset;
    }


    public override void InitializeShape(Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        OnInitialized();
    }

    public override void UpdateShape(float dt, Transform2D parentTransform)
    {
        UpdateTransform(parentTransform);
        OnUpdate(dt);
    }

    public override void DrawShape()
    {
        OnDraw();
    }
    
    public override void RecalculateShape() { }

    protected override void OnInitialized() { }

    protected override void OnUpdate(float dt) { }

    protected override void OnDraw() { }
    
    internal void ResolveCollision(CollisionInformation info)
    {
        if (AdvancedCollisionNotifications)
        {
            foreach (var collision in info.Collisions)
            {
                if (ComputeIntersections)
                {
                    ColliderIntersected(collision);
                    OnColliderIntersected?.Invoke(collision);
                }
                else
                {
                    ColliderOverlapped(collision.Other, collision.FirstContact);
                    OnColliderOverlapped?.Invoke(this, collision.Other, collision.FirstContact);
                }
            
            }
        }
        
        Collision(info);
        OnCollision?.Invoke(this, info);
    }
    internal void ResolveCollisionEnded(Collider other)
    {
        CollisionEnded(other);
        OnCollisionEnded?.Invoke(this, other);
    }
    
    
    /// <summary>
    /// This function will only be called when AdvancedCollisionNotifications is enabled and
    /// ComputeIntersection is disabled
    /// If AdvancedCollisionNotification is enabled and ComputeIntersection is enabled ColliderIntersected will be called instead
    /// </summary>
    protected virtual void ColliderOverlapped(Collider other, bool firstContact) { }
    
    /// <summary>
    /// This function will only be called when AdvancedCollisionNotifications is enabled and
    /// ComputeIntersection is enabled
    /// If AdvancedCollisionNotification is enabled and ComputeIntersection is disabled ColliderOverlapped will be called instead
    /// </summary>
    protected virtual void ColliderIntersected(Collision collision) { }
    
    /// <summary>
    /// This functions is always called when this collider had at least one collision with another collider this frame
    /// </summary>
    protected virtual void Collision(CollisionInformation info) { }
    
    /// <summary>
    /// This function will always be called when a previous collision with another collider has ended this frame
    /// </summary>
    protected virtual void CollisionEnded(Collider other) { }
    
    protected virtual void OnAddedToCollisionBody(CollisionObject newParent) { }
    protected virtual void OnRemovedFromCollisionBody(CollisionObject formerParent) { }

    public abstract Rect GetBoundingBox();

    public Polygon? Project(Vector2 v)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.ProjectShape(v, 8);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.ProjectShape(v);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.ProjectShape(v);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.ProjectShape(v);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.ProjectShape(v);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.ProjectShape(v);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.ProjectShape(v);
        }

        return null;
    }

    #region Closest

    public CollisionPoint GetClosestCollisionPoint(Vector2 p)
    {
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestCollisionPoint(p);
            case ShapeType.Segment:return GetSegmentShape().GetClosestCollisionPoint(p);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestCollisionPoint(p);
            case ShapeType.Quad:return GetQuadShape().GetClosestCollisionPoint(p);
            case ShapeType.Rect:return GetRectShape().GetClosestCollisionPoint(p);
            case ShapeType.Poly:return GetPolygonShape().GetClosestCollisionPoint(p);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestCollisionPoint(p);
        }

        return new();
    }

    public ClosestDistance GetClosestDistanceTo(IShape shape)
    {
        switch (shape.GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetClosestDistanceTo(shape.GetCircleShape());
            case ShapeType.Segment: return GetClosestDistanceTo(shape.GetSegmentShape());
            case ShapeType.Triangle: return GetClosestDistanceTo(shape.GetTriangleShape());
            case ShapeType.Quad: return GetClosestDistanceTo(shape.GetQuadShape());
            case ShapeType.Rect: return GetClosestDistanceTo(shape.GetRectShape());
            case ShapeType.Poly: return GetClosestDistanceTo(shape.GetPolygonShape());
            case ShapeType.PolyLine: return GetClosestDistanceTo(shape.GetPolylineShape());
        }

        return new();
    }
    
    public ClosestDistance GetClosestDistanceTo(Vector2 p)
    {
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestDistanceTo(p);
            case ShapeType.Segment:return GetSegmentShape().GetClosestDistanceTo(p);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestDistanceTo(p);
            case ShapeType.Quad:return GetQuadShape().GetClosestDistanceTo(p);
            case ShapeType.Rect:return GetRectShape().GetClosestDistanceTo(p);
            case ShapeType.Poly:return GetPolygonShape().GetClosestDistanceTo(p);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestDistanceTo(p);
        }

        return new();
    }
    public ClosestDistance GetClosestDistanceTo(Segment shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestDistanceTo(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestDistanceTo(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestDistanceTo(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestDistanceTo(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestDistanceTo(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestDistanceTo(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestDistanceTo(shape);
        }

        return new();
    }
    public ClosestDistance GetClosestDistanceTo(Circle shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestDistanceTo(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestDistanceTo(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestDistanceTo(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestDistanceTo(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestDistanceTo(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestDistanceTo(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestDistanceTo(shape);
        }

        return new();
    }
    public ClosestDistance GetClosestDistanceTo(Triangle shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestDistanceTo(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestDistanceTo(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestDistanceTo(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestDistanceTo(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestDistanceTo(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestDistanceTo(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestDistanceTo(shape);
        }

        return new();
    }
    public ClosestDistance GetClosestDistanceTo(Quad shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestDistanceTo(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestDistanceTo(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestDistanceTo(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestDistanceTo(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestDistanceTo(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestDistanceTo(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestDistanceTo(shape);
        }

        return new();
    }
    public ClosestDistance GetClosestDistanceTo(Rect shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestDistanceTo(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestDistanceTo(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestDistanceTo(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestDistanceTo(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestDistanceTo(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestDistanceTo(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestDistanceTo(shape);
        }

        return new();
    }
    public ClosestDistance GetClosestDistanceTo(Polygon shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestDistanceTo(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestDistanceTo(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestDistanceTo(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestDistanceTo(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestDistanceTo(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestDistanceTo(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestDistanceTo(shape);
        }

        return new();
    }
    public ClosestDistance GetClosestDistanceTo(Polyline shape)
    {
        
        switch (GetShapeType())
        {
            case ShapeType.None: return new();
            case ShapeType.Circle: return GetCircleShape().GetClosestDistanceTo(shape);
            case ShapeType.Segment:return GetSegmentShape().GetClosestDistanceTo(shape);
            case ShapeType.Triangle:return GetTriangleShape().GetClosestDistanceTo(shape);
            case ShapeType.Quad:return GetQuadShape().GetClosestDistanceTo(shape);
            case ShapeType.Rect:return GetRectShape().GetClosestDistanceTo(shape);
            case ShapeType.Poly:return GetPolygonShape().GetClosestDistanceTo(shape);
            case ShapeType.PolyLine:return GetPolylineShape().GetClosestDistanceTo(shape);
        }

        return new();
    }
    #endregion

    #region Contains

    public bool ContainsPoint(Vector2 p)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsPoint(p);
            case ShapeType.Segment: return GetSegmentShape().ContainsPoint(p);
            case ShapeType.Triangle: return GetTriangleShape().ContainsPoint(p);
            case ShapeType.Quad: return GetQuadShape().ContainsPoint(p);
            case ShapeType.Rect: return GetRectShape().ContainsPoint(p);
            case ShapeType.Poly: return GetPolygonShape().ContainsPoint(p);
            case ShapeType.PolyLine: return GetPolylineShape().ContainsPoint(p);
        }

        return false;
    }

    public bool ContainsShape(IShape shape)
    {
        switch (shape.GetShapeType())
        {
            case ShapeType.Circle: return ContainsShape(shape.GetCircleShape());
            case ShapeType.Segment: return ContainsShape(shape.GetSegmentShape());
            case ShapeType.Triangle: return ContainsShape(shape.GetTriangleShape());
            case ShapeType.Quad: return ContainsShape(shape.GetQuadShape());
            case ShapeType.Rect: return ContainsShape(shape.GetRectShape());
            case ShapeType.Poly: return ContainsShape(shape.GetPolygonShape());
            case ShapeType.PolyLine: return ContainsShape(shape.GetPolylineShape());
        }

        return false;
    }
    public bool ContainsShape(Segment shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Circle shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Triangle shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }

    public bool ContainsShape(Quad shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Rect shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Polygon shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    public bool ContainsShape(Polyline shape)
    {
        switch (GetShapeType())
        {
            case ShapeType.Circle: return GetCircleShape().ContainsShape(shape);
            case ShapeType.Segment: return false;
            case ShapeType.Triangle: return GetTriangleShape().ContainsShape(shape);
            case ShapeType.Quad: return GetQuadShape().ContainsShape(shape);
            case ShapeType.Rect: return GetRectShape().ContainsShape(shape);
            case ShapeType.Poly: return GetPolygonShape().ContainsShape(shape);
            case ShapeType.PolyLine: return false;
        }

        return false;
    }
    #endregion
    
    #region Overlap
    public bool Overlap(CollisionObject other)
    {
        if (!Enabled || !other.Enabled || !other.HasColliders) return false;
        foreach (var otherCol in other.Colliders)
        {
            // if(!otherCol.Enabled) continue;
            if (Overlap(otherCol)) return true;
        }

        return false;
    }
    public bool Overlap(Collider other)
    {
        if (!Enabled || !other.Enabled) return false;

        switch (other.GetShapeType())
        {
            case ShapeType.Circle: return Overlap(other.GetCircleShape());
            case ShapeType.Segment: return Overlap(other.GetSegmentShape());
            case ShapeType.Triangle: return Overlap(other.GetTriangleShape());
            case ShapeType.Quad: return Overlap(other.GetQuadShape());
            case ShapeType.Rect: return Overlap(other.GetRectShape());
            case ShapeType.Poly: return Overlap(other.GetPolygonShape());
            case ShapeType.PolyLine: return Overlap(other.GetPolylineShape());
        }

        return false;
    }
    public bool Overlap(IShape other)
    {
        if (!Enabled) return false;

        switch (other.GetShapeType())
        {
            case ShapeType.Circle: return Overlap(other.GetCircleShape());
            case ShapeType.Segment: return Overlap(other.GetSegmentShape());
            case ShapeType.Triangle: return Overlap(other.GetTriangleShape());
            case ShapeType.Quad: return Overlap(other.GetQuadShape());
            case ShapeType.Rect: return Overlap(other.GetRectShape());
            case ShapeType.Poly: return Overlap(other.GetPolygonShape());
            case ShapeType.PolyLine: return Overlap(other.GetPolylineShape());
        }

        return false;
    }

    public bool Overlap(Segment segment)
    { 
        if (!Enabled) return false;
        
        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.OverlapShape(segment);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.OverlapShape(segment);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.OverlapShape(segment);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.OverlapShape(segment);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.OverlapShape(segment);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.OverlapShape(segment);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.OverlapShape(segment);
        }

        return false;
    }
    public bool Overlap(Triangle triangle)
    {
        if (!Enabled) return false;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.OverlapShape(triangle);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.OverlapShape(triangle);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.OverlapShape(triangle);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.OverlapShape(triangle);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.OverlapShape(triangle);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.OverlapShape(triangle);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.OverlapShape(triangle);
        }

        return false;
    }
    public bool Overlap(Circle circle)
    {
        if (!Enabled) return false;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.OverlapShape(circle);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.OverlapShape(circle);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.OverlapShape(circle);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.OverlapShape(circle);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.OverlapShape(circle);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.OverlapShape(circle);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.OverlapShape(circle);
        }

        return false;
    }
    public bool Overlap(Rect rect)
    {
        if (!Enabled) return false;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.OverlapShape(rect);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.OverlapShape(rect);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.OverlapShape(rect);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.OverlapShape(rect);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.OverlapShape(rect);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.OverlapShape(rect);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.OverlapShape(rect);
        }

        return false;
    }
    public bool Overlap(Quad quad)
    {
        if (!Enabled) return false;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.OverlapShape(quad);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.OverlapShape(quad);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.OverlapShape(quad);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.OverlapShape(quad);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.OverlapShape(quad);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.OverlapShape(quad);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.OverlapShape(quad);
        }

        return false;
    }

    public bool Overlap(Polygon poly)
    {
        if (!Enabled) return false;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.OverlapShape(poly);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.OverlapShape(poly);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.OverlapShape(poly);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.OverlapShape(poly);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.OverlapShape(poly);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.OverlapShape(poly);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.OverlapShape(poly);
        }

        return false;
    }
    public bool Overlap(Polyline polyLine)
    {
        if (!Enabled) return false;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.OverlapShape(polyLine);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.OverlapShape(polyLine);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.OverlapShape(polyLine);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.OverlapShape(polyLine);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.OverlapShape(polyLine);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.OverlapShape(polyLine);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.OverlapShape(polyLine);
        }

        return false;
    }
    #endregion

    #region Intersect
    public CollisionPoints? Intersect(CollisionObject other)
    {
        if (!Enabled || !other.Enabled || !other.HasColliders) return null;
        CollisionPoints? result = null;
        foreach (var otherCol in other.Colliders)
        {
            var points = Intersect(otherCol);
            if (points == null) continue;
            
            result ??= new();
            result.AddRange(points);
        }
        return result;
    }
    public CollisionPoints? Intersect(Collider other)
    {
        if (!Enabled || !other.Enabled) return null;

        switch (other.GetShapeType())
        {
            case ShapeType.Circle: return Intersect(other.GetCircleShape());
            case ShapeType.Segment: return Intersect(other.GetSegmentShape());
            case ShapeType.Triangle: return Intersect(other.GetTriangleShape());
            case ShapeType.Rect: return Intersect(other.GetRectShape());
            case ShapeType.Quad: return Intersect(other.GetQuadShape());
            case ShapeType.Poly: return Intersect(other.GetPolygonShape());
            case ShapeType.PolyLine: return Intersect(other.GetPolylineShape());
        }

        return null;
    }
    public CollisionPoints? Intersect(IShape other)
    {
        if (!Enabled) return null;

        switch (other.GetShapeType())
        {
            case ShapeType.Circle: return Intersect(other.GetCircleShape());
            case ShapeType.Segment: return Intersect(other.GetSegmentShape());
            case ShapeType.Triangle: return Intersect(other.GetTriangleShape());
            case ShapeType.Rect: return Intersect(other.GetRectShape());
            case ShapeType.Quad: return Intersect(other.GetQuadShape());
            case ShapeType.Poly: return Intersect(other.GetPolygonShape());
            case ShapeType.PolyLine: return Intersect(other.GetPolylineShape());
        }

        return null;
    }

    public CollisionPoints? Intersect(Segment segment)
    {
        if (!Enabled) return null;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(segment);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(segment);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(segment);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(segment);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(segment);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(segment);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(segment);
        }

        return null;
    }
    
    public CollisionPoints? Intersect(Triangle triangle)
    {
        if (!Enabled) return null;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(triangle);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(triangle);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(triangle);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(triangle);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(triangle);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(triangle);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(triangle);
        }

        return null;
    }
    public CollisionPoints? Intersect(Circle circle)
    {
        if (!Enabled) return null;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(circle);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(circle);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(circle);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(circle);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(circle);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(circle);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(circle);
        }

        return null;
    }
    public CollisionPoints? Intersect(Rect rect)
    {
        if (!Enabled) return null;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(rect);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(rect);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(rect);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(rect);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(rect);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(rect);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(rect);
        }

        return null;
    }
    public CollisionPoints? Intersect(Quad quad)
    {
        if (!Enabled) return null;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(quad);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(quad);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(quad);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(quad);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(quad);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(quad);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(quad);
        }

        return null;
    }
    public CollisionPoints? Intersect(Polygon poly)
    {
        if (!Enabled) return null;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(poly);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(poly);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(poly);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(poly);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(poly);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(poly);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(poly);
        }

        return null;
    }
    public CollisionPoints? Intersect(Polyline polyLine)
    {
        if (!Enabled) return null;

        switch (GetShapeType())
        {
            case ShapeType.Circle:
                var c = GetCircleShape();
                return c.IntersectShape(polyLine);
            case ShapeType.Segment:
                var s = GetSegmentShape();
                return s.IntersectShape(polyLine);
            case ShapeType.Triangle:
                var t = GetTriangleShape();
                return t.IntersectShape(polyLine);
            case ShapeType.Rect:
                var r = GetRectShape();
                return r.IntersectShape(polyLine);
            case ShapeType.Quad:
                var q = GetQuadShape();
                return q.IntersectShape(polyLine);
            case ShapeType.Poly:
                var p = GetPolygonShape();
                return p.IntersectShape(polyLine);
            case ShapeType.PolyLine:
                var pl = GetPolylineShape();
                return pl.IntersectShape(polyLine);
        }

        return null;
    }
    #endregion

}
