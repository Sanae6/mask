using Godot;
using System;

namespace Game;

public partial class Timer : Label {
    [Export] private Player player;
    private ulong startTime;
    private ulong? endTime;

    public override void _Ready() {
        startTime = Time.GetTicksMsec();

        player.OnDeath += () => endTime = Time.GetTicksMsec();
    }

    public string GetTime() {
        long nsTicks = (long)((endTime ?? Time.GetTicksMsec()) - startTime) * (long)1e4;
        return new TimeSpan(nsTicks).ToString("m':'ss");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        Text = GetTime();
    }
}