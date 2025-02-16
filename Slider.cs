using Godot;
using System;

public partial class Slider : Node2D {

    float percent;

    public void SetPercent(float percent) {
        this.percent = Mathf.Clamp(percent, 0, 1);
        QueueRedraw();
    }

    public override void _Draw() {
        int distance = Mathf.RoundToInt(percent * 32);
        Vector2[] pointsNW = new Vector2[3] {
            new( 0,  0), new( 1 + distance,  0), new( 0,  1 + distance)
        };
        DrawColoredPolygon(pointsNW, new(0.7f, 0.7f, 0.7f));
        Vector2[] pointsNE = new Vector2[3] {
            new(32,  0), new(32,  0 + distance), new(32 - distance, 0)
        };
        DrawColoredPolygon(pointsNE, new(0.7f, 0.7f, 0.7f));
        Vector2[] pointsSE = new Vector2[3] {
            new(32, 32), new(32 - distance, 32), new(32, 32 - distance)
        };
        DrawColoredPolygon(pointsSE, new(0.7f, 0.7f, 0.7f));
        Vector2[] pointsSW = new Vector2[3] {
            new( 0, 32), new( 0, 31 - distance), new( 1 + distance, 32)
        };
        DrawColoredPolygon(pointsSW, new(0.7f, 0.7f, 0.7f));
    }
}
