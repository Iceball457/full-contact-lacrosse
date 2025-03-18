using System;
using System.Collections.Generic;
using System.Diagnostics;
using FullContactLacrosse.Player;
using Godot;

public partial class GameInput : Node {
    public static GameInput Singleton { get; private set; }
    Player[] playerRegistry = new Player[32];
    readonly string[] inputStrings = [
        "Select",
        "Cancel",
        "Up",
        "Down",
        "Left",
        "Right"
    ];

    public GameInput() {
        Singleton?.QueueFree();
        Singleton = this;
    }

    public void RegisterPlayer(int device, Player player) {
        playerRegistry[device] = player;
    }

    public Player[] GetPlayers() {
        return playerRegistry;
    }
}
