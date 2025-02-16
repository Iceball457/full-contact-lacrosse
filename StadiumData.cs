using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[Serializable]
public class StadiumData {
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WallType {
        [EnumMember(Value = "None")]
        None,

        [EnumMember(Value = "Wall")]
        Wall,

        [EnumMember(Value = "Mesh")]
        Mesh,

        [EnumMember(Value = "Gate")]
        Gate,

        [EnumMember(Value = "Slow")]
        Slow,

        [EnumMember(Value = "Fast")]
        Fast
    }
    [JsonProperty]
    WallType[,] walls = new WallType[32, 18];
    public WallType GetWall(int x, int y) {
        return walls[Modulo(x, 32), Modulo(y, 18)];
    }
    public List<ScoreZoneData> scoringZones = new() {
        new(new(25, 7), true),
        new(new(25, 9), true),
        new(new(5, 7), false),
        new(new(5, 9), false),
    };
    public StadiumData() { }
    public StadiumData(List<ScoreZoneData> scoreZones, Vector2I puckSpawn, Vector2I[] playerSpawns) {
        this.scoringZones = scoreZones;
        this.puckSpawn = puckSpawn;
        if (playerSpawns.Length > 6) throw new Exception("Invalid player spawn count!");
        this.playerSpawns = playerSpawns;
    }
    public Vector2I puckSpawn = new(256, 144);
    public Vector2I[] playerSpawns = new Vector2I[6] {
        new(208, 144), new(224, 112), new(192, 176),
        new(304, 144), new(288, 176), new(320, 112)
    };

