using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

public partial class Puck : Node2D {

    public float puckDropTimer = 3f;
    public static Puck singleton;
    const int SCREEN_WIDTH = 512;
    const int SCREEN_HEIGHT = 288;
    private const float FAST_DRAG = 1.5f;
    private const float SLOW_DRAG = 2.5f;
    AudioStream bounce = GD.Load<AudioStream>("res://bounce.wav");
    AudioStream catchPuck = GD.Load<AudioStream>("res://catch.wav");


    [Export] int trailLength;

    public Player Holder { get; private set; }

    public Vector2 location = new Vector2(SCREEN_WIDTH / 2, SCREEN_HEIGHT / 2);

    public Vector2 Location { get => GetLocation(); set => SetLocation(value); }

    private Vector2 GetLocation() {
        return location;
    }

    private void SetLocation(Vector2 location) {
        QueueRedraw();
        this.location = location;
    }

    public Vector2 Velocity { get; set; }
    Vector2 toShift;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        singleton?.QueueFree();
        singleton = this;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        Vector2I cell = Stadium.GetCell(location);

        if (puckDropTimer > 0) {
            puckDropTimer -= (float)delta;
            QueueRedraw();
            return;
        }

        Location += Velocity * (float)delta;
        if (Stadium.GetWall(cell) != StadiumData.WallType.Mesh) {
            Velocity -= Velocity * (float)delta * (Velocity.Length() > 40f ? FAST_DRAG : SLOW_DRAG);
        }
        if (Holder == null) {
            if (Location.X < 0) {
                Location += Vector2.Right * SCREEN_WIDTH;

            } else if (Location.X >= SCREEN_WIDTH) {
                Location += Vector2.Left * SCREEN_WIDTH;
            }
            if (Location.Y < 0) {
                Location += Vector2.Down * SCREEN_HEIGHT;
            } else if (Location.Y >= SCREEN_HEIGHT) {
                Location += Vector2.Up * SCREEN_HEIGHT;
            }
        }
        //Debug.WriteLine($"Puck: ({subPixel.X}, {subPixel.Y})");
        //Debug.WriteLine($"Puck: ({trail.Points[0].X}, {trail.Points[0].Y})");

        // Wall Collision
        foreach (Vector2I offset in new List<Vector2I>() { new(0, 0), new(1, 0), new(0, 1), new(-1, 0), new(0, -1) }) {
            Vector2I wallCell = new(Modulo(cell.X + offset.X, 32), Modulo(cell.Y + offset.Y, 18)); // cell + new Vector2I(x, y);
            switch (Stadium.GetWall(wallCell)) {
                case StadiumData.WallType.Wall:
                case StadiumData.WallType.Gate:
                    CollideWithWall(wallCell);
                    break;
            }
        }

        // Player Collision
        if (Holder == null) {
            foreach (Player player in GameInput.SINGLETON.GetPlayers()) {
                if (player == null) continue;
                for (int x = -SCREEN_WIDTH; x <= SCREEN_WIDTH; x += SCREEN_WIDTH) {
                    for (int y = -SCREEN_HEIGHT; y <= SCREEN_HEIGHT; y += SCREEN_HEIGHT) {
                        int xOffset = (int)(player.Location.X - Location.X + x);
                        int yOffset = (int)(player.Location.Y - Location.Y + y);
                        //Debug.WriteLine($"{nearbyWalls.Count}).");
                        if (MathF.Abs(xOffset) < 8 && MathF.Abs(yOffset) < 8 && player.GrabLockout <= 0 && player.Stunned <= 0) {
                            GameManager.PlaySound(catchPuck);
                            PickUp(player);
                        }
                    }
                }
            }
        }

        // Score Zone
        foreach (ScoreZone zone in GameManager.GetZones()) {
            int xOffset = (int)(zone.Transform.Origin.X - Location.X + 18);
            int yOffset = (int)(zone.Transform.Origin.Y - Location.Y + 18);
            //Debug.WriteLine($"{nearbyWalls.Count}).");
            if (MathF.Abs(xOffset) <= 16 && MathF.Abs(yOffset) <= 16) {
                GameManager.Score(zone.Team1, (float)delta);
            }
        }


