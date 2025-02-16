using System;
using System.Collections.Generic;
using Godot;

public partial class LocalControllerSetup : Node2D {
    // Called when the node enters the scene tree for the first time.
    // TODO: Bundle the index of the device with whether or not it's shared "struct(int, bool)"
    List<int> knownControllerIndices = new();
    [Export] ControllerIndicator[] indicators;
    public override void _Ready() {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Input(InputEvent @event) {
        if (@event.IsAction("Select")) {
            float select = @event.GetActionStrength("Select");
            if (select > 0.6f) {
                if (!knownControllerIndices.Contains(@event.Device)) {
                    knownControllerIndices.Add(@event.Device);
                }
            }
        }
        for (int i = 0; i < knownControllerIndices.Count; i++) {
            indicators[i].deviceIdx = knownControllerIndices[i];
        }
    }
}
