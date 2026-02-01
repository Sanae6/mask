using Godot;
using System;

public partial class HoleSpawner : Node2D {
    [Export] public Terrain terrain;
    [Export] public Shape2D bulletShape;
    [Export] public Shape2D groundHoleShape;
    [Export] public PackedScene platformScene;

    private Timer bulletTimer;
    private Timer groundHoleTimer;
    private Timer platformTimer;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        bulletTimer = GetNode<Timer>("../BulletTimer");
        groundHoleTimer = GetNode<Timer>("../GroundHoleTimer");
        platformTimer = GetNode<Timer>("../PlatformTimer");
        
        bulletTimer.Timeout += SpawnBulletHole;
        groundHoleTimer.Timeout += SpawnGroundHole;
        platformTimer.Timeout += SpawnPlatform;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private void SpawnBulletHole() {
        CreateCrawler(new Vector2(GetRoundBoundedPos().X, 0),
            0, Mathf.Pi / 2 + (float)GD.RandRange(-0.75, 0.75), 150, bulletShape, 10.0f);
    }

    private void SpawnGroundHole() {
        var isMovingLeft = GD.Randi() % 2 == 0;
        const int X_OFFSET = 150;
        var pos = new Vector2(isMovingLeft ? GetViewportRect().Size.X+X_OFFSET : -X_OFFSET, GetViewportRect().Size.Y);
        var direction = (isMovingLeft ? Mathf.Pi : 0);
        var rotation = (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
        CreateCrawler(pos, rotation, direction, 150, groundHoleShape, 10.0f);
    }

    private void SpawnPlatform() {
        var platform = platformScene.Instantiate() as CrawlingTerrain;
        platform.Position = new Vector2(GetRoundBoundedPos().X, GetViewportRect().Size.Y + 200);
        platform.terrain = terrain;
        AddChild(platform);
    }

    private Vector2 GetRoundBoundedPos() {
        return new Vector2((float)GD.RandRange(0, GetViewportRect().Size.X),
            (float)GD.RandRange(0, GetViewportRect().Size.Y));
    }

    private CrawlingTerrain CreateCrawler(Vector2 position, float rotation, float direction, float speed, Shape2D shape, float lifeTime) {
        var crawler = new CrawlingTerrain();
        crawler.terrain = terrain;
        crawler.Position = position;
        crawler.Rotation = rotation;
        crawler.Direction = direction;
        crawler.Speed = speed;
        crawler.LifeTime = lifeTime;

        var terrainShape = new TerrainShape();
        terrainShape.Shape = shape;
        terrainShape.booleanOperation = Geometry2D.PolyBooleanOperation.Difference;
        
        crawler.AddChild(terrainShape);
        AddChild(crawler);

        return crawler;
    }
}