    public string ToJson() {
        return JsonFormat(JsonConvert.SerializeObject(this));
    }
    public static StadiumData FromJson(string json) {
        return JsonConvert.DeserializeObject<StadiumData>(json);
    }
    public static StadiumData REGULAR_STADIUM {
        get => GenerateRegularStadium();
    }
    private static StadiumData GenerateRegularStadium() {
        StadiumData output = new();
        for (int x = 0; x < 32; x++) {
            for (int y = 0; y < 18; y++) {
                if (x == 0 || y == 0 || x == 31 || y == 17) {
                    output.walls[x, y] = WallType.Wall;
                } else if (x == 1 || y == 1 || x == 30 || y == 16) {
                    output.walls[x, y] = WallType.Slow;
                } else if (x == 3 || y == 3 || x == 28 || y == 14) {
                    //regular.walls[x, y] = WallType.Boost;
                }
            }
        }
        return output;
    }
    public static StadiumData CORNER_STADIUM {
        get => GenerateCornerStadium();
    }
    private static StadiumData GenerateCornerStadium() {
        StadiumData output = new();
        for (int x = 0; x < 32; x++) {
            for (int y = 0; y < 18; y++) {
                if (x == 0 || y == 0 || x == 31 || y == 17) {
                    output.walls[x, y] = WallType.Wall;
                }
            }
        }
        output.scoringZones = new() {
            new(new(1, 1), true),
            new(new(1, 15), false),
            new(new(29, 1), false),
            new(new(29, 15), true),
        };
        return output;
    }
    public static StadiumData HOCKEY_STADIUM {
        get => GenerateHockeyStadium();
    }
    private static StadiumData GenerateHockeyStadium() {
        StadiumData output = new();
        for (int x = 0; x < 32; x++) {
            for (int y = 0; y < 18; y++) {
                if (x == 0 || y == 0 || x == 31 || y == 17) {
                    output.walls[x, y] = WallType.Wall;
                }
            }
        }
        for (int y = 7; y <= 10; y++) {
            output.walls[4, y] = WallType.Wall;
            output.walls[5, y] = WallType.Slow;
            output.walls[27, y] = WallType.Wall;
            output.walls[26, y] = WallType.Slow;
        }
        for (int x = 4; x <= 6; x++) {
            output.walls[x, 6] = WallType.Wall;
            output.walls[x, 11] = WallType.Wall;
        }
        for (int x = 25; x <= 27; x++) {
            output.walls[x, 6] = WallType.Wall;
            output.walls[x, 11] = WallType.Wall;
        }
        return output;
    }
    public static StadiumData FOOTBALL_STADIUM {
        get => GenerateFootballStadium();
    }
    private static StadiumData GenerateFootballStadium() {
        StadiumData output = new();
        for (int y = 0; y < 18; y++) {
            output.walls[0, y] = WallType.Wall;
            output.walls[31, y] = WallType.Wall;
        }
        output.scoringZones = new List<ScoreZoneData> {
            new(new(1, 0), false),
            new(new(1, 2), false),
            new(new(1, 4), false),
            new(new(1, 6), false),
            new(new(1, 8), false),
            new(new(1, 10), false),
            new(new(1, 12), false),
            new(new(1, 14), false),
            new(new(1, 16), false),
            new(new(29, 0), true),
            new(new(29, 2), true),
            new(new(29, 4), true),
            new(new(29, 6), true),
            new(new(29, 8), true),
            new(new(29, 10), true),
            new(new(29, 12), true),
            new(new(29, 14), true),
            new(new(29, 16), true)
        };
        return output;
    }
    public static StadiumData TENNIS_STADIUM {
        get => GenerateTennisStadium();
    }
    private static StadiumData GenerateTennisStadium() {
        StadiumData output = new();
        for (int x = 0; x < 32; x++) {
            for (int y = 0; y < 18; y++) {
                if (x == 0 || y == 0 || x == 31 || y == 17) {
                    output.walls[x, y] = WallType.Wall;
                }
            }
        }
        for (int y = 1; y < 17; y++) {
            output.walls[15, y] = WallType.Mesh;
            output.walls[16, y] = WallType.Mesh;
        }
        output.puckSpawn = new(128, 144);
        output.scoringZones = new List<ScoreZoneData> {
            new(new(1, 1), false),
            new(new(1, 3), false),
            new(new(1, 5), false),
            new(new(1, 7), false),
            new(new(1, 9), false),
            new(new(1, 11), false),
            new(new(1, 13), false),
            new(new(1, 15), false),
            new(new(3, 1), false),
            new(new(3, 3), false),
            new(new(3, 5), false),
            new(new(3, 7), false),
            new(new(3, 9), false),
            new(new(3, 11), false),
            new(new(3, 13), false),
            new(new(3, 15), false),
            new(new(13, 1), false),
            new(new(13, 3), false),
            new(new(13, 5), false),
            new(new(13, 7), false),
            new(new(13, 9), false),
            new(new(13, 11), false),
            new(new(13, 13), false),
            new(new(13, 15), false),
            new(new(17, 1), true),
            new(new(17, 3), true),
            new(new(17, 5), true),
            new(new(17, 7), true),
            new(new(17, 9), true),
            new(new(17, 11), true),
            new(new(17, 13), true),
            new(new(17, 15), true),
            new(new(27, 1), true),
            new(new(27, 3), true),
            new(new(27, 5), true),
            new(new(27, 7), true),
            new(new(27, 9), true),
            new(new(27, 11), true),
            new(new(27, 13), true),
            new(new(27, 15), true),
            new(new(29, 1), true),
            new(new(29, 3), true),
            new(new(29, 5), true),
            new(new(29, 7), true),
            new(new(29, 9), true),
            new(new(29, 11), true),
            new(new(29, 13), true),
            new(new(29, 15), true),
        };
        return output;
    }

    static string JsonFormat(string json) {
        string pattern = @"(:\[|],|\},|\})(?![^{}]*\})(\{)*";
        string replacement = "$1\n$2";

        Regex regex = new(pattern);
        string formattedJson = regex.Replace(json, replacement);

        return formattedJson;
    }
    public static void ExportJsonTemplate() {
        string jsonData = REGULAR_STADIUM.ToJson();
        string homePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        string customPath = Path.Combine(homePath, ".full-contact-lacrosse", "custom-stadiums");
        // Create the custom stadiums directory if it doesn't exist
        if (!Directory.Exists(customPath)) {
            Directory.CreateDirectory(customPath);
        }

        // Create a FileStream to write the JSON data to a file in the custom stadiums directory
        string filePath = Path.Combine(customPath, "custom-stadium-template.json");
        File.WriteAllText(filePath, string.Empty);
        File.WriteAllText(filePath, jsonData, Encoding.UTF8);
    }
    static int Modulo(int a, int b) { return (a % b + b) % b; }

}



[Serializable]
public record ScoreZoneData {
    public readonly Vector2I location;
    public readonly bool isTeam1;

    public ScoreZoneData(Vector2I location, bool isTeam1) {
        this.location = location;
        this.isTeam1 = isTeam1;
    }
}