using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Godot;

public partial class Player : Node2D {
    readonly Random rand = new();
    [Export] Texture2D bodyTexture;
    [Export] AudioStreamPlayer audio;
    AudioStream dash = GD.Load<AudioStream>("res://dash.wav");
    AudioStream pass = GD.Load<AudioStream>("res://pass.wav");
    AudioStream tackle = GD.Load<AudioStream>("res://tackle.wav");

    private void SetBodyTexture(Texture2D value) {
        bodyTexture = value;
        img = bodyTexture.GetImage();
    }

    Image img;
    [Export] public int DeviceIdx { get; set; }
    float moveSpeed = 120f;
    float throwForce = 200f;
    float chargeRate = 1 / 2f;
    float chargeCap = 2f;
    float tackleSpeed = 150f;

    float tackleDuration = .75f;
    float stunDuration = .8f;

    float charge;
    public float GrabLockout { get; set; }
    public float Tackling { get; set; }
    public float TackleLockout { get; set; }
    public float Stunned { get; set; }
    float shakeTimer;
    bool stunShake;
    public Vector2 forcedMovement;
    const int SCREEN_WIDTH = 512;
    const int SCREEN_HEIGHT = 288;

    public Puck Holding { get; set; }

    Dictionary<string, float> inputs = new() {
        {"Select",0},
        {"Cancel",0},
        {"Up",0},
        {"Down",0},
        {"Left",0},
        {"Right",0}
    };
    Vector2 throwDirection = Vector2.Up;
    Dictionary<string, float> inputOld = new() {
        {"Select",0},
        {"Cancel",0},
        {"Up",0},
        {"Down",0},
        {"Left",0},
        {"Right",0}
    };
    Dictionary<string, float> inputDelta = new() {
        {"Select",0},
        {"Cancel",0},
        {"Up",0},
        {"Down",0},
        {"Left",0},
        {"Right",0}
    };

    readonly string[] inputStrings = new string[] {
        "Select",
        "Cancel",
        "Up",
        "Down",
        "Left",
        "Right"
    };

    Vector2 direction;

    Vector2 location = new(SCREEN_WIDTH / 2, SCREEN_HEIGHT / 2);
    public Vector2 Location { get => GetLocation(); set => SetLocation(value); }

    private Vector2 GetLocation() {
        return location;
    }

    private void SetLocation(Vector2 location) {
        QueueRedraw();
        this.location = location;
    }

    public override void _Ready() {
        MatchSetup(DeviceIdx);
        img = bodyTexture.GetImage();
    }
    public void MatchSetup(int deviceForMatch) {
        GameInput.SINGLETON.RegisterPlayer(deviceForMatch, this);
    }

    public void SetRole(PlayerRecord.Role role) {
        switch (role) {
            case PlayerRecord.Role.Thrower:
                throwForce = 200;
                chargeRate = 5 / 8f;
                chargeCap = 3f;
                stunDuration = 0.55f;
                tackleDuration = 0.5f;
                SetBodyTexture(GD.Load<Texture2D>("res://thrower.png"));
                break;
            case PlayerRecord.Role.Runner:
                moveSpeed = 125f;
                tackleSpeed = 155f;
                SetBodyTexture(GD.Load<Texture2D>("res://runner.png"));
                break;
            case PlayerRecord.Role.Tackler:
                moveSpeed = 115f;
                tackleSpeed = 145f;
                stunDuration = 1f;
                throwForce = 180f;
                SetBodyTexture(GD.Load<Texture2D>("res://tackler.png"));
                break;
        }
    }

    public void Input(string inputType, float inputValue) {
        inputs[inputType] = inputValue;
        direction = new Vector2(
            inputs["Right"] - inputs["Left"],
            inputs["Down"] - inputs["Up"]
        );
        if (direction.LengthSquared() > 1) direction = direction.Normalized();
        if (direction.Length() > 0.5f) throwDirection = direction.Normalized();
    }

