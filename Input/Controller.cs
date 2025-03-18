using System.Collections.Generic;
using Godot;

namespace FullContactLacrosse.Input;

public partial class Controller : Hid {
    [Export] public int DeviceIdx { get; set; }
    string[] actionNames = ["Select", "Cancel", "Up", "Down", "Left", "Right"];
    float[] actionStrengths = new float[6];
    float[] actionTracers = new float[6];
    float[] actionDeltas = new float[6];
    public override Intention GetIntention() {
        Dictionary<string, ButtonState> buttons = [];
        for (int i = 0; i < 6; i++) {
            actionDeltas[i] = actionStrengths[i] - actionTracers[i];
            buttons.Add(
                actionNames[i],
                actionStrengths[i] != 0 ? (
                    actionTracers[i] != 0 ? ButtonState.Held : ButtonState.Down
                ) : (
                    actionTracers[i] != 0 ? ButtonState.Up : ButtonState.None
                )
            );
        }
        var output = new Intention(
            new(
                actionStrengths[5] - actionStrengths[4],
                actionStrengths[3] - actionStrengths[2]
            ),
            buttons
        );
        actionTracers = actionStrengths.Clone() as float[];
        return output;
    }
    public override void _Input(InputEvent e) {
        if (e.Device != DeviceIdx) return;
        //if (e.IsAction("Select")) //GD.Print($"{GetParent().Name} reports input from {e.Device}");
        for (int i = 0; i < 6; i++) if (e.IsAction(actionNames[i])) actionStrengths[i] = e.GetActionStrength(actionNames[i]);
    }
}