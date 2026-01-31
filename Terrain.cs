using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;
using Godot.NativeInterop;

[GlobalClass]
public partial class Terrain : StaticBody2D
{
	public List<WorldOp> operations = [];

	private uint shapeOwnerId;
	private List<Vector2[]> polygons = [];
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		operations.Add(new WorldOp([new Vector2(0, 500), new Vector2(1150,400), new Vector2(1150,600), new Vector2(0, 600)],  Geometry2D.PolyBooleanOperation.Union));
		operations.Add(new WorldOp([new Vector2(200, 330), new Vector2(200, 660), new Vector2(300, 650), new Vector2(300, 330)],  Geometry2D.PolyBooleanOperation.Union));
		shapeOwnerId = CreateShapeOwner(this);
		RecalculatePolygons();
		foreach (var polygon in polygons)
		{
			ConvexPolygonShape2D shape = new ConvexPolygonShape2D();
			shape.Points = polygon;
			ShapeOwnerAddShape(shapeOwnerId, shape);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Draw()
	{
		foreach (var polygon in polygons)
		{
			DrawColoredPolygon(polygon, new Color(GD.Randf(), GD.Randf(), GD.Randf()));
		}
	}

	private void RecalculatePolygons()
	{
		List<Vector2[]> new_polygons = [[]];
		
		foreach (var operation in operations)
		{
			switch (operation.boolOperation)
			{
				case Geometry2D.PolyBooleanOperation.Union:
					var added = false;
					for (var i = 0; i < new_polygons.Count && !added; i++)
					{
						var mergedPolygons = Geometry2D.MergePolygons(new_polygons[i], operation.points);
						if (mergedPolygons.Count != 1) continue;
						new_polygons[i] = mergedPolygons[0];
						added = true;
					}
					if (!added)
					{
						new_polygons.Add(operation.points);
					}
					break;
			}
		}
		
		polygons.Clear();
		foreach (var polygon in new_polygons)
		{
			polygons.AddRange(Geometry2D.DecomposePolygonInConvex(polygon));
		}
		
		QueueRedraw();
	}

	public record struct WorldOp(Vector2[] points, Geometry2D.PolyBooleanOperation boolOperation);
}