    public override void _Process(double delta) {
        // Input
        foreach (string inputType in inputStrings) {
            inputDelta[inputType] = inputs[inputType] - inputOld[inputType];
            inputOld[inputType] = inputs[inputType];
        }

        // Cooldowns
        if (GrabLockout > 0) {
            GrabLockout -= (float)delta;
        } else {
            GrabLockout = 0;
        }
        if (TackleLockout > 0) {
            TackleLockout -= (float)delta;
        } else {
            TackleLockout = 0;
        }
        if (Stunned > GrabLockout) {
            GrabLockout = Stunned;
        }

        if (Stunned > 0 && Holding != null) {
            //Debug.WriteLine("Hey!");
            Holding.Drop(new Vector2(rand.Next(), rand.Next()).Normalized());
        }

        if (Tackling <= 0 && Stunned <= 0) {
            // Movement
            Location += direction * moveSpeed * (float)delta * Encumberance();
        } else {
            Location += forcedMovement * (float)delta * Encumberance();
            Tackling -= (float)delta;
            Stunned -= (float)delta;
        }
        if (Location.X < -8) {
            Location += Vector2.Right * SCREEN_WIDTH;
            Holding?.ShiftTrail(Vector2.Right * SCREEN_WIDTH);
        } else if (Location.X >= SCREEN_WIDTH - 8) {
            Location += Vector2.Left * SCREEN_WIDTH;
            Holding?.ShiftTrail(Vector2.Left * SCREEN_WIDTH);
        }
        if (Location.Y < -8) {
            Location += Vector2.Down * SCREEN_HEIGHT;
            Holding?.ShiftTrail(Vector2.Down * SCREEN_HEIGHT);
        } else if (Location.Y >= SCREEN_HEIGHT - 8) {
            Location += Vector2.Up * SCREEN_HEIGHT;
            Holding?.ShiftTrail(Vector2.Up * SCREEN_HEIGHT);
        }

        //Render
        if (Stunned > 0) {
            shakeTimer -= (float)delta;
            if (shakeTimer < 0) {
                //Debug.WriteLine("In");
                if (stunShake) {
                    Location += Vector2.Right * 2;
                } else {
                    Location += Vector2.Left * 2;
                }
                stunShake = !stunShake;
                shakeTimer = 1 / 20f;

            }
        }

        // Wall Collision
        Vector2I cell = Stadium.GetCell(location);
        //Debug.WriteLine($"{Stadium.ActiveStadium.GetWall(cell)}");
        foreach (Vector2I offset in new List<Vector2I>() { new(0, 0), new(0, -1), new(1, 0), new(0, 1), new(-1, 0), new(1, -1), new(1, 1), new(-1, 1), new(-1, -1) }) {
            Vector2I wallCell = new(Modulo(cell.X + offset.X, 32), Modulo(cell.Y + offset.Y, 18)); // cell + new Vector2I(x, y);
            switch (Stadium.GetWall(wallCell)) {
                case StadiumData.WallType.Wall:
                case StadiumData.WallType.Mesh:
                    CollideWithWall(wallCell);
                    break;
            }
        }

        // Player Collision
        foreach (Player player in GameInput.SINGLETON.GetPlayers()) {
            if (player == null || player == this) continue;
            for (int x = -SCREEN_WIDTH; x <= SCREEN_WIDTH; x += SCREEN_WIDTH) {
                for (int y = -SCREEN_HEIGHT; y <= SCREEN_HEIGHT; y += SCREEN_HEIGHT) {
                    int xOffset = (int)(player.Location.X - Location.X + x);
                    int yOffset = (int)(player.Location.Y - Location.Y + y);
                    //Debug.WriteLine($"{nearbyWalls.Count}).");
                    if (MathF.Abs(xOffset) < 14 && MathF.Abs(yOffset) < 14 && Tackling > 0 && player.Stunned <= 0) {
                        if (Tackling <= 0) forcedMovement = new();
                        Tackle(player);
                        if (player.Tackling <= 0) {
                            if (inputs["Cancel"] > 0) { // Tackleboost
                                forcedMovement *= 1.5f;
                                //Tackling += tackleDuration * 0.5f;
                            }
                        } else {
                            player.Tackle(this, true);
                        }
                    }
                }
            }
        }

        if (Holding != null) GrabLockout = 0.5f;


        // Debug
        if (inputDelta["Select"] > 0) {
            //Wall.GetWalls((Vector2I)(Transform.Origin / 16f).Round(), 1, true);
        }
        if (inputDelta["Select"] > 0) {
            charge = 1f;
        } else if (inputDelta["Select"] < 0) {
            // Throw the puck!
            if (Holding != null) {
                Vector2 throwVelocity = throwDirection * throwForce * charge;
                Holding.Drop(throwVelocity);
                GameManager.PlaySound(pass);
                //Debug.WriteLine($"Throw @ {charge}. Distance @ 60fps: {Puck.CalculateDisplacement(throwVelocity.Length(), 1f / 60)}, Distance @ 240fps: {Puck.CalculateDisplacement(throwVelocity.Length(), 1f / 240)}");
            }
        }
        if (inputs["Select"] > 0 && Holding != null) {
            charge += (float)delta * chargeRate;
            charge = MathF.Min(charge, chargeCap);
            //Debug.WriteLine($"Charge: {charge}");
        } else {
            charge = 1f;
        }
        if (inputDelta["Cancel"] > 0 && TackleLockout <= 0 && Stunned <= 0 && Holding == null) {
            // Tackle
            if (direction.Length() > 0.5f) {
                GameManager.PlaySound(dash);
                forcedMovement = direction.Normalized() * tackleSpeed;
                Tackling = tackleDuration;
                TackleLockout = 1.1f;
            }
        }
    }

