using Godot;
using System;

public partial class Player : CharacterBody2D {
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
	}

	private int coyote;
    private float jumpGrav;
    private const int CoyoteDuration = 16;
    private const float JumpBurst = -1000;
    private const float JumpGravMultiplier = 50;
    private const float JumpGravReleaseMin = 100;
    private const float JumpHoldDuration = 20;
    private const float SpeedCap = 800;
    private const int GroundFriction = 80;
    private const int AirFriction = 80;
    private const int MoveMultiplier = 80;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta) {
		Vector2 newVel = Velocity;
		if (!IsOnFloor()) {
			coyote = Mathf.Max(coyote - 1, 0);
            jumpGrav += JumpGravMultiplier * (float) delta;
            newVel.Y += jumpGrav;
        } else {
			coyote = CoyoteDuration;
            jumpGrav = 0;
        }

		if (Input.IsActionJustPressed("ui_accept") && coyote > 0) {
			newVel.Y = -JumpBurst;
			coyote = 0;
            jumpGrav = 0;
        }

        if (jumpGrav < JumpHoldDuration && Input.IsActionPressed("ui_accept")) {
            newVel.Y = JumpBurst;
        } else {
            jumpGrav = Mathf.Max(jumpGrav, JumpGravReleaseMin);
        }

        
		float axis = Input.GetAxis("ui_left", "ui_right");
		int axisSign = Mathf.Sign(axis);
		if (axis != 0 && (newVel.X == 0 || axisSign == Mathf.Sign(newVel.X))) {
			newVel.X += axis * MoveMultiplier;
		} else {
			newVel.X = Mathf.MoveToward(newVel.X, 0, IsOnFloor() ? GroundFriction : AirFriction);
		}

        newVel.X = Mathf.Clamp(newVel.X, -SpeedCap, SpeedCap);

		Velocity = newVel;

		MoveAndSlide();
	}

}
