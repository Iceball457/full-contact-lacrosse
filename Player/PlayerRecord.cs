using FullContactLacrosse.Input;
using Godot;

namespace FullContactLacrosse.Player;

public readonly struct PlayerRecord(int deviceIdx, bool team1, string role) {
    public int DeviceIdx { get; } = deviceIdx;
    public bool Team1 { get; } = team1;
    public string Role { get; } = role;

    public Player GenerateLocalPlayerNode() {
        Player output = GD.Load<PackedScene>("res://Player/player.tscn").Instantiate<Player>();
        Controller controller = new() {
            DeviceIdx = DeviceIdx
        };
        output.Controller = controller;

        //GD.Print($"Index: {DeviceIdx}\nRole {Role}");
        output.SetRole(Role);

        output.Visual.Modulate = ColorManager.GetTeamColor(Team1 ? 0 : 1);

        output.AddChild(controller);
        return output;
    }
}