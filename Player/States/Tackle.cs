using FullContactLacrosse.Input;
using FullContactLacrosse.Player;
using Godot;

namespace FullContactLacrosse.Player.States;

public partial class Tackle : PlayerState {
    AudioStream dashSound = GD.Load<AudioStream>("res://dash.wav");

    Vector2 velocity;
    protected override void OnEnter(Intention input, double delta) {
        velocity = input.Direction.Normalized() * Player.Role.TackleSpeed * (float)delta;
        MinimumDuration = Player.Role.TackleDuration;
        Player.OnHitWall += WallBounce;
    }

    protected override void OnProcess(Intention input, double delta) {
        if (Interruptable) Player.Machine.SetState("Normal");
        Player.Location += velocity;
        Puck puck = Player.IsOverlappingPuck();
        if (puck != null) {
            if (puck.Holder == null) puck.PickUp(Player);
        }
        foreach (Player other in Player.GetOverlappingPlayers()) {
            if (other.Stunned) continue;
            other.GetHit(velocity * 0.5f, Player.Role.StunDuration);
            if (other.Tackling) {
                Tackle tackle = other.Machine.GetState("Tackle") as Tackle;
                Player.GetHit(tackle.velocity * 0.5f, other.Role.StunDuration);
            }
        }
    }

    protected override void OnExit(Intention input, double delta) {
        Player.OnHitWall -= WallBounce;
    }

    void WallBounce(Vector2 multiply) {
        Player.GetHit(velocity * multiply * 0.5f, Player.Role.StunDuration);
    }

    // if (Tackling <= 0) forcedMovement = new();
    //Tackle(player);
    //                    if (player.Tackling <= 0) {
    //                        if (inputs["Cancel"] > 0) { // Tackleboost
    //                            forcedMovement *= 1.5f;
    //                            //Tackling += tackleDuration * 0.5f;
    //                        }
    //                    } else {
    //player.HitTackle(this, true);
    //}
}