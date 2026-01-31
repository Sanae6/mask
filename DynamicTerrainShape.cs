using Godot;

namespace Mask;

[GlobalClass]
public partial class DynamicTerrainShape : TerrainShape {
    private Terrain terrain;
    private Terrain.WorldOp worldOp;
    public override void _Ready() {
        Node parent = GetParent();
        while (parent is { } node) {
            if (node is Terrain temp) {
                terrain = temp;
                break;
            }

            parent = node.GetParent();
        }

        worldOp = new Terrain.WorldOp(GetPolygon(), booleanOperation);
        terrain.operations.Add(worldOp);
    }

    public override void _Process(double delta) {
        GetPolygon().CopyTo(worldOp.points, 0);
        worldOp.boolOperation = booleanOperation;
        
        terrain.QueueRecalculation();
    }
}