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

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventMouseButton { Pressed: true } eventMouseButton) return;
		
		var op = new WorldOp([new Vector2(25, 25), new Vector2(25, -25), new Vector2(-25, -25), new Vector2(-25, 25)], eventMouseButton.ButtonIndex == MouseButton.Left ? Geometry2D.PolyBooleanOperation.Union : Geometry2D.PolyBooleanOperation.Difference);
		for (var i = 0; i < op.points.Length; i++)
		{
			op.points[i] += eventMouseButton.Position;
		}
		operations.Add(op);
		RecalculatePolygons();
	}

	private void RecalculatePolygons()
	{
		List<Vector2[]> newPolygons = [[]];
		
		foreach (var operation in operations)
		{
			switch (operation.boolOperation)
			{
				case Geometry2D.PolyBooleanOperation.Union:
					var added = false;
					for (var i = 0; i < newPolygons.Count && !added; i++)
					{
						var mergedPolygons = Geometry2D.MergePolygons(newPolygons[i], operation.points);
						if (mergedPolygons.Count != 1) continue;
						newPolygons[i] = mergedPolygons[0];
						added = true;
					}
					if (!added)
					{
						newPolygons.Add(operation.points);
					}
					break;
				
				case Geometry2D.PolyBooleanOperation.Difference:
					var originalPolygonCount = newPolygons.Count;
					for (var i = 0; i < originalPolygonCount; i++)
					{
						var clippedPolygons = Geometry2D.ClipPolygons(newPolygons[i], operation.points);
						if (clippedPolygons.Count == 2)
						{
						 //
						}
						newPolygons[i] = clippedPolygons[0];
						clippedPolygons.RemoveAt(0);
						newPolygons.AddRange(clippedPolygons);
					}
					break;
			}
		}
		
		polygons.Clear();
		foreach (var polygon in newPolygons)
		{
			polygons.AddRange(Geometry2D.DecomposePolygonInConvex(polygon));
		}
		
		QueueRedraw();
	}

	public record struct WorldOp(Vector2[] points, Geometry2D.PolyBooleanOperation boolOperation);
}
