using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class TerrainShape : CollisionShape2D {
    [Export] public Geometry2D.PolyBooleanOperation booleanOperation = Geometry2D.PolyBooleanOperation.Union;

    public Vector2[] GetPolygon() {
        switch (Shape) {
            case RectangleShape2D rectShape: {
                Vector2[] points = PolygonUtils.CreateCenteredRect(rectShape.Size);
                return points.Select(p => GlobalTransform * p).ToArray();
            }

            case CircleShape2D circleShape: {
                Vector2[] points = PolygonUtils.CreateNGon(circleShape.Radius, 24);
                return points.Select(p => GlobalTransform * p).ToArray();
            }

            default:
                throw new Exception("unsupported terrain shape dumbass");
        }
    }
}