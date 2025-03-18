using System;
using System.Collections.Generic;
using FullContactLacrosse.Input;
using Godot;

namespace FullContactLacrosse.Player;

public abstract partial class PlayerState : Node {
    public Player Player { get; set; }
    public double Duration { get; private set; } = 0;
    public double MinimumDuration { get; internal set; } = 0;
    public virtual bool Interruptable { get { return Duration >= MinimumDuration; } }

    internal void Enter(Intention input, double delta) {
        Duration = 0;
        OnEnter(input, delta);
    }
    protected virtual void OnEnter(Intention input, double delta) { }
    internal void Process(Intention input, double delta) {
        Duration += delta;
        OnProcess(input, delta);
    }
    protected virtual void OnProcess(Intention input, double delta) { }
    internal void Exit(Intention input, double delta) {
        OnExit(input, delta);
    }
    protected virtual void OnExit(Intention input, double delta) { }


}