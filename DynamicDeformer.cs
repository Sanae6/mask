using Godot;
using System;

public partial class DynamicDeformer : Node2D {
    private Terrain terrain;
    private Vector2[] points;
    [Export] private int pointCount = 4;

    public override void _Ready() {
        Node parent = GetParent();
        while (parent is { } node) {
            if (node is Terrain temp) {
                terrain = temp;
                break;
            }

            parent = node.GetParent();
        }

        if (terrain is null)
            throw new Exception("deformer must be parented to a terrain");

        points = new Vector2[pointCount];
        terrain.operations.Add(new Terrain.WorldOp(points, Geometry2D.PolyBooleanOperation.Difference));
    }

    private double time;

    public override void _Process(double delta) {
        time += delta / 5;

        float interval = (Mathf.Pi * 2 / pointCount);
        float startOffset = interval / 2;
        for (int i = 0; i < pointCount; i++) {
            float rad = startOffset + i * interval;
            points[i] = Position +
                        new Vector2((float)Mathf.Sin(time) * 500 + Mathf.Cos(rad) * 100, Mathf.Sin(rad) * 100);
        }

        terrain.QueueRecalculation();
    }
}