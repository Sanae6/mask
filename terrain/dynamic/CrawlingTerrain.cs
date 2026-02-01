using Godot;

[GlobalClass]
public partial class CrawlingTerrain : DynamicTerrainObj {
    [Export] public float Speed = 10.0f;
    [Export] public float Direction = 0.0f;
    [Export] public float LifeTime = 5.0f;
    public bool ShowGradientTexture;
    private GradientTexture2D texture;

    public override void _Ready() {
        base._Ready();
        GetTree().CreateTimer(LifeTime).Timeout += QueueFree;
        texture = GD.Load<GradientTexture2D>("res://survival/fallingGradient.tres");
    }

    public override void _Process(double delta) {
        Position += new Vector2(Speed * (float)delta, 0).Rotated(Direction);
        base._Process(delta);
    }

    public override void _Draw() {
        if (ShowGradientTexture)
            DrawTexture(texture, -texture.GetSize() / 2, new Color(Colors.White, 0.5f));
    }
}