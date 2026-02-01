using Godot;
using System;

public partial class linear : PathFollow2D
{
    [Export]
    private float speed = 60;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {
		Progress += (float)delta * speed;
	}
}
