using Godot;

[GlobalClass]
public partial class CrawlingTerrain : DynamicTerrainObj {
    [Export] public float Speed = 10.0f;
    [Export] public float LifeTime = 5.0f;

    public override void _Ready() {
        base._Ready();
        GetTree().CreateTimer(LifeTime).Timeout += QueueFree;
    }
    
    public override void _Process(double delta) {
        Position += new Vector2(Speed*(float)delta, 0).Rotated(Rotation);
        base._Process(delta);
    }
}