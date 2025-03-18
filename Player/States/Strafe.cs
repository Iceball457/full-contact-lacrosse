using FullContactLacrosse.Input;
using Godot;

namespace FullContactLacrosse.Player.States;

public partial class Strafe : PlayerState {
    [Export] float grabLockout;
    AudioStream pass = GD.Load<AudioStream>("res://pass.wav");

    protected override void OnEnter(Intention input, double delta) {
        Player.Charge = 1f;
        MinimumDuration = float.MaxValue;
        if (input.Direction.LengthSquared() > 0.1) Player.ThrowDirection = input.Direction.Normalized();
    }

    protected override void OnProcess(Intention input, double delta) {
        if (Interruptable) Player.Machine.SetState("Normal");
        Player.Location += input.Direction * Player.Role.TackleSpeed * (float)delta;
        Player.Charge += Player.Role.ChargeRate * (float)delta;
        if (input.GetButton("Select") == ButtonState.Up) Throw();
    }

    void Throw() {
        Player.HeldPuck.Drop(Player.ThrowDirection * Player.Role.ThrowForce * Player.Charge);
        MinimumDuration = Duration + grabLockout;
        GameManager.PlaySound(pass);
    }

    protected override void OnExit(Intention input, double delta) {
        Player.Charge = 1;
    }
}