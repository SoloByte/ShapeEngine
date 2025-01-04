using System.Diagnostics;
using System.Numerics;
using ShapeEngine.Color;
using ShapeEngine.Core.CollisionSystem;
using ShapeEngine.Core.Shapes;
using ShapeEngine.Core.Structs;
using ShapeEngine.Input;
using ShapeEngine.Lib;
using ShapeEngine.Text;
using Color = System.Drawing.Color;
using ShapeEngine.Random;
namespace Examples.Scenes.ExampleScenes;




public class ShapesExample : ExampleScene
{
    private const float LineThickness = 4f;

    private enum ShapeMode
    {
        Overlap = 0,
        Intersection = 1,
        ClosestDistance = 2
    }
    private abstract class Shape
    {
        public abstract Vector2 GetPosition();
        public abstract void Rotate(float angleRad);
        public abstract void Move(Vector2 newPosition);
        public abstract void Draw(ColorRgba color);
        public abstract ShapeType GetShapeType();
        
        public abstract ClosestPointResult GetClosestPointToShape(Shape shape);
        public abstract bool OverlapWith(Shape shape);
        public abstract CollisionPoints? IntersectWith(Shape shape);
        
        public abstract ClosestPointResult GetClosestPointToPolygon(Polygon polygon);
        public abstract bool OverlapWith(Polygon polygon);
        public abstract CollisionPoints? IntersectWith(Polygon polygon);
        
        public abstract Polygon? GetProjectionPoints(Vector2 projectionPosition);
        
        public abstract bool GetSegment(int index, out Segment segment);
        
        
        public string GetName()
        {
            switch (GetShapeType())
            {
                case ShapeType.None: return "Point";
                case ShapeType.Circle: return "Circle";
                case ShapeType.Segment: return "Segment";
                case ShapeType.Ray: return "Ray";
                case ShapeType.Line: return "Line";
                case ShapeType.Triangle: return "Triangle";
                case ShapeType.Quad: return "Quad";
                case ShapeType.Rect: return "Rect";
                case ShapeType.Poly: return "Poly";
                case ShapeType.PolyLine: return "Polyline";
            }

            return "Invalid Shape";
        }
    }

    private class PointShape : Shape
    {
        public Vector2 Position;
        private float size;
        public PointShape(Vector2 pos, float size)
        {
            this.Position = pos;
            this.size = size;
        }
        public override void Move(Vector2 newPosition)
        {
            Position = newPosition;
        }

        public override void Rotate(float angleRad)
        {
            // No rotation for point shape
        }
        public override void Draw(ColorRgba color)
        {
            Position.Draw(size, color, 16);
        }

