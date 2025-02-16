using Godot;
using System;

public partial class RootFinder : Node {
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        GameManager.Tree = GetTree();
        QueueFree();
    }
}
