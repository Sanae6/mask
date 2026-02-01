using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using FileAccess = Godot.FileAccess;

public partial class Bapple : Node2D {
    [Export] private Terrain terrain;
    private List<List<Terrain.WorldOp>> Ops = new List<List<Terrain.WorldOp>>();

    public override void _EnterTree() {
        var data = File.ReadAllText("/home/aubrey/Projects/mask/badapple/badapple.json");
        var desList = JsonSerializer.Deserialize<List<List<Des>>>(data);
        Ops = desList.Select(x => {
            return x.Select(y =>
                    new Terrain.WorldOp(y.points.Select(v => v.V()).ToArray(),
                        y.is_hole ? Geometry2D.PolyBooleanOperation.Difference : Geometry2D.PolyBooleanOperation.Union))
                .ToList();
        }).ToList();
        QueueRedraw();
        Engine.MaxFps = 30;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        QueueRedraw();
    }

    private int frame;

    public override void _Draw() {
        if (frame >= Ops.Count) return;
        terrain.operations = Ops[frame++];
        terrain.QueueRecalculation();
        //foreach (var des in ) {
        //    GD.Print($"{des.points[0].V()}");
        //    for (var i = 0; i < des.points.Count - 1; i++) {
        //        DrawLine(des.points[i].V(), des.points[i + 1].V(), Colors.Magenta, 5);
        //    }
        //}
    }

    private class Des {
        [JsonPropertyName("points")] public required List<V2> points { get; set; }
        [JsonPropertyName("is_hole")] public required bool is_hole { get; set; }
    }

    private struct V2 {
        [JsonPropertyName("X")] public required float X { get; set; }
        [JsonPropertyName("Y")] public required float Y { get; set; }

        public Vector2 V() {
            return new Vector2(X, Y);
        }
    }
}