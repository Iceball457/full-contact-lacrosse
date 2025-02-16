using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Godot;

public static class GameManager {
    public static SceneTree Tree { get; set; }
    static StadiumData stadiumToPlay;
    public static StadiumData GetStadium() {
        return stadiumToPlay;
    }
    const float SCORE_TO_WIN = 15f;

    static float team1Score, team2Score;
    static List<PlayerRecord> playerRegistry = new();

    static int PlayersOnTeam(bool team1) {
        int output = 0;
        foreach (PlayerRecord playerRecord in playerRegistry) {
            if (playerRecord.team1 == team1) output++;
        }
        return output;
    }
    public static int ActiveControllers { get; set; }

    public static void RegisterPlayer(PlayerRecord player) {
        playerRegistry.Add(player);
    }
    public static void RemovePlayer(PlayerRecord player) {
        playerRegistry.Remove(player);
    }
    public static void ResetPlayerRegistry() {
        playerRegistry.Clear();
        ActiveControllers = 0;
    }
    static readonly PackedScene PLAYER_PREFAB = GD.Load<PackedScene>("res://Player.tscn");
    static readonly PackedScene ONE_SHOT_AUDIO = GD.Load<PackedScene>("res://PlayOneShot.tscn");

    static readonly PackedScene MAIN_MENU = GD.Load<PackedScene>("res://Menu.tscn");
    static readonly PackedScene LEVEL_SELECT = GD.Load<PackedScene>("res://LevelSelect.tscn");
    static readonly PackedScene PLAYER_SELECT = GD.Load<PackedScene>("res://LocalControllerSetup.tscn");
    static readonly PackedScene STADIUM = GD.Load<PackedScene>("res://Stadium.tscn");
    static List<ScoreZone> scoreZones = new();

    public static void RegisterZone(ScoreZone zone) {
        scoreZones.Add(zone);
    }
    public static void ClearZones() {
        scoreZones = new();
    }

    public static List<ScoreZone> GetZones() {
        return scoreZones;
    }

    public static void SelectStage() {
        LoadScene(LEVEL_SELECT);
    }
    public static void SelectStageCanceled() {
        LoadScene(MAIN_MENU);
    }

    public static void CheckReady() {
        int team1Players = 0;
        int team2Players = 0;
        foreach (PlayerRecord player in playerRegistry) {
            if (player.team1) team1Players++;
            else team2Players++;
        }
        GD.Print($"Registered: {playerRegistry.Count}\nActive Controllers: {ActiveControllers}\nTeam 1: {team1Players}\nTeam 2: {team2Players}\n");
        if (playerRegistry.Count == ActiveControllers && team1Players > 0 && team2Players > 0) {
            // Start the game!
            StartGame();
        }
    }

    public static void StageChosen(StadiumData stadiumToPlay) {
        GameManager.stadiumToPlay = stadiumToPlay;
        ResetPlayerRegistry();
        LoadScene(PLAYER_SELECT);
    }

    public static void PlayerSelectCanceled() {
        ResetPlayerRegistry();
        LoadScene(LEVEL_SELECT);
    }

    public static void StartGame() {
        // Load the selected level!
        LoadScene(STADIUM);
        // Spawn players!
        int team1Count = 0;
        int team2Count = 0;
        foreach (PlayerRecord playerRecord in playerRegistry) {
            Player newPlayer = PLAYER_PREFAB.Instantiate<Player>();
            newPlayer.DeviceIdx = playerRecord.deviceIdx;
            newPlayer.Location = Stadium.GetSpawn(playerRecord.team1, playerRecord.team1 ? team1Count++ : team2Count++, PlayersOnTeam(playerRecord.team1));
            GD.Print($"Index: {playerRecord.deviceIdx}\nRole {playerRecord.role}");
            newPlayer.SetRole(playerRecord.role);
            newPlayer.Modulate = ColorManager.GetTeamColor(playerRecord.team1 ? 0 : 1);
            Tree.Root.AddChild(newPlayer);
        }
        // Countdown?
    }

    private static void LoadScene(PackedScene newSceneRoot) {
        if (Tree == null) throw new Exception("Add a Root Finder to any scene! GameManager Needs the scene tree!");
        foreach (Node node in Tree.Root.GetChildren()) {
            node.QueueFree();
        }
        Tree.Root.AddChild(newSceneRoot.Instantiate());
    }

    public static void Score(bool team1, float score) {
        if (team1) {
            team1Score += score;
        } else {
            team2Score += score;

        }
        foreach (ScoreZone zone in scoreZones) {
            if (zone.Team1) {
                zone.SetPercent(team1Score / SCORE_TO_WIN);
            } else {
                zone.SetPercent(team2Score / SCORE_TO_WIN);
            }
        }
        if (team1Score >= SCORE_TO_WIN) {
            // Team 1 wins!
            EndGame(true);
        }
        if (team2Score >= SCORE_TO_WIN) {
            // Team 2 wins!
            EndGame(false);
        }
    }

    public static void PlaySound(AudioStream sound) {
        PlayOneShot player = ONE_SHOT_AUDIO.Instantiate<PlayOneShot>();
        player.Stream = sound;
        Tree.Root.AddChild(player);
    }

    static void EndGame(bool team1Wins) {
        LoadScene(MAIN_MENU);
    }
}
