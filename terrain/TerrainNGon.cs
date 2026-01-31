using Godot;
using System;
using System.Linq;

[GlobalClass]
[Tool]
public partial class TerrainNGon : Node2D, TerrainObj
{
    [Export] public Geometry2D.PolyBooleanOperation booleanOperation { get; set; } = Geometry2D.PolyBooleanOperation.Union;

    [Export] public int sides {
        get => _sides;
        set { _sides = value; QueueRedraw(); }
    }
    [Export] public float radius {
        get => _radius;
        set { _radius = value; QueueRedraw(); }
    }

    private int _sides = 3;
    private float _radius = 1.0f;

    public override void _Draw() {
        DrawColoredPolygon(PolygonUtils.CreateNGon(radius, sides), Colors.White);
    }

    public Vector2[] GetPolygon() {
        Vector2[] points = PolygonUtils.CreateNGon(radius, sides);
        return points.Select(p => Transform * p).ToArray();
    }

}
