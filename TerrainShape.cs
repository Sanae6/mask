using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class TerrainShape : CollisionShape2D
{
    [Export] public Geometry2D.PolyBooleanOperation booleanOperation = Geometry2D.PolyBooleanOperation.Union;

    public Vector2[] getPolygon() {
        switch (Shape) {
            case RectangleShape2D rectShape:
                Vector2[] points = [
                    new (-rectShape.Size.X / 2, -rectShape.Size.Y / 2),
                    new (-rectShape.Size.X / 2, rectShape.Size.Y / 2),
                    new (rectShape.Size.X / 2, rectShape.Size.Y / 2),
                    new (rectShape.Size.X / 2, -rectShape.Size.Y / 2)
                ];

                return points.Select(p => Transform * p).ToArray();
                
            default:
                throw new Exception("unsupported terrain shape dumbass");
        }
    }
}
