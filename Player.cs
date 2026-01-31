using Godot;
using System;

public partial class Player : CharacterBody2D {
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	private int coyote = 0;
    private int jumpTime = 0; 

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta) {
		var newVel = Velocity;
		if (!IsOnFloor()) {
            if (newVel.Y)
			newVel.Y -= -1000 * (float)delta;
			coyote = Mathf.Max(coyote - 1, 0);
		} else {
			coyote = 16;
		}

		if (Input.IsActionJustPressed("ui_accept") && coyote > 0) {
			newVel.Y = -400;
			coyote = 0;
		}

		var axis = Input.GetAxis("ui_left", "ui_right");
		var axisSign = Mathf.Sign(axis);
		if (axis != 0 && (newVel.X == 0 || axisSign == Mathf.Sign(newVel.X))) {
			newVel.X += axis * 30;
		} else {
			newVel.X = Mathf.MoveToward(newVel.X, 0, IsOnFloor() ? 80 : 60);
		}

		const float speedCap = 500;
		newVel.X = Mathf.Clamp(newVel.X, -speedCap, speedCap);

		Velocity = newVel;

		MoveAndSlide();
	}
}
