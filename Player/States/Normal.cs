using FullContactLacrosse.Input;
using Godot;

namespace FullContactLacrosse.Player.States;

public partial class Normal : PlayerState {
    protected override void OnProcess(Intention input, double delta) {
        Player.Location += input.Direction * Player.Role.MoveSpeed * (float)delta;
        if (input.Direction.LengthSquared() > 0.1) Player.ThrowDirection = input.Direction.Normalized();
        Puck puck = Player.IsOverlappingPuck();
        if (puck != null) {
            if (puck.Holder == null) puck.PickUp(Player);
            if (input.GetButton("Select") == ButtonState.Down) Player.Machine.SetState("Strafe");
        }
        if (!Player.Holding && input.GetButton("Cancel") == ButtonState.Down && input.Direction.Length() > 0.15) Player.Machine.SetState("Tackle");
    }
}