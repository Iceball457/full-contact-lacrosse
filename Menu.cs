using System;
using Godot;

public partial class Menu : Node2D {
    int pointer = 0;
    [Export] float pointerOffset;
    [Export] float itemHeight;
    [Export] Node2D pointerDisplay;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        GameManager.ResetPlayerRegistry();
        GameManager.ClearZones();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("Up")) {
            pointer = Modulo(pointer - 1, 4);
        }
        if (Input.IsActionJustPressed("Down")) {
            pointer = Modulo(pointer + 1, 4);
        }
        pointerDisplay.Transform = new(0, new(0, pointer * itemHeight + pointerOffset));
        if (Input.IsActionJustPressed("Select")) {
            // TODO: Assign the stadium to the game manager and swap to the player select screen!
            switch (pointer) {
                case 0:
                    GameManager.SelectStage();
                    break;
                case 2:
                    StadiumData.ExportJsonTemplate();
                    break;
                case 3:
                    GetTree().Quit();
                    break;
            }
        }

    }
    static int Modulo(int a, int b) { return (a % b + b) % b; }
}
