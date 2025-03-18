using System.Collections.Generic;
using FullContactLacrosse.Input;
using Godot;

namespace FullContactLacrosse.Player;

public partial class PlayerStateMachine : Node {
    readonly Dictionary<string, PlayerState> states = [];
    [Export] public PlayerState CurrentState { get; private set; }
    PlayerState queuedState;

    public override void _EnterTree() {
        Player parent = GetParent<Player>();
        foreach (Node child in GetChildren()) {
            if (child is not PlayerState state) continue;
            state.Player = parent;
            states.Add(state.Name, state);
        }
    }

    public void Process(Intention input, double delta) {
        GoToQueuedState(input, delta);
        CurrentState.Process(input, delta);
    }

    public void SetState(string stateName) {
        queuedState = states[stateName];
    }
    public PlayerState GetState(string stateName) {
        return states[stateName];
    }
    private void GoToQueuedState(Intention input, double delta) {
        if (queuedState == null) return;
        if (CurrentState != null) {
            if (!CurrentState.Interruptable) return;
            CurrentState.Exit(input, delta);
        }
        CurrentState = queuedState;
        CurrentState?.Enter(input, delta);
        queuedState = null;
    }
}