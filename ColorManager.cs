using Godot;
using System;
using System.Collections.Generic;

public static class ColorManager {

    static readonly List<Color> teamColorPalette = new() {
        new("#FFB12B"),
        new("#47C2FF"),
        new("#C70B0B"),
        new("#8745FF"),
        new("#52F94F")
};

    public static int[] TeamColors { get; set; } = new int[2] {
        0, 1
    };

    public static void ChangeTeamColor(int teamIdx, int paletteIncrement) {
        TeamColors[teamIdx] = Modulo(TeamColors[teamIdx] + paletteIncrement, teamColorPalette.Count);
        int otherTeam = teamIdx == 0 ? 1 : 0;
        if (TeamColors[teamIdx] == TeamColors[otherTeam]) {
            TeamColors[teamIdx] = Modulo(TeamColors[teamIdx] + paletteIncrement, teamColorPalette.Count);
        }
    }

    public static Color GetTeamColor(int teamIdx) {
        return teamColorPalette[TeamColors[teamIdx]];
    }

    static int Modulo(int a, int b) { return (a % b + b) % b; }

}
