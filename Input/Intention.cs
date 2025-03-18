using System.Collections.Generic;
using Godot;

namespace FullContactLacrosse.Input;

public readonly struct Intention(Vector2 direction, Dictionary<string, ButtonState> buttons) {
    public Vector2 Direction { get; } = direction;
    readonly Dictionary<string, ButtonState> buttons = buttons;

    public ButtonState GetButton(string button) {
        return buttons[button];
    }
}

public enum ButtonState {
    None,
    Down,
    Held,
    Up,
}