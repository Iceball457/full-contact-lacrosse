using Godot;

namespace FullContactLacrosse.Input;

public abstract partial class Hid : Node {
    public abstract Intention GetIntention();
}