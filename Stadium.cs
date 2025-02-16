using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

public partial class Stadium : Node2D {
    [Export] Texture2D[] wallTex = new Texture2D[1];
    static StadiumData data;

    static PackedScene puckPrefab = GD.Load<PackedScene>("res://Puck.tscn");
    static PackedScene scoreZonePrefab = GD.Load<PackedScene>("res://ScoreZone.tscn");
    public override void _Ready() {
        LoadStadium(GameManager.GetStadium());
    }


    public void LoadStadium(StadiumData stadium) {
        data = stadium;
        Puck newPuck = puckPrefab.Instantiate<Puck>();
        newPuck.Location = data.puckSpawn;
        GetTree().Root.CallDeferred("add_child", new Variant[] { newPuck });
        Debug.WriteLine($"Scoring Zones in stadium: {data.scoringZones.Count}");
        foreach (ScoreZoneData zoneData in data.scoringZones) {
            ScoreZone newScoreZone = scoreZonePrefab.Instantiate<ScoreZone>();
            newScoreZone.Transform = new(0, zoneData.location * 16);
            newScoreZone.Team1 = zoneData.isTeam1;
            newScoreZone.Modulate = ColorManager.GetTeamColor(newScoreZone.Team1 ? 0 : 1);
            GetTree().Root.CallDeferred("add_child", new Variant[] { newScoreZone });
        }
        QueueRedraw();
    }

    public static StadiumData.WallType GetWall(int x, int y) {
        return data.GetWall(x, y);
    }
    public static StadiumData.WallType GetWall(Vector2I location) {
        return data.GetWall(location.X, location.Y);
    }
    public static Vector2 GetSpawn(bool team1, int indexOnTeam, int playersOnTeam) {
        int spawnIndex = 0;
        if (!team1) spawnIndex += 3;
        if (playersOnTeam != 1) {
            spawnIndex += (1 + indexOnTeam) % 3;
        }
        return data.playerSpawns[spawnIndex];
    }
    public static Vector2I GetCell(Vector2 location) {
        return new(Mathf.RoundToInt((location.X - 8) / 16), Mathf.RoundToInt((location.Y - 8) / 16));
    }

    public override void _Draw() {
        for (int x = 0; x < 32; x++) {
            for (int y = 0; y < 18; y++) {
                // Draw all the walls!
                if (wallTex[(int)data.GetWall(x, y)] != null)
                    DrawTextureRect(wallTex[(int)data.GetWall(x, y)], new(new(x * 16, y * 16), new(16, 16)), false);
            }
        }
    }

}
