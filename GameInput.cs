using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public partial class GameInput : Node {
    public static GameInput SINGLETON;
    Player[] playerRegistry = new Player[32];
    readonly string[] inputStrings = new string[] {
        "Select",
        "Cancel",
        "Up",
        "Down",
        "Left",
        "Right"
    };

    public GameInput() {
        SINGLETON?.QueueFree();
        SINGLETON = this;
    }

    public override void _Input(InputEvent @event) {
        foreach (string inputType in inputStrings) {
            //Debug.WriteLine($"{@event.Device}");
            if (@event.IsAction(inputType)) playerRegistry[@event.Device].Input(inputType, @event.GetActionStrength(inputType));
            //if (@event.IsAction(inputType)) Debug.WriteLine($"Valid Select: {@event.IsActionPressed(inputType)}");
            //Debug.WriteLine($"{inputType}: {@event.IsActionPressed(inputType)}");
        }
    }

    public void RegisterPlayer(int device, Player player) {
        playerRegistry[device] = player;
    }

    public Player[] GetPlayers() {
        return playerRegistry;
    }
}