        public override Vector2 GetPosition() => Position;
        public override ShapeType GetShapeType() => ShapeType.None;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            var result = shape.GetClosestPointToShape(this);
            return result.Switch();

        }
        public override bool OverlapWith(Shape shape)
        {
            return false;
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            return null;
        }
        
        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = polygon.GetClosestPoint(Position, out float disSquared, out int index);
            return new(new(Position, (result.Point - Position).Normalize()), result, disSquared, -1, index);

        }
        public override bool OverlapWith(Polygon polygon)
        {
            return polygon.ContainsPoint(Position);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return null;
        }

        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            return new() { Position, projectionPosition };
        }
        
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = new();
            return false;
        }

    }
    private class SegmentShape : Shape
    {
        public Segment Segment;
        private Vector2 position;
        public SegmentShape(Vector2 pos, float size)
        {
            position = pos;
            var randAngle = Rng.Instance.RandAngleRad();
            var offset = new Vector2(size, 0f).Rotate(randAngle);
            var start = pos - offset;
            var end = pos + offset;
            Segment = new(start, end);
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            Segment = Segment.ChangePosition(offset);
            position = newPosition;
        }
        public override void Rotate(float angleRad)
        {
            Segment = Segment.ChangeRotation(angleRad, 0.5f);
        }
        public override void Draw(ColorRgba color)
        {
            Segment.Draw(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Segment;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            if (shape is PointShape pointShape)
            {
                var point = pointShape.Position;
                var p= Segment.GetClosestPoint(point, out float distance);
                return new ClosestPointResult(
                    p,
                    new CollisionPoint(point, (p.Point - point).Normalize()),
                    distance
                    );
            }
            if (shape is SegmentShape segmentShape) return Segment.GetClosestPoint(segmentShape.Segment);
            if (shape is RayShape rayShape) return Segment.GetClosestPoint(rayShape.Ray);
            if (shape is LineShape lineShape) return Segment.GetClosestPoint(lineShape.Line);
            if (shape is CircleShape circleShape) return Segment.GetClosestPoint(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Segment.GetClosestPoint(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Segment.GetClosestPoint(quadShape.Quad);
            if (shape is RectShape rectShape) return Segment.GetClosestPoint(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Segment.GetClosestPoint(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Segment.GetClosestPoint(polylineShape.Polyline);
            return new();
        }
        public override bool OverlapWith(Shape shape)
        {
            if (shape is PointShape pointShape) return Segment.ContainsPoint(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Segment.OverlapShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Segment.OverlapShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Segment.OverlapShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Segment.OverlapShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Segment.OverlapShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Segment.OverlapShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Segment.OverlapShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Segment.OverlapShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Segment.OverlapShape(polylineShape.Polyline);
            return new();
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            if (shape is PointShape pointShape) return null;
            if (shape is SegmentShape segmentShape) return Segment.IntersectShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Segment.IntersectShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Segment.IntersectShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Segment.IntersectShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Segment.IntersectShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Segment.IntersectShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Segment.IntersectShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Segment.IntersectShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Segment.IntersectShape(polylineShape.Polyline);
            return new();
        }

        public override Vector2 GetPosition() => position;

        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = Segment.GetClosestPoint(polygon);
            return result;

        }
        public override bool OverlapWith(Polygon polygon)
        {
            return Segment.OverlapShape(polygon);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return Segment.IntersectShape(polygon);
        }
        
        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            var v = projectionPosition - position;

            return Segment.ProjectShape(v);
        }
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = Segment;
            return true;
        }
    }
    private class RayShape(Vector2 pos, Vector2 dir) : Shape
    {
        public Ray Ray = new(pos, dir);

        public override void Move(Vector2 newPosition)
        {
            Ray = Ray.SetPoint(newPosition);
        }
        public override void Rotate(float angleRad)
        {
            Ray = Ray.ChangeRotation(angleRad);
        }
        public override void Draw(ColorRgba color)
        {
            Ray.Draw(Ray.MaxLength, LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Ray;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            if (shape is PointShape pointShape)
            {
                var point = pointShape.Position;
                var p= Ray.GetClosestPoint(point, out float distance);
                return new ClosestPointResult(
                    p,
                    new CollisionPoint(point, (p.Point - point).Normalize()),
                    distance
                    );
            }
            if (shape is SegmentShape segmentShape) return Ray.GetClosestPoint(segmentShape.Segment);
            if (shape is RayShape rayShape) return Ray.GetClosestPoint(rayShape.Ray);
            if (shape is LineShape lineShape) return Ray.GetClosestPoint(lineShape.Line);
            if (shape is CircleShape circleShape) return Ray.GetClosestPoint(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Ray.GetClosestPoint(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Ray.GetClosestPoint(quadShape.Quad);
            if (shape is RectShape rectShape) return Ray.GetClosestPoint(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Ray.GetClosestPoint(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Ray.GetClosestPoint(polylineShape.Polyline);
            return new();
        }
        public override bool OverlapWith(Shape shape)
        {
            if (shape is PointShape pointShape) return Ray.IsPointOnRay(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Ray.OverlapShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Ray.OverlapShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Ray.OverlapShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Ray.OverlapShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Ray.OverlapShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Ray.OverlapShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Ray.OverlapShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Ray.OverlapShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Ray.OverlapShape(polylineShape.Polyline);
            return new();
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            if (shape is PointShape pointShape) return null;
            if (shape is SegmentShape segmentShape) return Ray.IntersectShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Ray.IntersectShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Ray.IntersectShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Ray.IntersectShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Ray.IntersectShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Ray.IntersectShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Ray.IntersectShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Ray.IntersectShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Ray.IntersectShape(polylineShape.Polyline);
            return new();
        }
        
        public override Vector2 GetPosition() => Ray.Point;

        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = Ray.GetClosestPoint(polygon);
            return result;

        }
        public override bool OverlapWith(Polygon polygon)
        {
            return Ray.OverlapShape(polygon);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return Ray.IntersectShape(polygon);
        }
        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            return null;
        }
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = Ray.ToSegment(Ray.MaxLength);
            return true;
        }
    }
    private class LineShape(Vector2 pos, Vector2 dir) : Shape
    {
        public Line Line = new(pos, dir);

        public override void Move(Vector2 newPosition)
        {
            Line = Line.SetPoint(newPosition);
        }
        public override void Rotate(float angleRad)
        {
            Line = Line.ChangeRotation(angleRad);
        }
        public override void Draw(ColorRgba color)
        {
            Line.Draw(Line.MaxLength, LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Line;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            if (shape is PointShape pointShape)
            {
                var point = pointShape.Position;
                var p= Line.GetClosestPoint(point, out float distance);
                return new ClosestPointResult(
                    p,
                    new CollisionPoint(point, (p.Point - point).Normalize()),
                    distance
                    );
            }
            if (shape is SegmentShape segmentShape) return Line.GetClosestPoint(segmentShape.Segment);
            if (shape is RayShape rayShape) return Line.GetClosestPoint(rayShape.Ray);
            if (shape is LineShape lineShape) return Line.GetClosestPoint(lineShape.Line);
            if (shape is CircleShape circleShape) return Line.GetClosestPoint(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Line.GetClosestPoint(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Line.GetClosestPoint(quadShape.Quad);
            if (shape is RectShape rectShape) return Line.GetClosestPoint(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Line.GetClosestPoint(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Line.GetClosestPoint(polylineShape.Polyline);
            return new();
        }
        public override bool OverlapWith(Shape shape)
        {
            if (shape is PointShape pointShape) return Line.IsPointOnLine(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Line.OverlapShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Line.OverlapShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Line.OverlapShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Line.OverlapShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Line.OverlapShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Line.OverlapShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Line.OverlapShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Line.OverlapShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Line.OverlapShape(polylineShape.Polyline);
            return new();
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            if (shape is PointShape pointShape) return null;
            if (shape is SegmentShape segmentShape) return Line.IntersectShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Line.IntersectShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Line.IntersectShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Line.IntersectShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Line.IntersectShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Line.IntersectShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Line.IntersectShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Line.IntersectShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Line.IntersectShape(polylineShape.Polyline);
            return new();
        }
        
        public override Vector2 GetPosition() => Line.Point;

        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = Line.GetClosestPoint(polygon);
            return result;

        }
        public override bool OverlapWith(Polygon polygon)
        {
            return Line.OverlapShape(polygon);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return Line.IntersectShape(polygon);
        }
        
        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            return null;
        }
        
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = Line.ToSegment(Line.MaxLength);
            return true;
        }
    }
    private class CircleShape : Shape
    {
        public Circle Circle;
        public CircleShape(Vector2 pos, float size)
        {
            Circle = new(pos, size);
        }
        public override void Move(Vector2 newPosition)
        {
            Circle = new(newPosition, Circle.Radius);
        }
        public override void Rotate(float angleRad)
        {
           //no rotation for circle
        }
        public override void Draw(ColorRgba color)
        {
            Circle.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Circle;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            if (shape is PointShape pointShape)
            {
                var point = pointShape.Position;
                var p= Circle.GetClosestPoint(point, out float distance);
                return new ClosestPointResult(
                    p,
                    new CollisionPoint(point, (p.Point - point).Normalize()),
                    distance
                );
            }
            if (shape is SegmentShape segmentShape) return Circle.GetClosestPoint(segmentShape.Segment);
            if (shape is RayShape rayShape) return Circle.GetClosestPoint(rayShape.Ray);
            if (shape is LineShape lineShape) return Circle.GetClosestPoint(lineShape.Line);
            if (shape is CircleShape circleShape) return Circle.GetClosestPoint(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Circle.GetClosestPoint(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Circle.GetClosestPoint(quadShape.Quad);
            if (shape is RectShape rectShape) return Circle.GetClosestPoint(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Circle.GetClosestPoint(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Circle.GetClosestPoint(polylineShape.Polyline);
            return new();
        }
        public override bool OverlapWith(Shape shape)
        {
            if (shape is PointShape pointShape) return Circle.ContainsPoint(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Circle.OverlapShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Circle.OverlapShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Circle.OverlapShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Circle.OverlapShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Circle.OverlapShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Circle.OverlapShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Circle.OverlapShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Circle.OverlapShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Circle.OverlapShape(polylineShape.Polyline);
            return new();
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            if (shape is PointShape pointShape) return null;
            if (shape is SegmentShape segmentShape) return Circle.IntersectShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Circle.IntersectShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Circle.IntersectShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Circle.IntersectShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Circle.IntersectShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Circle.IntersectShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Circle.IntersectShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Circle.IntersectShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Circle.IntersectShape(polylineShape.Polyline);
            return new();
        }
        
        public override Vector2 GetPosition() => Circle.Center;

        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = Circle.GetClosestPoint(polygon);
            return result;

        }
        public override bool OverlapWith(Polygon polygon)
        {
            return Circle.OverlapShape(polygon);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return Circle.IntersectShape(polygon);
        }
        
        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            var v = projectionPosition - Circle.Center;

            return Circle.ProjectShape(v);
        }
        
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = new();
            return false;
        }
    }
    private class TriangleShape : Shape
    {
        private Vector2 position;
        public Triangle Triangle;

        public TriangleShape(Vector2 pos, float size)
        {
            position = pos;
            var randAngle = Rng.Instance.RandAngleRad();
            var a = pos + new Vector2(size * Rng.Instance.RandF(0.75f, 1.5f), size * Rng.Instance.RandF(-0.5f, 0.5f)).Rotate(randAngle);
            var b = pos + new Vector2(-size * Rng.Instance.RandF(0.75f, 1.5f), -size * Rng.Instance.RandF(0.5f, 1f)).Rotate(randAngle);
            var c = pos + new Vector2(-size * Rng.Instance.RandF(0.75f, 1.5f), size * Rng.Instance.RandF(0.5f, 1f)).Rotate(randAngle);
            Triangle = new(a, b, c);
        }

        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            Triangle = Triangle.ChangePosition(offset);
            position = newPosition;
        }
        public override void Rotate(float angleRad)
        {
            Triangle = Triangle.ChangeRotation(angleRad);
        }
        public override void Draw(ColorRgba color)
        {
            Triangle.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Triangle;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            if (shape is PointShape pointShape)
            {
                var point = pointShape.Position;
                var p= Triangle.GetClosestPoint(point, out float distance);
                return new ClosestPointResult(
                    p,
                    new CollisionPoint(point, (p.Point - point).Normalize()),
                    distance
                );
            }
            if (shape is SegmentShape segmentShape) return Triangle.GetClosestPoint(segmentShape.Segment);
            if (shape is RayShape rayShape) return Triangle.GetClosestPoint(rayShape.Ray);
            if (shape is LineShape lineShape) return Triangle.GetClosestPoint(lineShape.Line);
            if (shape is CircleShape circleShape) return Triangle.GetClosestPoint(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Triangle.GetClosestPoint(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Triangle.GetClosestPoint(quadShape.Quad);
            if (shape is RectShape rectShape) return Triangle.GetClosestPoint(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Triangle.GetClosestPoint(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Triangle.GetClosestPoint(polylineShape.Polyline);
            return new();
        }
        public override bool OverlapWith(Shape shape)
        {
            if (shape is PointShape pointShape) return Triangle.ContainsPoint(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Triangle.OverlapShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Triangle.OverlapShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Triangle.OverlapShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Triangle.OverlapShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Triangle.OverlapShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Triangle.OverlapShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Triangle.OverlapShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Triangle.OverlapShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Triangle.OverlapShape(polylineShape.Polyline);
            return new();
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            if (shape is PointShape pointShape) return null;
            if (shape is SegmentShape segmentShape) return Triangle.IntersectShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Triangle.IntersectShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Triangle.IntersectShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Triangle.IntersectShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Triangle.IntersectShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Triangle.IntersectShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Triangle.IntersectShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Triangle.IntersectShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Triangle.IntersectShape(polylineShape.Polyline);
            return new();
        }
        
        
        public override Vector2 GetPosition() => position;

        
        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = Triangle.GetClosestPoint(polygon);
            return result;
        }
        public override bool OverlapWith(Polygon polygon)
        {
            return Triangle.OverlapShape(polygon);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return Triangle.IntersectShape(polygon);
        }
        
        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            var v = projectionPosition - position;

            return Triangle.ProjectShape(v);
        }
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = Triangle.GetSegment(index);
            return true;
        }
    }
    private class QuadShape : Shape
    {
        public Quad Quad;
        public QuadShape(Vector2 pos, float size)
        {
            var randAngle = Rng.Instance.RandAngleRad();
            Quad = new(pos, new Size(size * 2), randAngle, new AnchorPoint(0.5f));
        }
        public override void Move(Vector2 newPosition)
        {
            Quad = Quad.SetPosition(newPosition, new AnchorPoint(0.5f));
        }
        public override void Rotate(float angleRad)
        {
            Quad = Quad.ChangeRotation(angleRad);
        }
        public override void Draw(ColorRgba color)
        {
           Quad.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Quad;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            if (shape is PointShape pointShape)
            {
                var point = pointShape.Position;
                var p= Quad.GetClosestPoint(point, out float distance);
                return new ClosestPointResult(
                    p,
                    new CollisionPoint(point, (p.Point - point).Normalize()),
                    distance
                );
            }
            if (shape is SegmentShape segmentShape) return Quad.GetClosestPoint(segmentShape.Segment);
            if (shape is RayShape rayShape) return Quad.GetClosestPoint(rayShape.Ray);
            if (shape is LineShape lineShape) return Quad.GetClosestPoint(lineShape.Line);
            if (shape is CircleShape circleShape) return Quad.GetClosestPoint(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Quad.GetClosestPoint(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Quad.GetClosestPoint(quadShape.Quad);
            if (shape is RectShape rectShape) return Quad.GetClosestPoint(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Quad.GetClosestPoint(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Quad.GetClosestPoint(polylineShape.Polyline);
            return new();
        }
        
        public override bool OverlapWith(Shape shape)
        {
            if (shape is PointShape pointShape) return Quad.ContainsPoint(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Quad.OverlapShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Quad.OverlapShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Quad.OverlapShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Quad.OverlapShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Quad.OverlapShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Quad.OverlapShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Quad.OverlapShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Quad.OverlapShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Quad.OverlapShape(polylineShape.Polyline);
            return new();
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            if (shape is PointShape pointShape) return null;
            if (shape is SegmentShape segmentShape) return Quad.IntersectShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Quad.IntersectShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Quad.IntersectShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Quad.IntersectShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Quad.IntersectShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Quad.IntersectShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Quad.IntersectShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Quad.IntersectShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Quad.IntersectShape(polylineShape.Polyline);
            return new();
        }
        
        public override Vector2 GetPosition() => Quad.Center;

        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = Quad.GetClosestPoint(polygon);
            return result;

        }
        public override bool OverlapWith(Polygon polygon)
        {
            return Quad.OverlapShape(polygon);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return Quad.IntersectShape(polygon);
        }
        
        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            var v = projectionPosition - Quad.Center;

            return Quad.ProjectShape(v);
        }
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = Quad.GetSegment(index);
            return true;
        }
    }
    private class RectShape : Shape
    {
        public Rect Rect;

        public RectShape(Vector2 pos, float size)
        {
            Rect = new(pos, new(size * 2, size * 2), new AnchorPoint(0.5f));
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - Rect.Center;
            Rect = Rect.ChangePosition(offset);
        }
        public override void Rotate(float angleRad)
        {
            //no rotation for rectangles
        }
        public override void Draw(ColorRgba color)
        {
            Rect.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Rect;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            if (shape is PointShape pointShape)
            {
                var point = pointShape.Position;
                var p= Rect.GetClosestPoint(point, out float distance);
                return new ClosestPointResult(
                    p,
                    new CollisionPoint(point, (p.Point - point).Normalize()),
                    distance
                );
            }
            if (shape is SegmentShape segmentShape) return Rect.GetClosestPoint(segmentShape.Segment);
            if (shape is RayShape rayShape) return Rect.GetClosestPoint(rayShape.Ray);
            if (shape is LineShape lineShape) return Rect.GetClosestPoint(lineShape.Line);
            if (shape is CircleShape circleShape) return Rect.GetClosestPoint(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Rect.GetClosestPoint(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Rect.GetClosestPoint(quadShape.Quad);
            if (shape is RectShape rectShape) return Rect.GetClosestPoint(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Rect.GetClosestPoint(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Rect.GetClosestPoint(polylineShape.Polyline);
            return new();
        }
        public override bool OverlapWith(Shape shape)
        {
            if (shape is PointShape pointShape) return Rect.ContainsPoint(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Rect.OverlapShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Rect.OverlapShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Rect.OverlapShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Rect.OverlapShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Rect.OverlapShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Rect.OverlapShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Rect.OverlapShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Rect.OverlapShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Rect.OverlapShape(polylineShape.Polyline);
            return new();
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            if (shape is PointShape pointShape) return null;
            if (shape is SegmentShape segmentShape) return Rect.IntersectShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Rect.IntersectShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Rect.IntersectShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Rect.IntersectShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Rect.IntersectShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Rect.IntersectShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Rect.IntersectShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Rect.IntersectShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Rect.IntersectShape(polylineShape.Polyline);
            return new();
        }
        
        public override Vector2 GetPosition() => Rect.Center;

        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = Rect.GetClosestPoint(polygon);
            return result;

        }
        public override bool OverlapWith(Polygon polygon)
        {
            return Rect.OverlapShape(polygon);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return Rect.IntersectShape(polygon);
        }
        
        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            var v = projectionPosition - Rect.Center;

            return Rect.ProjectShape(v);
        }
        
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = Rect.GetSegment(index);
            return true;
        }
    }
    private class PolygonShape : Shape
    {
        private Vector2 position;
        public readonly Polygon Polygon;

        public PolygonShape(Vector2 pos, float size)
        {
            Polygon = Polygon.Generate(pos, Rng.Instance.RandI(8, 16), size / 2, size);
            position = pos;
        }
        public override void Move(Vector2 newPosition)
        {
            var offset = newPosition - position;
            Polygon.ChangePosition(offset);
            position = newPosition;
        }
        public override void Rotate(float angleRad)
        {
            Polygon.ChangeRotation(angleRad, position);
        }
        public override void Draw(ColorRgba color)
        {
            Polygon.DrawLines(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.Poly;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            if (shape is PointShape pointShape)
            {
                var point = pointShape.Position;
                var p= Polygon.GetClosestPoint(point, out float distance);
                return new ClosestPointResult(
                    p,
                    new CollisionPoint(point, (p.Point - point).Normalize()),
                    distance
                );
            }
            if (shape is SegmentShape segmentShape) return Polygon.GetClosestPoint(segmentShape.Segment);
            if (shape is RayShape rayShape) return Polygon.GetClosestPoint(rayShape.Ray);
            if (shape is LineShape lineShape) return Polygon.GetClosestPoint(lineShape.Line);
            if (shape is CircleShape circleShape) return Polygon.GetClosestPoint(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Polygon.GetClosestPoint(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Polygon.GetClosestPoint(quadShape.Quad);
            if (shape is RectShape rectShape) return Polygon.GetClosestPoint(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Polygon.GetClosestPoint(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Polygon.GetClosestPoint(polylineShape.Polyline);
            return new();
        }
        public override bool OverlapWith(Shape shape)
        {
            if (shape is PointShape pointShape) return Polygon.ContainsPoint(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Polygon.OverlapShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Polygon.OverlapShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Polygon.OverlapShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Polygon.OverlapShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Polygon.OverlapShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Polygon.OverlapShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Polygon.OverlapShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Polygon.OverlapShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Polygon.OverlapShape(polylineShape.Polyline);
            return new();
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            if (shape is PointShape pointShape) return null;
            if (shape is SegmentShape segmentShape) return Polygon.IntersectShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Polygon.IntersectShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Polygon.IntersectShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Polygon.IntersectShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Polygon.IntersectShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Polygon.IntersectShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Polygon.IntersectShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Polygon.IntersectShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Polygon.IntersectShape(polylineShape.Polyline);
            return new();
        }
        
        public override Vector2 GetPosition() => position;

        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = Polygon.GetClosestPoint(polygon);
            return result;

        }
        public override bool OverlapWith(Polygon polygon)
        {
            return Polygon.OverlapShape(polygon);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return Polygon.IntersectShape(polygon);
        }
        
        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            var v = projectionPosition - position;

            return Polygon.ProjectShape(v);
        }
        
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = Polygon.GetSegment(index);
            return true;
        }
    }
    private class PolylineShape : Shape
    {
        private Vector2 position;
        public readonly Polyline Polyline;

        public PolylineShape(Vector2 pos, float size)
        {
            
            Polyline = Polygon.Generate(pos, Rng.Instance.RandI(8, 16), size / 2, size).ToPolyline();
            position = pos;
        }
        public override void Move(Vector2 newPosition)
        {
            
            Polyline.SetPosition(newPosition, position);
            // var offset = newPosition - position;
            // for (var i = 0; i < Polyline.Count; i++)
            // {
            //     var p = Polyline[i];
            //     Polyline[i] = p + offset;
            // }
            position = newPosition;
        }
        public override void Rotate(float angleRad)
        {
            Polyline.ChangeRotation(angleRad, position);
        }
        public override void Draw(ColorRgba color)
        {
            Polyline.Draw(LineThickness, color);
        }

        public override ShapeType GetShapeType() => ShapeType.PolyLine;
        
        public override ClosestPointResult GetClosestPointToShape(Shape shape)
        {
            if (shape is PointShape pointShape)
            {
                var point = pointShape.Position;
                var p= Polyline.GetClosestPoint(point, out float distance);
                return new ClosestPointResult(
                    p,
                    new CollisionPoint(point, (p.Point - point).Normalize()),
                    distance
                );
            }
            if (shape is SegmentShape segmentShape) return Polyline.GetClosestPoint(segmentShape.Segment);
            if (shape is RayShape rayShape) return Polyline.GetClosestPoint(rayShape.Ray);
            if (shape is LineShape lineShape) return Polyline.GetClosestPoint(lineShape.Line);
            if (shape is CircleShape circleShape) return Polyline.GetClosestPoint(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Polyline.GetClosestPoint(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Polyline.GetClosestPoint(quadShape.Quad);
            if (shape is RectShape rectShape) return Polyline.GetClosestPoint(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Polyline.GetClosestPoint(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Polyline.GetClosestPoint(polylineShape.Polyline);
            return new();
        }
        
        public override bool OverlapWith(Shape shape)
        {
            if (shape is PointShape pointShape) return Polyline.ContainsPoint(pointShape.Position);
            if (shape is SegmentShape segmentShape) return Polyline.OverlapShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Polyline.OverlapShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Polyline.OverlapShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Polyline.OverlapShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Polyline.OverlapShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Polyline.OverlapShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Polyline.OverlapShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Polyline.OverlapShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Polyline.OverlapShape(polylineShape.Polyline);
            return new();
        }
        public override CollisionPoints? IntersectWith(Shape shape)
        {
            if (shape is PointShape pointShape) return null;
            if (shape is SegmentShape segmentShape) return Polyline.IntersectShape(segmentShape.Segment);
            if (shape is RayShape rayShape) return Polyline.IntersectShape(rayShape.Ray);
            if (shape is LineShape lineShape) return Polyline.IntersectShape(lineShape.Line);
            if (shape is CircleShape circleShape) return Polyline.IntersectShape(circleShape.Circle);
            if (shape is TriangleShape triangleShape) return Polyline.IntersectShape(triangleShape.Triangle);
            if (shape is QuadShape quadShape) return Polyline.IntersectShape(quadShape.Quad);
            if (shape is RectShape rectShape) return Polyline.IntersectShape(rectShape.Rect);
            if (shape is PolygonShape polygonShape) return Polyline.IntersectShape(polygonShape.Polygon);
            if (shape is PolylineShape polylineShape) return Polyline.IntersectShape(polylineShape.Polyline);
            return new();
        }
        
        public override Vector2 GetPosition() => position;

        public override ClosestPointResult GetClosestPointToPolygon(Polygon polygon)
        {
            var result = Polyline.GetClosestPoint(polygon);
            return result;

        }
        public override bool OverlapWith(Polygon polygon)
        {
            return Polyline.OverlapShape(polygon);
        }
        public override CollisionPoints? IntersectWith(Polygon polygon)
        {
            return Polyline.IntersectShape(polygon);
        }
        
        public override Polygon? GetProjectionPoints(Vector2 projectionPosition)
        {
            var v = projectionPosition - position;

            return Polyline.ProjectShape(v);
        }
        public override bool GetSegment(int index, out Segment segment)
        {
            segment = Polyline.GetSegment(index);
            return true;
        }
    }
    
    
    private InputAction nextStaticShape;
    private InputAction nextMovingShape;
    private InputAction changeMode;
    private InputAction toggleProjection;
    private InputAction rotateMovingShape;
    private InputAction rotateStaticShape;
    private const float rotationSpeedRad = 90 * ShapeMath.DEGTORAD;
    private Shape staticShape;
    private Shape movingShape;
    private Polygon? projection = null;
    private bool projectionActive = false;
    private ShapeMode shapeMode = ShapeMode.Overlap;
    
    public ShapesExample()
    {
        Title = "Shapes Example";

        var nextStaticShapeMb = new InputTypeMouseButton(ShapeMouseButton.LEFT);
        var nextStaticShapeGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_LEFT);
        var nextStaticShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.Q);
        nextStaticShape = new(nextStaticShapeMb, nextStaticShapeGp, nextStaticShapeKb);
        
        var nextMovingShapeMb = new InputTypeMouseButton(ShapeMouseButton.RIGHT);
        var nextMovingShapeGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_FACE_UP);
        var nextMovingShapeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.E);
        nextMovingShape = new(nextMovingShapeMb, nextMovingShapeGp, nextMovingShapeKb);
        
        var changeModeMB = new InputTypeMouseButton(ShapeMouseButton.MIDDLE);
        var changeModeGp = new InputTypeGamepadButton(ShapeGamepadButton.LEFT_TRIGGER_TOP);
        var changeModeKb = new InputTypeKeyboardButton(ShapeKeyboardButton.TAB);
        changeMode = new(changeModeMB, changeModeGp, changeModeKb);
        
        var toggleProjectionGp = new InputTypeGamepadButton(ShapeGamepadButton.RIGHT_TRIGGER_TOP);
        var toggleProjectionKb = new InputTypeKeyboardButton(ShapeKeyboardButton.SPACE);
        toggleProjection = new(toggleProjectionGp, toggleProjectionKb);

        var rotateMovingShapeKb = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.FIVE, ShapeKeyboardButton.SIX);
        var rotateMovingShapeGp = new InputTypeGamepadButtonAxis(ShapeGamepadButton.LEFT_FACE_LEFT, ShapeGamepadButton.LEFT_FACE_RIGHT);
        rotateMovingShape = new(rotateMovingShapeKb, rotateMovingShapeGp);
        
        var rotateStaticShapeKb = new InputTypeKeyboardButtonAxis(ShapeKeyboardButton.SEVEN, ShapeKeyboardButton.EIGHT); 
        var rotateStaticShapeGp = new InputTypeGamepadButtonAxis(ShapeGamepadButton.RIGHT_FACE_LEFT, ShapeGamepadButton.RIGHT_FACE_RIGHT);
        rotateStaticShape = new(rotateStaticShapeKb, rotateStaticShapeGp);
        
        textFont.FontSpacing = 1f;
        textFont.ColorRgba = Colors.Light;

        staticShape = CreateShape(new(), 150, ShapeType.Triangle);
        movingShape = CreateShape(new(), 50, ShapeType.Triangle);

    }
    public override void Reset()
    {
        
    }
    protected override void OnHandleInputExample(float dt, Vector2 mousePosGame, Vector2 mousePosGameUi, Vector2 mousePosUI)
    {
        base.HandleInput(dt, mousePosGame, mousePosGameUi, mousePosUI);
        var gamepad = GAMELOOP.CurGamepad;
        
        nextStaticShape.Gamepad = gamepad;
        nextStaticShape.Update(dt);
        
        nextMovingShape.Gamepad = gamepad;
        nextMovingShape.Update(dt);
        
        changeMode.Gamepad = gamepad;
        changeMode.Update(dt);
        
        toggleProjection.Gamepad = gamepad;
        toggleProjection.Update(dt);
        
        rotateMovingShape.Gamepad = gamepad;
        rotateMovingShape.Update(dt);
        
        rotateStaticShape.Gamepad = gamepad;
        rotateStaticShape.Update(dt);
        
        if (nextStaticShape.State.Pressed)
        {
            NextStaticShape();   
        }
        
        if (nextMovingShape.State.Pressed)
        {
            NextMovingShape(mousePosGame);   
        }

        if (changeMode.State.Pressed)
        {
            switch (shapeMode)
            {
                case ShapeMode.Overlap: 
                    shapeMode = ShapeMode.Intersection;
                    break;
                case ShapeMode.Intersection:
                    shapeMode = ShapeMode.ClosestDistance;
                    break;
                case ShapeMode.ClosestDistance:
                    shapeMode = ShapeMode.Overlap;
                    break;
            }
        }
        
        if (toggleProjection.State.Pressed)
        {
            projectionActive = !projectionActive;
        }
        
        if (rotateMovingShape.State.Down)
        {
            movingShape.Rotate(rotationSpeedRad * dt * rotateMovingShape.State.AxisRaw);
        }
        
        if (rotateStaticShape.State.Down)
        {
            staticShape.Rotate(rotationSpeedRad * dt * rotateStaticShape.State.AxisRaw);
        }
        
        movingShape.Move(mousePosGame);
    }

    protected override void OnUpdateExample(GameTime time, ScreenInfo game, ScreenInfo gameUi, ScreenInfo ui)
    {
        if (projectionActive)
        {
            projection = movingShape.GetProjectionPoints(game.MousePos);
        }
        else
        {
            movingShape.Move(game.MousePos);
        }
    }

    protected override void OnDrawGameExample(ScreenInfo game)
    {
        if (projectionActive)
        {
            if(projection != null) projection.DrawLines(4f, Colors.Special);
        }

        if (shapeMode == ShapeMode.Overlap)
        {
            bool overlap = projectionActive && projection != null ? staticShape.OverlapWith(projection) : staticShape.OverlapWith(movingShape);
            if (overlap)
            {
                staticShape.Draw(Colors.Highlight);
                movingShape.Draw(Colors.Warm);
            }
            else
            {
                staticShape.Draw(Colors.Highlight.ChangeBrightness(-0.3f));
                movingShape.Draw(Colors.Warm.ChangeBrightness(-0.3f));
            }
        }
        else if (shapeMode == ShapeMode.Intersection)
        {
            var result = projectionActive && projection != null ? staticShape.IntersectWith(projection) : movingShape.IntersectWith(staticShape);

            if (result == null || result.Count <= 0)
            {
                staticShape.Draw(Colors.Highlight.ChangeBrightness(-0.3f));
                movingShape.Draw(Colors.Warm.ChangeBrightness(-0.3f));
            }
            else
            {
                staticShape.Draw(Colors.Highlight);
                movingShape.Draw(Colors.Warm);

                foreach (var cp in result)
                {
                    cp.Point.Draw(12f, Colors.Cold, 16);
                    ShapeDrawing.DrawSegment(cp.Point, cp.Point + cp.Normal * 75f, 2f, Colors.Cold, LineCapType.Capped, 4);
                }
            }
            
        }
        else
        {
            staticShape.Draw(Colors.Highlight.ChangeBrightness(-0.3f));
            movingShape.Draw(Colors.Warm.ChangeBrightness(-0.3f));
            
            var closestPointResult = projectionActive && projection != null ? staticShape.GetClosestPointToPolygon(projection) : staticShape.GetClosestPointToShape(movingShape);
            if (closestPointResult.DistanceSquared > 0)
            {
                if (staticShape.GetSegment(closestPointResult.SegmentIndex, out var staticSeg))
                {
                    staticSeg.Draw(LineThickness, Colors.Light);
                }

                if (movingShape.GetSegment(closestPointResult.SegmentIndex, out var movingSeg))
                {
                    movingSeg.Draw(LineThickness, Colors.Light);
                }
                
                var segment = new Segment(closestPointResult.Self.Point, closestPointResult.Other.Point);
                segment.Draw(LineThickness, Colors.Light);
                closestPointResult.Self.Point.Draw(12f, Colors.Highlight);
                closestPointResult.Other.Point.Draw(12f, Colors.Warm);
            
            }
        }
    }
    protected override void OnDrawGameUIExample(ScreenInfo gameUi)
    {
        
    }
    protected override void OnDrawUIExample(ScreenInfo ui)
    {
        
        var curDevice = ShapeInput.CurrentInputDeviceType;
        var curDeviceNoMouse = ShapeInput.CurrentInputDeviceTypeNoMouse;
        var nextStaticText = nextStaticShape.GetInputTypeDescription( curDevice, true, 1, false); 
        var nextMovingText = nextMovingShape.GetInputTypeDescription( curDevice, true, 1, false); 
        var changeModeText = changeMode.GetInputTypeDescription( curDevice, true, 1, false); 
        var toggleProjectionText = toggleProjection.GetInputTypeDescription(curDeviceNoMouse, true, 1, false); 
        var rotateStaticText = rotateStaticShape.GetInputTypeDescription(curDeviceNoMouse, true, 1, false); 
        var rotateMovingText = rotateStaticShape.GetInputTypeDescription(curDeviceNoMouse, true, 1, false); 
        // var offset = changeOffset.GetInputTypeDescription( curDevice , true, 1, false);

        var topCenter = GAMELOOP.UIRects.GetRect("center").ApplyMargins(0,0,0.05f,0.9f);
        textFont.ColorRgba = Colors.Light;
        var mode = 
            shapeMode == ShapeMode.Overlap ? "Overlap" :
            shapeMode == ShapeMode.Intersection ? "Intersection" : 
            "Closest Distance";
        
        textFont.DrawTextWrapNone($"{changeModeText} Mode: {mode} | {toggleProjectionText} Projection {projectionActive}", topCenter, new(0.5f, 0.5f));
        
        var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
        var hSplit = bottomCenter.SplitH(0.45f, 0.1f, 0.45f);
        var margin = bottomCenter.Height * 0.05f;
        var leftRect = hSplit[0];
        var middleRect = hSplit[1];
        var rightRect = hSplit[2];
        
        leftRect.DrawLines(2f, Colors.Highlight);
        rightRect.DrawLines(2f, Colors.Warm);
        // string infoText = $"Add Point {create} | Remove Point {delete} | Inflate {offset} {MathF.Round(offsetDelta * 100) / 100}";

            
        var textStatic = $"{nextStaticText} {staticShape.GetName()}";
        var textMiddle = " vs ";
        var textMoving = $"{movingShape.GetName()} {nextMovingText}";
        
        textFont.ColorRgba = Colors.Highlight;
        textFont.DrawTextWrapNone(textStatic, leftRect.ApplyMarginsAbsolute(margin, margin, margin, margin), new(0f, 0.5f));
        textFont.ColorRgba = Colors.Light;
        textFont.DrawTextWrapNone(textMiddle, middleRect, new(0.5f));
        textFont.ColorRgba = Colors.Warm;
        textFont.DrawTextWrapNone(textMoving, rightRect.ApplyMarginsAbsolute(margin, margin, margin, margin), new(1f, 0.5f));
        
        
        // var curDevice = ShapeInput.CurrentInputDeviceType;
        // var nextStaticText = nextStaticShape. GetInputTypeDescription( curDevice, true, 1, false); 
        // var nextMovingText = nextMovingShape. GetInputTypeDescription( curDevice, true, 1, false); 
        // var changeModeText = changeMode. GetInputTypeDescription( curDevice, true, 1, false); 
        // // var offset = changeOffset.GetInputTypeDescription( curDevice , true, 1, false);
        //
        // var topCenter = GAMELOOP.UIRects.GetRect("center").ApplyMargins(0,0,0.05f,0.9f);
        // textFont.ColorRgba = Colors.Light;
        // var mode = 
        //     shapeMode == ShapeMode.Overlap ? "Overlap" :
        //     shapeMode == ShapeMode.Intersection ? "Intersection" : 
        //     "Closest Distance";
        //
        // textFont.DrawTextWrapNone($"{changeModeText} Mode: {mode}", topCenter, new(0.5f, 0.5f));
        //
        // var bottomCenter = GAMELOOP.UIRects.GetRect("bottom center");
        // var hSplit = bottomCenter.SplitH(0.45f, 0.1f, 0.45f);
        // var margin = bottomCenter.Height * 0.05f;
        // var leftRect = hSplit[0];
        // var middleRect = hSplit[1];
        // var rightRect = hSplit[2];
        //
        // leftRect.DrawLines(2f, Colors.Highlight);
        // rightRect.DrawLines(2f, Colors.Warm);
        // // string infoText = $"Add Point {create} | Remove Point {delete} | Inflate {offset} {MathF.Round(offsetDelta * 100) / 100}";
        //
        //     
        // var textStatic = $"{nextStaticText} {staticShape.GetName()}";
        // var textMiddle = " vs ";
        // var textMoving = $"{movingShape.GetName()} {nextMovingText}";
        //
        // textFont.ColorRgba = Colors.Highlight;
        // textFont.DrawTextWrapNone(textStatic, leftRect.ApplyMarginsAbsolute(margin, margin, margin, margin), new(0f, 0.5f));
        // textFont.ColorRgba = Colors.Light;
        // textFont.DrawTextWrapNone(textMiddle, middleRect, new(0.5f));
        // textFont.ColorRgba = Colors.Warm;
        // textFont.DrawTextWrapNone(textMoving, rightRect.ApplyMarginsAbsolute(margin, margin, margin, margin), new(1f, 0.5f));
    }

    private void NextStaticShape(float size = 300f)
    {
        switch (staticShape.GetShapeType())
        {
            case ShapeType.None: staticShape = CreateShape(new(), size, ShapeType.Segment); //point
                break;
            case ShapeType.Segment: staticShape = CreateShape(new(), size, ShapeType.Ray);
                break;
            case ShapeType.Ray: staticShape = CreateShape(new(), size, ShapeType.Line);
                break;
            case ShapeType.Line: staticShape = CreateShape(new(), size, ShapeType.Circle);
                break;
            case ShapeType.Circle: staticShape = CreateShape(new(), size, ShapeType.Triangle);
                break;
            case ShapeType.Triangle: staticShape = CreateShape(new(), size, ShapeType.Quad);
                break;
            case ShapeType.Quad: staticShape = CreateShape(new(), size, ShapeType.Rect);
                break;
            case ShapeType.Rect: staticShape = CreateShape(new(), size, ShapeType.Poly);
                break;
            case ShapeType.Poly: staticShape = CreateShape(new(), size, ShapeType.PolyLine);
                break;
            case ShapeType.PolyLine: staticShape = CreateShape(new(), size / 4, ShapeType.None);
                break;
        }
    }
    private void NextMovingShape(Vector2 pos, float size = 125f)
    {
        switch (movingShape.GetShapeType())
        {
            case ShapeType.None: movingShape = CreateShape(pos, size, ShapeType.Segment); //point
                break;
            case ShapeType.Segment: movingShape = CreateShape(pos, size, ShapeType.Ray);
                break;
            case ShapeType.Ray: movingShape = CreateShape(pos, size, ShapeType.Line);
                break;
            case ShapeType.Line: movingShape = CreateShape(pos, size, ShapeType.Circle);
                break;
            case ShapeType.Circle: movingShape = CreateShape(pos, size, ShapeType.Triangle);
                break;
            case ShapeType.Triangle: movingShape = CreateShape(pos, size, ShapeType.Quad);
                break;
            case ShapeType.Quad: movingShape = CreateShape(pos, size, ShapeType.Rect);
                break;
            case ShapeType.Rect: movingShape = CreateShape(pos, size, ShapeType.Poly);
                break;
            case ShapeType.Poly: movingShape = CreateShape(pos, size, ShapeType.PolyLine);
                break;
            case ShapeType.PolyLine: movingShape = CreateShape(pos, size / 4, ShapeType.None);
                break;
        }
    }
    private Shape CreateShape(Vector2 pos, float size, ShapeType type)
    {
        switch (type)
        {
            case ShapeType.None: return new PointShape(pos, size);
            case ShapeType.Circle: return new CircleShape(pos, size);
            case ShapeType.Segment: return new SegmentShape(pos, size);
            case ShapeType.Ray: return new RayShape(pos, Rng.Instance.RandVec2().Normalize());
            case ShapeType.Line: return new LineShape(pos, Rng.Instance.RandVec2().Normalize());
            case ShapeType.Triangle: return new TriangleShape(pos, size);
            case ShapeType.Quad: return new QuadShape(pos, size);
            case ShapeType.Rect: return new RectShape(pos, size);
            case ShapeType.Poly: return new PolygonShape(pos, size);
            case ShapeType.PolyLine: return new PolylineShape(pos, size);
        }
        
        return new PointShape(pos, size);
    }
    
}


