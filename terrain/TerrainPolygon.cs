using Godot;
using System;

[GlobalClass]
public partial class TerrainPolygon : Polygon2D {
    [Export] public Geometry2D.PolyBooleanOperation booleanOperation = Geometry2D.PolyBooleanOperation.Union;
}
