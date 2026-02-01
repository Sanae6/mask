using System;
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

        if (terrain is null) {
            QueueFree();
            throw new NullReferenceException("no terrain object was linked");
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

    public override void _Draw() {
        var points = worldOp.points;
        for (var i = 0; i < points.Length; i++) {
            var (start, end) = (points[i + 0], points[(i + 1) % points.Length]);
            DrawLine(start, end, Colors.White);
            DrawDashedLine(start, end, Colors.Black, 4, 8);
        }

        base._Draw();
    }
}