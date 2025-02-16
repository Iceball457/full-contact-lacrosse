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
        "Right",
        "Select2",
        "Cancel2",
        "Up2",
        "Down2",
        "Left2",
        "Right2"
    };

    public GameInput() {
        SINGLETON?.QueueFree();
        SINGLETON = this;
    }

    public override void _Input(InputEvent @event) {
        foreach (string inputType in inputStrings) {
            Debug.WriteLine($"{@event.Device}");
            if (@event.IsAction(inputType)) playerRegistry[GetPlayerIndex(@event.Device, inputType.Contains("2"))].Input(inputType, @event.GetActionStrength(inputType));
            //if (@event.IsAction(inputType)) Debug.WriteLine($"Valid Select: {@event.IsActionPressed(inputType)}");
            //Debug.WriteLine($"{inputType}: {@event.IsActionPressed(inputType)}");
        }
    }
    private int GetPlayerIndex(int device, bool sharedPlayer) {
        return device*2 + (sharedPlayer?1: 0);
    }
    public void RegisterPlayer(int device, bool sharedPlayer, Player player) {
        playerRegistry[GetPlayerIndex(device, sharedPlayer)] = player;
    }

    public Player[] GetPlayers() {
        return playerRegistry;
    }
}
