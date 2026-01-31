using Godot;
using System;
using System.Linq;

[GlobalClass]
public partial class TerrainPolygon : Polygon2D, TerrainObj {
    [Export] public Geometry2D.PolyBooleanOperation booleanOperation { get; set; } = Geometry2D.PolyBooleanOperation.Union;

    public Vector2[] GetPolygon() {
        return Polygon.Select(p => Transform * p).ToArray();
    }
}
