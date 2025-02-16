using Godot;
using System;

public partial class PlayOneShot : AudioStreamPlayer {
    double lifeSpan;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        lifeSpan = Stream.GetLength();
        Play();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        lifeSpan -= delta;
        if (lifeSpan <= 0) QueueFree();
    }
}
