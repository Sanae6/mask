using Godot;
using System;

public partial class HoleSpawner : Node2D {
    [Export] public Terrain terrain;
    [Export] public Shape2D bulletShape;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        var timer = new Timer();
        timer.Autostart = true;
        timer.WaitTime = 0.5;
        timer.Timeout += () => CreateCrawler(new Vector2((float)GD.RandRange(0, GetViewportRect().Size.X), 0),
            Mathf.Pi / 2 + (float)GD.RandRange(-0.5, 0.5), 150, bulletShape, 10.0f);
        AddChild(timer);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
    }

    private CrawlingTerrain CreateCrawler(Vector2 position, float rotation, float speed, Shape2D shape, float lifeTime) {
        var crawler = new CrawlingTerrain();
        crawler.terrain = terrain;
        crawler.Position = position;
        crawler.Rotation = rotation;
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