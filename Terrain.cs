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
    private int staticOperationCount;

    private bool recalculateQueued;

    [Export] public Color color;

    private uint shapeOwnerId;
    private List<Vector2[]> polygons = [];


    // Called when the node enters the scene tree for the first time.
    public override void _EnterTree() {
        foreach (var child in GetChildren()) {
            if (child is TerrainPolygon terrainPolygon) {
                operations.Add(new WorldOp(terrainPolygon.Polygon, terrainPolygon.booleanOperation));
                child.QueueFree();
            } else if (child is TerrainShape terrainShape) {
                operations.Add(new WorldOp(terrainShape.getPolygon(), terrainShape.booleanOperation));
                child.QueueFree();
            }
        }

        staticOperationCount = operations.Count;
        shapeOwnerId = CreateShapeOwner(this);
        RecalculatePolygons();
        foreach (var polygon in polygons) {
            ConvexPolygonShape2D shape = new ConvexPolygonShape2D();
            shape.Points = polygon;
            ShapeOwnerAddShape(shapeOwnerId, shape);
        }
    }

    public void QueueRecalculation() => recalculateQueued = true;

    public override void _Process(double delta) {
        if (recalculateQueued) {
            recalculateQueued = false;
            RecalculatePolygons();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Draw() {
        foreach (Vector2[] polygon in polygons) {
            DrawColoredPolygon(polygon, Colors.CornflowerBlue);
        }
    }

    public override void _Input(InputEvent @event) {
        if (@event is not InputEventMouseButton { Pressed: true } eventMouseButton) return;

        var op = new WorldOp([new Vector2(25, 25), new Vector2(25, -25), new Vector2(-25, -25), new Vector2(-25, 25)],
            eventMouseButton.ButtonIndex switch {
                MouseButton.Left => Geometry2D.PolyBooleanOperation.Union,
                MouseButton.Right => Geometry2D.PolyBooleanOperation.Difference,
                _ => Geometry2D.PolyBooleanOperation.Xor
            });
        for (var i = 0; i < op.points.Length; i++) {
            op.points[i] += eventMouseButton.Position;
        }

        operations.Insert(staticOperationCount++, op);
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
                        var outerPolygonCount = mergedPolygons.Aggregate(0,
                            (acc, poly) => acc + (Geometry2D.IsPolygonClockwise(poly) ? 0 : 1));
                        if (outerPolygonCount != 1) continue;

                        mergedPolygons = MergeInnerPolygons(mergedPolygons);
                        newPolygons[i] = mergedPolygons[0];
                        added = true;
                    }

                    if (!added) {
                        newPolygons.Add(operation.points);
                    }

                    break;

                case Geometry2D.PolyBooleanOperation.Difference:
                    newPolygons = newPolygons
                        .SelectMany(x => MergeInnerPolygons(Geometry2D.ClipPolygons(x, operation.points))).ToList();
                    break;

                case Geometry2D.PolyBooleanOperation.Intersection:
                    newPolygons = newPolygons
                        .SelectMany(x => MergeInnerPolygons(Geometry2D.IntersectPolygons(x, operation.points)))
                        .ToList();
                    break;

                case Geometry2D.PolyBooleanOperation.Xor:
                    newPolygons = newPolygons
                        .SelectMany(x => MergeInnerPolygons(Geometry2D.ExcludePolygons(x, operation.points)))
                        .ToList();
                    break;
            }
        }

        polygons.Clear();
        foreach (var polygon in newPolygons) {
            polygons.AddRange(Geometry2D.DecomposePolygonInConvex(polygon));
        }

        QueueRedraw();
        ShapeOwnerClearShapes(shapeOwnerId);
        foreach (var polygon in polygons) {
            ConvexPolygonShape2D shape = new ConvexPolygonShape2D();
            shape.Points = polygon;
            ShapeOwnerAddShape(shapeOwnerId, shape);
        }
    }

    private Array<Vector2[]> MergeInnerPolygons(Array<Vector2[]> polygons) {
        var hasHole = false;
        var outerCount = 0;
        var outerIdx = -1;
        for (var i = 0; i < polygons.Count; i++) {
            if (Geometry2D.IsPolygonClockwise(polygons[i])) {
                hasHole = true;
            } else {
                outerIdx = i;
                outerCount++;
            }
        }

        if (!hasHole) return polygons;
        if (outerCount == 1) {
            var outer = polygons[outerIdx];

            outer = polygons
                .Where((t, j) => j != outerIdx)
                .Aggregate(outer, HolepunchPolygon);

            return [outer];
        } else {
            var outers = polygons
                .Where(x => !Geometry2D.IsPolygonClockwise(x))
                .Select(x => new KeyValuePair<Vector2[], Array<Vector2[]>>(x, []))
                .ToDictionary();

            foreach (var inner in polygons.Where(Geometry2D.IsPolygonClockwise)) {
                foreach (var (outer, associatedInners) in outers) {
                    if (!Geometry2D.IsPointInPolygon(inner[0], outer)) continue;
                    associatedInners.Add(inner);
                    break;
                }
            }

            Array<Vector2[]> newPolygons = [];
            foreach (var (outer, associatedInners) in outers) {
                associatedInners.Add(outer);
                newPolygons.AddRange(MergeInnerPolygons(associatedInners));
            }

            return newPolygons;
        }
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
            if (Math.Abs(point1.DistanceTo(outer[j]) + point1.DistanceTo(outer[(j + 1) % outer.Length]) -
                         outer[j].DistanceTo(outer[(j + 1) % outer.Length])) < 0.001) {
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

        str = str.Remove(str.Length - 1, 1);
        str += ")";
        GD.Print(str);
    }

    public record struct WorldOp(Vector2[] points, Geometry2D.PolyBooleanOperation boolOperation);
}