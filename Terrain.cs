using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Collections;
using Godot.NativeInterop;
using Array = System.Array;

[GlobalClass]
public partial class Terrain : StaticBody2D {
    public List<WorldOp> operations = [];

    private uint shapeOwnerId;
    private List<Vector2[]> polygons = [];


    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        operations.Add(new WorldOp(
            [new Vector2(0, 500), new Vector2(1150, 400), new Vector2(1150, 600), new Vector2(0, 600)],
            Geometry2D.PolyBooleanOperation.Union));
        operations.Add(new WorldOp(
            [new Vector2(200, 330), new Vector2(200, 660), new Vector2(300, 650), new Vector2(300, 330)],
            Geometry2D.PolyBooleanOperation.Union));
        shapeOwnerId = CreateShapeOwner(this);
        RecalculatePolygons();
        foreach (var polygon in polygons) {
            ConvexPolygonShape2D shape = new ConvexPolygonShape2D();
            shape.Points = polygon;
            ShapeOwnerAddShape(shapeOwnerId, shape);
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Draw() {
        foreach (var polygon in polygons) {
            DrawColoredPolygon(polygon, new Color(GD.Randf(), GD.Randf(), GD.Randf()));
        }
    }

    public override void _Input(InputEvent @event) {
        if (@event is not InputEventMouseButton { Pressed: true } eventMouseButton) return;

        var op = new WorldOp([new Vector2(25, 25), new Vector2(25, -25), new Vector2(-25, -25), new Vector2(-25, 25)],
            eventMouseButton.ButtonIndex == MouseButton.Left
                ? Geometry2D.PolyBooleanOperation.Union
                : Geometry2D.PolyBooleanOperation.Difference);
        for (var i = 0; i < op.points.Length; i++) {
            op.points[i] += eventMouseButton.Position;
        }

        operations.Add(op);
        RecalculatePolygons();
    }

    private void RecalculatePolygons() {
        List<Vector2[]> newPolygons = [[]];

        foreach (var operation in operations) {
            switch (operation.boolOperation) {
                case Geometry2D.PolyBooleanOperation.Union:
                    var added = false;
                    for (var i = 0; i < newPolygons.Count && !added; i++) {
                        var mergedPolygons = Geometry2D.MergePolygons(newPolygons[i], operation.points);
                        if (mergedPolygons.Count != 1) continue;
                        newPolygons[i] = mergedPolygons[0];
                        added = true;
                    }

                    if (!added) {
                        newPolygons.Add(operation.points);
                    }

                    break;

                case Geometry2D.PolyBooleanOperation.Difference:
                    var originalPolygonCount = newPolygons.Count;
                    for (var i = 0; i < originalPolygonCount; i++) {
                        var clippedPolygons = Geometry2D.ClipPolygons(newPolygons[i], operation.points);

                        var hasHole = false;
                        var outerIdx = -1;
                        foreach (var polygon in clippedPolygons) {
                            if (Geometry2D.IsPolygonClockwise(polygon)) {
                                hasHole = true;
                            } else {
                                outerIdx = i;
                            }
                        }

                        if (hasHole) {
                            var outer = clippedPolygons[outerIdx];

                            outer = clippedPolygons
                                .Where((t, j) => j != outerIdx)
                                .Aggregate(outer, HolepunchPolygon);

                            newPolygons[i] = outer;
                        } else {
                            newPolygons[i] = clippedPolygons[0];
                            clippedPolygons.RemoveAt(0);
                            newPolygons.AddRange(clippedPolygons);
                        }
                    }

                    break;
            }
        }

        polygons.Clear();
        foreach (var polygon in newPolygons) {
            polygons.AddRange(Geometry2D.DecomposePolygonInConvex(polygon));
        }

        QueueRedraw();
    }

    private Vector2[] HolepunchPolygon(Vector2[] outer, Vector2[] inner) {
        // We wish to join the inner and outer hull by connecting the following points
        // The highest point in the inner polygon
        int innerIntersectionIndex = 0;
        var point0 = inner[0];
        for (var j = 1; j < inner.Length; j++) {
            if (inner[j].Y > point0.Y) {
                innerIntersectionIndex = j;
                point0 = inner[j];
            }
        }

        // The point of the intersection after moving upwards from the above point
        var point1 =
            Geometry2D.IntersectPolylineWithPolygon((Vector2[])[point0, new Vector2(point0.X, 1e9f)], outer)[0][1];

        // Find where in the outer polygon to add the new line segment
        // Check every line segment to see which one point1 lies on
        var outerIntersectionIndex = -1; // Should always be set
        for (var j = 0; j < outer.Length; j++) {
            if (point1.DistanceTo(outer[j]) + point1.DistanceTo(outer[(j + 1) % outer.Length]) ==
                outer[j].DistanceTo(outer[(j + 1) % outer.Length])) {
                outerIntersectionIndex = j;
                break;
            }
        }

        // Create new polygon
        Vector2[] newPoly = new Vector2[inner.Length + outer.Length + 3];
        int destIndex = 0;
        Array.Copy(outer, 0, newPoly, destIndex, outerIntersectionIndex + 1);
        destIndex += outerIntersectionIndex + 1;
        newPoly[destIndex] = point1;
        destIndex++;
        for (var j = 0; j < inner.Length; j++) {
            newPoly[destIndex] = inner[(j + innerIntersectionIndex) % inner.Length];
            destIndex++;
        }

        newPoly[destIndex] = point0;
        destIndex++;
        newPoly[destIndex] = point1;
        destIndex++;
        Array.Copy(outer, outerIntersectionIndex + 1, newPoly, destIndex, outer.Length - outerIntersectionIndex - 1);
        return newPoly;
    }

    private void PrintVec2ArrForDesmos(Vector2[] points) {
        String str = "polygon(";
        foreach (var point in points) {
            str += $"({point.X}, {point.Y}),";
        }

        str.Remove(str.Length - 1, 1);
        str += ")";
        GD.Print(str);
    }

    public record struct WorldOp(Vector2[] points, Geometry2D.PolyBooleanOperation boolOperation);
}