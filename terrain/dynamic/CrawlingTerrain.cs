using Godot;

[GlobalClass]
public partial class CrawlingTerrain : DynamicTerrainObj {
    [Export] private float speed = 10.0f;

    public override void _Process(double delta) {
        Position += new Vector2(speed*(float)delta, 0).Rotated(Rotation);
        base._Process(delta);
    }
}