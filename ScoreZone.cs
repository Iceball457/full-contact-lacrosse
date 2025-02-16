using Godot;
using System;
using System.Collections.Generic;

public partial class ScoreZone : Node2D {
    [Export] public bool Team1 { get; set; }
    [Export] Slider[] sliders = new Slider[1];

    public override void _Ready() {
        GameManager.RegisterZone(this);
        Modulate = ColorManager.GetTeamColor(Team1 ? 0 : 1);
    }

    public void SetPercent(float percent) {
        foreach (Slider slider in sliders) {
            slider.SetPercent(percent);
        }
    }
}
