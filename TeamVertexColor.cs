using Godot;
using System;

public partial class TeamVertexColor : Node2D {
    readonly Vector2[] POINTS = new Vector2[4] {
        new(128, 0),
        new(288, 0),
        new(384, 288),
        new(224, 288)
    };

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        QueueRedraw();
    }

    public override void _Draw() {
        Color[] colors = new Color[4] {
            ColorManager.GetTeamColor(0),
            ColorManager.GetTeamColor(1),
            ColorManager.GetTeamColor(1),
            ColorManager.GetTeamColor(0)
        };
        DrawPolygon(POINTS, colors);
    }
}