        if (Holder != null) {
            Velocity = Vector2.Zero;
            Location = Holder.Location;
        } else {
            if (Velocity.Length() < 8f) Velocity = new();
        }
    }

    private void CollideWithWall(Vector2I wallCell) {
        Vector2 wallLocation = new(wallCell.X * 16 + 8, wallCell.Y * 16 + 8);
        int xOffset = (int)(Location.X - wallLocation.X);
        int yOffset = (int)(Location.Y - wallLocation.Y);
        if (MathF.Abs(xOffset) < 10 && MathF.Abs(yOffset) < 10) {
            GameManager.PlaySound(bounce);
            if (MathF.Abs(xOffset) < MathF.Abs(yOffset)) {
                // Adjust on Y for smallest change
                Location = new Vector2(Location.X, wallLocation.Y + MathF.Sign(yOffset) * 10f);
                Velocity = new(Velocity.X, -Velocity.Y);
            } else {
                // Adjust on X for smallest change
                Location = new Vector2(wallLocation.X + MathF.Sign(xOffset) * 10f, location.Y);
                Velocity = new(-Velocity.X, Velocity.Y);
            }
        }
    }

    public void PickUp(Player player) {
        if (player.GrabLockout > 0 || player.Stunned > 0) return;
        if (Holder != null) Holder.Holding = null;
        player.Holding = this;
        Holder = player;
    }
    public void Drop(Vector2 velocity) {
        Holder.Holding = null;
        Holder = null;
        Velocity = velocity;
    }

    public void ShiftTrail(Vector2 amount) {
        toShift = amount;
        //for (int i = 0; i < trailLength; i++) {
        //    trail.SetPointPosition(i, trail.GetPointPosition(i) + amount);
        //}
    }

    public static IEnumerable<Vector2> CalculateDisplacement(Vector2 initialPosition, Vector2 initialVelocity, float delta) {
        Vector2 landingPosition = initialPosition;
        Vector2 velocity = initialVelocity;

        while (velocity.Length() > 8) {
            Vector2I cell = Stadium.GetCell(landingPosition);

            yield return landingPosition += velocity * (float)delta;
            if (Stadium.GetWall(cell) != StadiumData.WallType.Mesh) {
                velocity -= velocity * (float)delta * (velocity.Length() > 40f ? FAST_DRAG : SLOW_DRAG);
            }
            if (landingPosition.X < 0) {
                landingPosition += Vector2.Right * SCREEN_WIDTH;

            } else if (landingPosition.X >= SCREEN_WIDTH) {
                landingPosition += Vector2.Left * SCREEN_WIDTH;
            }
            if (landingPosition.Y < 0) {
                landingPosition += Vector2.Down * SCREEN_HEIGHT;
            } else if (landingPosition.Y >= SCREEN_HEIGHT) {
                landingPosition += Vector2.Up * SCREEN_HEIGHT;
            }
            //Debug.WriteLine($"Puck: ({subPixel.X}, {subPixel.Y})");
            //Debug.WriteLine($"Puck: ({trail.Points[0].X}, {trail.Points[0].Y})");

            // Wall Collision
            foreach (Vector2I offset in new List<Vector2I>() { new(0, 0), new(1, 0), new(0, 1), new(-1, 0), new(0, -1) }) {
                Vector2I wallCell = new(Modulo(cell.X + offset.X, 32), Modulo(cell.Y + offset.Y, 18)); // cell + new Vector2I(x, y);
                switch (Stadium.GetWall(wallCell)) {
                    case StadiumData.WallType.Wall:
                    case StadiumData.WallType.Gate:
                        SimulateCollideWithWall(wallCell);
                        break;
                }
            }
        }

        yield return landingPosition;

        void SimulateCollideWithWall(Vector2I wallCell) {
            Vector2 wallLocation = new(wallCell.X * 16 + 8, wallCell.Y * 16 + 8);
            int xOffset = (int)(landingPosition.X - wallLocation.X);
            int yOffset = (int)(landingPosition.Y - wallLocation.Y);
            if (MathF.Abs(xOffset) < 10 && MathF.Abs(yOffset) < 10) {
                if (MathF.Abs(xOffset) < MathF.Abs(yOffset)) {
                    // Adjust on Y for smallest change
                    landingPosition = new Vector2(landingPosition.X, wallLocation.Y + MathF.Sign(yOffset) * 10f);
                    velocity = new(velocity.X, -velocity.Y);
                } else {
                    // Adjust on X for smallest change
                    landingPosition = new Vector2(wallLocation.X + MathF.Sign(xOffset) * 10f, landingPosition.Y);
                    velocity = new(-velocity.X, velocity.Y);
                }
            }
        }
    }

    public override void _Draw() {
        if (puckDropTimer > 0) {
            int width = Mathf.RoundToInt(puckDropTimer * 10 + 6);
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < width; y++) {
                    DrawRect(new(new(Modulo(Mathf.RoundToInt(Location.X) + x - width / 2, SCREEN_WIDTH), Modulo(Mathf.RoundToInt(Location.Y) + y - width / 2, SCREEN_HEIGHT)), new(1, 1)), new(0, 0, 0, 0.5f - puckDropTimer / 6f));
                }
            }
        } else {
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 4; y++) {
                    DrawRect(new(new(Modulo(Mathf.RoundToInt(Location.X) + x - 2, SCREEN_WIDTH), Modulo(Mathf.RoundToInt(Location.Y) + y - 2, SCREEN_HEIGHT)), new(1, 1)), new(0, 0, 0));
                }
            }
        }
    }

    static int Modulo(int a, int b) { return (a % b + b) % b; }

}
