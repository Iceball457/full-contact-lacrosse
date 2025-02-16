using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public partial class LevelSelect : Node {
    const int SCREEN_WIDTH = 512;
    const int SCREEN_HEIGHT = 288;
    List<StadiumData> stadiums = new() {
        StadiumData.REGULAR_STADIUM,
        StadiumData.CORNER_STADIUM,
        StadiumData.HOCKEY_STADIUM,
        StadiumData.FOOTBALL_STADIUM,
        StadiumData.TENNIS_STADIUM
    };
    List<ImageTexture> renders = new();


    [Export] Texture2D[] wallTex = new Texture2D[1];
    [Export] Sprite2D sprite;
    int pointer = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        // Generate a list of levels from the default set as well as all level json files in "~/.full-contact-lacrosse/custom-stadiums/"
        // Append the filename of each file in the custom stadium path to that list
        // Specify the directory path for custom stadiums
        string customStadiumsPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".full-contact-lacrosse", "custom-stadiums");

        // Get all level json files in the custom stadium path
        string[] customStadiumFiles = Directory.GetFiles(customStadiumsPath, "*.json");

        // Append the filename of each file to the list of levels
        foreach (string path in customStadiumFiles) {
            string fileContentsJson = File.ReadAllText(path);
            Debug.WriteLine(fileContentsJson);
            // Todo: catch serialization errors and add a non-selectable stage icon with a file name and warning for the player to see
            StadiumData newStadium = StadiumData.FromJson(fileContentsJson);
            Debug.WriteLine(newStadium.GetWall(1, 1));
            stadiums.Add(newStadium);
        }
        // Generate textures for each level
        foreach (StadiumData stadium in stadiums) {
            renders.Add(GenerateTexture(stadium));
        }
        sprite.Texture = renders[pointer];
    }

    private ImageTexture GenerateTexture(StadiumData stadium) {
        Image output = Image.Create(SCREEN_WIDTH, SCREEN_HEIGHT, false, Image.Format.Rgba8);
        foreach (ScoreZoneData zone in stadium.scoringZones) {
            output.FillRect(new(new(Mathf.RoundToInt(zone.location.X * 16), Mathf.RoundToInt(zone.location.Y * 16)), new(32, 32)), zone.isTeam1 ? ColorManager.GetTeamColor(0) : ColorManager.GetTeamColor(1));
        }
        for (int x = 0; x < 32; x++) {
            for (int y = 0; y < 18; y++) {
                // Draw all the walls!
                if (wallTex[(int)stadium.GetWall(x, y)] != null) {
                    Image image = wallTex[(int)stadium.GetWall(x, y)].GetImage();
                    image.Convert(Image.Format.Rgba8);
                    output.BlitRectMask(image, image, new(new(0, 0), new(16, 16)), new(x * 16, y * 16));
                }
            }
        }
        return ImageTexture.CreateFromImage(output);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("Up")) {
            pointer = Modulo(pointer - 1, stadiums.Count);
            sprite.Texture = renders[pointer];
        }
        if (Input.IsActionJustPressed("Down")) {
            pointer = Modulo(pointer + 1, stadiums.Count);
            sprite.Texture = renders[pointer];
        }
        if (Input.IsActionJustPressed("Select")) {
            // TODO: Assign the stadium to the game manager and swap to the player select screen!
            GameManager.StageChosen(stadiums[pointer]);
        }
        if (Input.IsActionJustPressed("Cancel")) {
            GameManager.SelectStageCanceled();
        }
    }
    static int Modulo(int a, int b) { return (a % b + b) % b; }
}
