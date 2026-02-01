using Godot;
using System;

public partial class Survival : Node2D {
    private Player player;
    private Terrain terrain;
    private Control deathScreen;
    private Label timeLeftMessage;
[Export]    private Game.Timer timer;

    private bool Dying = false;
    private const double DEATH_TIME = 2;
    private double TimeLeft = DEATH_TIME;
    private Terrain.WorldOp deathCircle;
    private Vector2 deathCircleCenter = new Vector2(0, 0);
    private float deathCircleRadius = 1;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        terrain = GetNode<Terrain>("Terrain");
        player = GetNode<Player>("Player");
        deathScreen = GetNode<Control>("DeathScreen");
        timeLeftMessage = GetNode<Label>("DeathScreen/TileLeftText");
        
        player.OnDeath += OnDeath;
        deathCircle = new Terrain.WorldOp(PolygonUtils.CreateNGon(1, 24), Geometry2D.PolyBooleanOperation.Difference);
    }

    public override void _Process(double delta) {
        if (!Dying) return;

        deathCircleRadius += (float)delta * 1000;
        UpdateDeathCircle();
            
        if (TimeLeft <= 0) {
            deathScreen.Show();
            timer.Visible = false;
        }

        TimeLeft -= delta;
    }

    private bool reloading;

    public override void _Input(InputEvent @event) {
        base._Input(@event);
        if (TimeLeft > 0 || @event is not InputEventKey) return;
        var tree = GetTree();
        tree.CreateTimer(0.1).Timeout += () => {
            if (reloading) return;
            reloading = true;
            tree.ReloadCurrentScene();
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    private void OnDeath() {
        Dying = true;
        deathCircleCenter = new Vector2(player.Position.X, GetViewportRect().Size.Y);
        UpdateDeathCircle();
        terrain.operations.Add(deathCircle);
        timeLeftMessage.Text = $"You survived for {timer.GetTime()}";

        foreach (var child in GetChildren()) {
            if (child is Timer timer) {
                timer.Stop();
            }
        }
    }

    private void UpdateDeathCircle() {
        deathCircle.points = PolygonUtils.CreateNGon(deathCircleRadius, 24);
        
        for (int i = 0; i < deathCircle.points.Length; i++) {
            deathCircle.points[i] += deathCircleCenter;
        }
    }
}
