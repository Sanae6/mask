extends Node2D

@onready var terrain: Terrain = $"../Terrain"

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
    pass # Replace with function body.

func _process(delta: float) -> void:
    if (randf()<0.05):
        create_crawler(Vector2(0, 0), randf_range(0, PI), randf_range(150, 200))

# Called every frame. 'delta' is the elapsed time since the previous frame.
func create_crawler(pos: Vector2, rotation: float, speed: float):
    var crawler := CrawlingTerrain.new()
    crawler.terrain = terrain
    crawler.position = pos
    crawler.rotation = rotation
    crawler.speed = speed
    
    var terrain_shape := TerrainShape.new()
    terrain_shape.shape = CircleShape2D.new()
    (terrain_shape.shape as CircleShape2D).radius = 100
    terrain_shape.booleanOperation = Geometry2D.PolyBooleanOperation.OPERATION_DIFFERENCE
    crawler.add_child(terrain_shape)
    add_child(crawler)
    
