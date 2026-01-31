using Godot;

public interface TerrainObj {
    public Geometry2D.PolyBooleanOperation booleanOperation { get; set; }
    
    public Vector2[] GetPolygon();
}