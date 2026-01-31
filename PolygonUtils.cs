using System;
using Godot;

public static class PolygonUtils {
    public static Vector2[] CreateCenteredRect(Vector2 size) {
        return [
            new (-size.X / 2, -size.Y / 2),
            new (-size.X / 2, size.Y / 2),
            new (size.X / 2, size.Y / 2),
            new (size.X / 2, -size.Y / 2)
        ];
    }

    public static Vector2[] CreateNGon(float radius, int sides) {
        var points = new Vector2[sides];
        for (var i = 0; i < sides; i++) {
            var point = new Vector2(radius, 0);
            point = point.Rotated(-2 * Mathf.Pi * i / sides);
            points[i] = point;
        }

        return points;
    }
}