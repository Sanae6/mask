using Godot;

namespace Mask;

[GlobalClass]
public partial class DynamicTerrainShape : TerrainShape {
    private Terrain terrain;
    private Vector2[] points;
    public override void _Ready() {
        Node parent = GetParent();
        while (parent is { } node) {
            if (node is Terrain temp) {
                terrain = temp;
                break;
            }

            parent = node.GetParent();
        }

        points = GetPolygon();
        terrain.operations.Add(new Terrain.WorldOp(points, booleanOperation));
    }

    public override void _Process(double delta) {
        GetPolygon().CopyTo(points, 0);
        
        terrain.QueueRecalculation();
    }
}