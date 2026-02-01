using Godot;
using System;
using System.ComponentModel;

public partial class Player : CharacterBody2D {
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
    }

    public void Killed() {
        GetTree().ReloadCurrentScene();
    }

    private int coyote;
    private float jumpGrav;

    [Export] private int coyoteDuration = 16;
    [Export] private float jumpBurst = -1000;
    [Export] private float jumpGravMultiplier = 50;
    [Export] private float jumpGravReleaseMin = 100;
    [Export] private float jumpHoldDuration = 10;
    [Export] private float speedCap = 800;
    [Export] private int turnAroundDelta = 200;
    [Export] private int groundFriction = 80;
    [Export] private int airFriction = 80;
    [Export] private int moveMultiplier = 80;

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta) {
        Vector2 newVel = Velocity;
        if (!IsOnFloor()) {
            coyote = Mathf.Max(coyote - 1, 0);
            jumpGrav += jumpGravMultiplier * (float)delta;
            newVel.Y += jumpGrav;
        } else {
            coyote = coyoteDuration;
            jumpGrav = jumpGravReleaseMin;
        }

        if (Input.IsActionJustPressed("ui_accept") && coyote > 0) {
            newVel.Y = -jumpBurst;
            coyote = 0;
            jumpGrav = 0;
        }

        if (jumpGrav < jumpHoldDuration && Input.IsActionPressed("ui_accept")) {
            newVel.Y = jumpBurst;
        } else {
            jumpGrav = Mathf.Max(jumpGrav, jumpGravReleaseMin);
        }


        float axis = Input.GetAxis("ui_left", "ui_right");
        if (axis != 0 && (newVel.X == 0 || Mathf.Sign(axis) == Mathf.Sign(newVel.X))) {
            newVel.X += axis * moveMultiplier;
        } else {
            newVel.X = -Mathf.Sign(axis) == Mathf.Sign(newVel.X)
                ? Mathf.MoveToward(newVel.X, 0, 200)
                : Mathf.MoveToward(newVel.X, 0, IsOnFloor() ? groundFriction : airFriction);
        }

        newVel.X = Mathf.Clamp(newVel.X, -speedCap, speedCap);

        Velocity = newVel;

        MoveAndSlide();
    }
}