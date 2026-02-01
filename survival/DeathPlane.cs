using Godot;
using System;

public partial class DeathPlane : Area2D {
    [Export] private Player player;

    public override void _Ready() { 
        BodyEntered += a => {
            if (a == player)
                player.Killed();
        };
    }
}