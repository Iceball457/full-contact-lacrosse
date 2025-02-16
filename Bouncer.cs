using Godot;
using System;

public partial class Bouncer : Node2D {
    [Export] Vector2 intensity;
    [Export] float speed;
    float totalTime;
    [Export] Node2D child;


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        totalTime += (float)delta * speed;
        if (totalTime > Mathf.Tau) totalTime -= Mathf.Tau;
        child.Transform = new(0, intensity * Mathf.Abs(Mathf.Sin(totalTime)));
    }
}