    private void CollideWithWall(Vector2I wallCell) {
        Vector2 wallLocation = new(wallCell.X * 16 + 8, wallCell.Y * 16 + 8);
        int xOffset = (int)(Location.X - wallLocation.X);
        int yOffset = (int)(Location.Y - wallLocation.Y);
        if (MathF.Abs(xOffset) < 14 && MathF.Abs(yOffset) < 14) {
            Tackling = 0;
            if (MathF.Abs(xOffset) < MathF.Abs(yOffset)) {
                // Adjust on Y for smallest change
                Location = new Vector2(Location.X, wallLocation.Y + MathF.Sign(yOffset) * 14f);
            } else {
                // Adjust on X for smallest change
                Location = new Vector2(wallLocation.X + MathF.Sign(xOffset) * 14f, location.Y);
            }
        }
    }

    private float Encumberance() {
        float carrySlow = Holding == null ? 1 : 0.85f;
        float terrainSlow = 1;
        switch (Stadium.GetWall(Stadium.GetCell(location))) {
            case StadiumData.WallType.Slow:
                terrainSlow = 0.6f;
                break;
            case StadiumData.WallType.Fast:
                terrainSlow = 1.4f;
                break;
        }
        return carrySlow * terrainSlow;
    }

    void Tackle(Player player, bool invert = false, float multiplier = 1f) {
        player.Tackling = 0;
        GameManager.PlaySound(tackle);
        player.Stunned = stunDuration * multiplier;
        player.forcedMovement = forcedMovement.Normalized() * tackleSpeed * 0.4f * (invert ? -1 : 1);
        if (Puck.singleton != null) {
            if (Puck.singleton.Holder == player) Puck.singleton.PickUp(this);
        }
    }


    public override void _Draw() {
        if (Holding != null) {
            Vector2 throwVelocity = throwDirection * throwForce * charge;
            Vector2[] points = Puck.CalculateDisplacement(location, throwVelocity, 1f / (Mathf.Pi * 100)).ToArray();
            DrawPolyline(points, new(0, 0, 0, 0.05f), 2);
            DrawRect(new(new Vector2(points[^1].X - 2, points[^1].Y - 2), new(4, 4)), new(0, 0, 0, 0.25f));
            //Debug.WriteLine($"");
        }
        for (int x = 0; x < 16; x++) {
            for (int y = 0; y < 16; y++) {
                DrawRect(new(new(Modulo(Mathf.RoundToInt(Location.X) + x - 8, SCREEN_WIDTH), Modulo(Mathf.RoundToInt(Location.Y) + y - 8, SCREEN_HEIGHT)), new(1, 1)), img.GetPixel(x, y));
            }
        }
        //DrawRect(new(Stadium.GetCell(location) * 16, 16, 16), new(1, 0, 0));
    }
    static int Modulo(int a, int b) { return (a % b + b) % b; }
}