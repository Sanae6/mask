using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class DynamicTerrainObj : Node2D {
    [Export] private Terrain terrain;
    private List<(Vector2[], Terrain.WorldOp)> operations = [];

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

        if (terrain is null) {
            QueueFree();
            throw new NullReferenceException("no terrain object was linked");
        }

        foreach (var child in GetChildren()) {
            if (child is not TerrainObj terrainObj) continue;
            var polygon = terrainObj.GetPolygon().Clone() as Vector2[];
            var worldOp = new Terrain.WorldOp(polygon, terrainObj.booleanOperation);
            operations.Add((polygon, worldOp));
            terrain.operations.Add(worldOp);
            child.QueueFree();
        }

        terrain.QueueRecalculation();
    }

    public override void _Process(double delta) {
        foreach (var (polygon, worldOp) in operations) {
            worldOp.points = polygon.Select(x => GlobalTransform * x).ToArray();
        }

        terrain.QueueRecalculation();
    }
}