extends Node2D


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
    var parent_pos = get_parent().position
    var callback = ($"../../FollowPlayer" as Node2D).set_position.bind(parent_pos)
    get_tree().create_timer(1.0).timeout.connect(callback)
