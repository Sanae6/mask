using Godot;
using System;

public partial class Spotlight : Node2D {
    private double time;

    public override void _Process(double delta) {
        time += delta;

        Rotation = (float)Mathf.Sin(time) / 5f;
    }
}