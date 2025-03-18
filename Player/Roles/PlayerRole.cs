using Godot;

namespace FullContactLacrosse.Player.Roles;

[GlobalClass]
public partial class PlayerRole : Resource {
    [Export] public float MoveSpeed { get; private set; } = 120f;
    [Export] public float ThrowForce { get; private set; } = 200f;
    [Export] public float ChargeRate { get; private set; } = 1 / 2f;
    [Export] public float ChargeCap { get; private set; } = 2f;
    [Export] public float TackleSpeed { get; private set; } = 150f;
    [Export] public float TackleDuration { get; private set; } = .75f;
    [Export] public float StunDuration { get; private set; } = .8f;
    [Export] public Texture2D Texture { get; private set; }
}