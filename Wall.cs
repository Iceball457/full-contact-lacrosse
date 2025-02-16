using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

public partial class Wall : Node2D {
    static Dictionary<Vector2I, Wall> wallRegistry = new();
    const int SCREEN_WIDTH = 512;
    const int SCREEN_HEIGHT = 288;

    public Wall() {
    }

    public override void _Ready() {
        //Debug.WriteLine($"{Name}: ({Transform.Origin.X}, {Transform.Origin.Y})");
        Transform = new(0, (Transform.Origin / 16f).Round() * 16f);
        //Debug.WriteLine($"{Name}: ({Transform.Origin.X}, {Transform.Origin.Y})");
        Vector2I location = (Vector2I)Transform.Origin / 16;
        if (!wallRegistry.ContainsKey(location)) {
            wallRegistry[location] = this;
        } else {
            QueueFree();
        }
    }

    public static List<Wall> GetWalls(Vector2I location, int radius, bool verbose = false) {
        List<Wall> output = new();
        for (int x = -radius; x <= radius; x++) {
            for (int y = -radius; y <= radius; y++) {
                Vector2I searchSpot = new(Modulo(location.X + x, SCREEN_WIDTH / 16), Modulo(location.Y + y, SCREEN_HEIGHT / 16));
                if (verbose) Debug.WriteLine($"Searching: ({searchSpot.X}, {searchSpot.Y})");
                if (wallRegistry.ContainsKey(searchSpot)) {
                    output.Add(wallRegistry[searchSpot]);
                }
            }
        }
        if (verbose) Debug.WriteLine($"Found {output.Count} walls.");
        return output;
    }

    static int Modulo(int a, int b) { return (a % b + b) % b; }
}
