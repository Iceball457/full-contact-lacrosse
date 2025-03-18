using FullContactLacrosse.Input;
using Godot;

namespace FullContactLacrosse.Player.States;

public partial class Stun : PlayerState {
    AudioStream hitSound = GD.Load<AudioStream>("res://tackle.wav");
    Vector2 velocity;
    bool shaky;
    internal void Init(Vector2 velocity, float duration) {
        this.velocity = velocity;
        MinimumDuration = duration;
    }

    protected override void OnEnter(Intention input, double delta) {
        GameManager.PlaySound(hitSound);
    }

    protected override void OnProcess(Intention input, double delta) {
        if (Interruptable) Player.Machine.SetState("Normal");
        Player.Location += velocity;
        Player.Location += new Vector2(shaky ? 2 : -2, 0);
        shaky = !shaky;
    }
}