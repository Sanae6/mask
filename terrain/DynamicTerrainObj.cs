using System.Linq;
using Godot;

namespace Mask;

[GlobalClass]
public partial class DynamicTerrainObj : Node2D {
    [Export] private Terrain terrain;
    private Terrain.WorldOp worldOp;
    
    Vector2[] polygon;
    public override void _Ready() {
        if (terrain == null) {
            Node parent = GetParent();
            while (parent is { } node) {
                if (node is Terrain temp) {
                    terrain = temp;
                    break;
                }

                parent = node.GetParent();
            }
        }
        
        foreach (var child in GetChildren()) {
            if (child is not TerrainObj terrainObj) continue;
            polygon = terrainObj.GetPolygon().Clone() as Vector2[];
            worldOp = new Terrain.WorldOp(polygon, terrainObj.booleanOperation);
            child.QueueFree();
            break;
        }
        
        terrain.operations.Add(worldOp);
        terrain.QueueRecalculation();
    }
    
    public override void _Process(double delta) {
        worldOp.points = polygon.Select(x => GlobalTransform * x).ToArray();
        terrain.QueueRecalculation();
    }

}