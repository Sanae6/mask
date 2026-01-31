using Godot;
using System;

public partial class Player : CharacterBody2D {
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	private int coyote = 0;
    private float jumpTime = 0;
    private float jumpGrav = 0;
    private const float JumpDuration = 20;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta) {
		var newVel = Velocity;
		if (!IsOnFloor()) {
			coyote = Mathf.Max(coyote - 1, 0);
            jumpGrav += 50 * (float) delta;
            newVel.Y += jumpGrav;
        } else {
			coyote = 16;
            jumpGrav = 0;
        }

		if (Input.IsActionJustPressed("ui_accept") && coyote > 0) {
			newVel.Y = -1000;
			coyote = 0;
            jumpTime = JumpDuration;
            jumpGrav = 0;
        }

        if (jumpGrav < 10 && Input.IsActionPressed("ui_accept")) {
            newVel.Y = -1000;
        } else {
            jumpGrav = Mathf.Max(jumpGrav, 100);
        }

        
		var axis = Input.GetAxis("ui_left", "ui_right");
		var axisSign = Mathf.Sign(axis);
		if (axis != 0 && (newVel.X == 0 || axisSign == Mathf.Sign(newVel.X))) {
			newVel.X += axis * 30;
		} else {
			newVel.X = Mathf.MoveToward(newVel.X, 0, IsOnFloor() ? 100 : 80);
		}

		const float speedCap = 800;
		newVel.X = Mathf.Clamp(newVel.X, -speedCap, speedCap);

		Velocity = newVel;

		MoveAndSlide();
	}
}
