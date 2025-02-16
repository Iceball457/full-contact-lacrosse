using System;
using System.Diagnostics;
using Godot;

public partial class ControllerIndicator : Node2D {
    [Export] public int deviceIdx;
    [Export] Sprite2D number;
    [Export] Node2D offset;
    [Export] Node2D roleSelector;
    [Export] Node2D readyBanner;

    int roleSelection;
    [Export] Node2D[] roleSelectionGraphics;
    [Export] Node2D[] bouncers;

    PlayerRecord registeredPlayer;

    bool rightDown, leftDown;
    bool oldRight, oldLeft;
    bool upDown, downDown;
    bool oldUp, oldDown;
    bool selectDown, cancelDown;
    bool oldSelect, oldCancel;
    bool showTeamColor;

    enum ReadyState {
        NotPlaying,
        SelectingTeam,
        SelectingRole,
        Ready
    }
    ReadyState state = ReadyState.NotPlaying;

    int team;
    public override void _Ready() {
        AdjustRole(0);
    }
    public override void _Input(InputEvent @event) {
        number.Frame = Mathf.PosMod(deviceIdx, 4);
        if (@event.Device == deviceIdx) {
            if (@event.IsAction("Right")) {
                float right = @event.GetActionStrength("Right");
                if (right > 0.6f) {
                    rightDown = true;
                } else if (right < 0.4f) {
                    rightDown = false;
                }

            }
            if (@event.IsAction("Left")) {
                float left = @event.GetActionStrength("Left");
                if (left > 0.6f) {
                    leftDown = true;
                } else if (left < 0.4f) {
                    leftDown = false;
                }
            }
            if (@event.IsAction("Up")) {
                float up = @event.GetActionStrength("Up");
                if (up > 0.6f) {
                    upDown = true;
                } else if (up < 0.4f) {
                    upDown = false;
                }

            }
            if (@event.IsAction("Down")) {
                float down = @event.GetActionStrength("Down");
                if (down > 0.6f) {
                    downDown = true;
                } else if (down < 0.4f) {
                    downDown = false;
                }
            }
            if (@event.IsAction("Select")) {
                float select = @event.GetActionStrength("Select");
                if (select > 0.6f) {
                    selectDown = true;
                } else if (select < 0.4f) {
                    selectDown = false;
                }
            }
            if (@event.IsAction("Cancel")) {
                float cancel = @event.GetActionStrength("Cancel");
                if (cancel > 0.6f) {
                    cancelDown = true;
                } else if (cancel < 0.4f) {
                    cancelDown = false;
                }
            }
        }
    }

    public override void _Process(double delta) {
        bool rightDelta = rightDown && !oldRight;
        bool leftDelta = leftDown && !oldLeft;
        bool upDelta = upDown && !oldUp;
        bool downDelta = downDown && !oldDown;
        bool selectDelta = selectDown && !oldSelect;
        bool cancelDelta = cancelDown && !oldCancel;
        oldRight = rightDown;
        oldLeft = leftDown;
        oldUp = upDown;
        oldDown = downDown;
        oldSelect = selectDown;
        oldCancel = cancelDown;
        if (state == ReadyState.NotPlaying && cancelDelta) {
            GameManager.PlayerSelectCanceled();
        }
        if (state == ReadyState.SelectingTeam && rightDelta) {
            // Move Right
            team = Mathf.Clamp(team + 1, -1, 1);
        } else if (state == ReadyState.SelectingTeam && leftDelta) {
            // Move Left
            team = Mathf.Clamp(team - 1, -1, 1);
        }
        if (state == ReadyState.SelectingRole && rightDelta) {
            // Move Right
            AdjustRole(1);
        } else if (state == ReadyState.SelectingRole && leftDelta) {
            // Move Left
            AdjustRole(-1);
        }
        if (state == ReadyState.SelectingRole && upDelta) {
            ColorManager.ChangeTeamColor((team == 1) ? 1 : 0, -1);
        } else if (state == ReadyState.SelectingRole && downDelta) {
            ColorManager.ChangeTeamColor((team == 1) ? 1 : 0, 1);
        }

        if (selectDelta) {
            switch (state) {
                case ReadyState.NotPlaying:
                    JoinGame();
                    break;
                case ReadyState.SelectingTeam:
                    SelectTeam();
                    break;
                case ReadyState.SelectingRole:
                    SelectRole();
                    break;
            }
        } else if (cancelDelta) {
            switch (state) {
                case ReadyState.SelectingTeam:
                    LeaveGame();
                    break;
                case ReadyState.SelectingRole:
                    CancelTeam();
                    break;
                case ReadyState.Ready:
                    CancelRole();
                    break;
            }
        }
        offset.Transform = new(0, new(80 * team, 0));
        roleSelector.Transform = new(0, new(160 * team, 0));
        if (showTeamColor) {
            // Locate the team color
            Color teamColor = ColorManager.GetTeamColor((team == 1) ? 1 : 0);
            // Set the everything to that color
            number.Modulate = teamColor;
            roleSelector.Modulate = teamColor;
            readyBanner.Modulate = teamColor;
        }
    }

    void JoinGame() {
        Visible = true;

        GameManager.ActiveControllers++;

        state = ReadyState.SelectingTeam;
    }

    void LeaveGame() {
        Visible = false;

        GameManager.ActiveControllers--;

        state = ReadyState.NotPlaying;
    }

    void SelectTeam() {
        if (team == 0) return;
        // Show the player's team colors!
        showTeamColor = true;
        // Show the UI for the next step.
        roleSelector.Visible = true;

        state = ReadyState.SelectingRole;
    }

    void CancelTeam() {
        // Return icon to default color
        showTeamColor = false;
        number.Modulate = new(1, 1, 1);
        // Hide UI for next step.
        roleSelector.Visible = false;

        state = ReadyState.SelectingTeam;
    }

    void SelectRole() {
        // Mark player as ready! Banner in bg?
        readyBanner.Visible = true;
        // Hide the UI for the previous step.
        foreach (Node2D node in bouncers) {
            node.Visible = false;
        }
        //roleSelector.Visible = false;

        registeredPlayer = new(deviceIdx, team == -1, (PlayerRecord.Role)roleSelection);
        GameManager.RegisterPlayer(registeredPlayer);

        state = ReadyState.Ready;

        GameManager.CheckReady();
    }
    void CancelRole() {
        // Mark player as not ready.
        readyBanner.Visible = false;
        // Show the UI for the previous step.
        foreach (Node2D node in bouncers) {
            node.Visible = true;
        }
        //roleSelector.Visible = true;

        GameManager.RemovePlayer(registeredPlayer);
        registeredPlayer = null;

        state = ReadyState.SelectingRole;
    }
    void AdjustRole(int change) {
        roleSelection = Modulo(roleSelection + change, 3);
        for (int i = 0; i < 3; i++) {
            roleSelectionGraphics[i].Visible = i == roleSelection;
        }
    }

    static int Modulo(int a, int b) { return (a % b + b) % b; }
}
