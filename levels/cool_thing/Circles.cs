using Godot;
using System;
using System.Transactions;

public partial class Circles : DynamicTerrainObj
{
    [Export] float speed = 1.0f;

    public override void _Ready() {
        base._Ready();
        GD.Print(speed);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        base._Process(delta);
        Rotate((float)delta*Mathf.Pi*2*speed);
    }
}
