using System;
using System.Collections.Generic;
using System.Linq;
using FullContactLacrosse.Input;
using FullContactLacrosse.Player.Roles;
using FullContactLacrosse.Player.States;
using Godot;

namespace FullContactLacrosse.Player;

public partial class Player : Node {
    static readonly HashSet<Player> allPlayers = [];
    [Export] AudioStreamPlayer audio;
    public Hid Controller { get; internal set; }
    [Export] public PlayerVisual Visual { get; private set; }
    [Export] public PlayerStateMachine Machine { get; private set; }
    public PlayerRole Role { get; private set; }

    float charge = 1;
    public float Charge { get { return charge; } internal set { charge = Math.Clamp(value, 1, Role.ChargeCap); } }
    public Vector2 ThrowDirection { get; internal set; } = Vector2.Up;

    public bool Stunned { get { return Machine.CurrentState.Name == "Stun"; } }
    public bool Throwing { get { return Machine.CurrentState.Name == "Strafe"; } }
    public bool Tackling { get { return Machine.CurrentState.Name == "Tackle"; } }
    public bool Holding { get { return HeldPuck != null; } }

    public Puck HeldPuck { get; internal set; }

    Vector2 direction;

    Vector2 location = new(Stadium.SCREEN_WIDTH / 2, Stadium.SCREEN_HEIGHT / 2);
    public Vector2 Location { get => location; set { location = new(Modulo(value.X, Stadium.SCREEN_WIDTH), Modulo(value.Y, Stadium.SCREEN_HEIGHT)); Visual.QueueRedraw(); } }

    public override void _EnterTree() {
        allPlayers.Add(this);
    }

    public override void _ExitTree() {
        allPlayers.Remove(this);
    }

    public override void _PhysicsProcess(double delta) {
        Intention input = Controller.GetIntention();
        Machine.Process(input, delta);
        CollideWithWalls();
    }

    public void SetRole(string role) {
        Role = GD.Load<PlayerRole>($"res://Player/Roles/{role}.tres");
        Visual.SetBodyTexture(Role.Texture);
    }

    public HashSet<Player> GetOverlappingPlayers() {
        var output = from other in allPlayers
                     where ModhattanMax(Location, other.Location) < 14 && other != this
                     select other;
        return [.. output];
    }

    public Puck IsOverlappingPuck() {
        if (ModhattanMax(Location, Puck.singleton.Location) < 8) return Puck.singleton;
        return null;
    }

    internal Action<Vector2> OnHitWall;
    private void CollideWithWalls() {
        Vector2I i = Stadium.GetCell(Location);
        //GD.Print(i);

        Vector2I[] wallIndices = [
            new(i.X + 0, i.Y + 0),
            new(i.X + 0, i.Y - 1),
            new(i.X + 0, i.Y + 1),
            new(i.X - 1, i.Y + 0),
            new(i.X + 1, i.Y + 0),
            new(i.X - 1, i.Y - 1),
            new(i.X - 1, i.Y + 1),
            new(i.X + 1, i.Y - 1),
            new(i.X + 1, i.Y + 1),
        ];
        foreach (Vector2I wallIndex in wallIndices) {
            Vector2 wallLocation = new(wallIndex.X * 16 + 8, wallIndex.Y * 16 + 8);
            int xOffset = (int)(Location.X - wallLocation.X);
            int yOffset = (int)(Location.Y - wallLocation.Y);
            if (Stadium.IsSolid(Stadium.GetWall(wallIndex))) {
                if (ModhattanMax(Location, wallLocation) < 14) {
                    if (MathF.Abs(xOffset) < MathF.Abs(yOffset)) {
                        // Adjust on Y for smallest change
                        Location = new Vector2(Location.X, wallLocation.Y + MathF.Sign(yOffset) * 14f);
                        OnHitWall?.Invoke(new(1, -1));
                    } else {
                        // Adjust on X for smallest change
                        Location = new Vector2(wallLocation.X + MathF.Sign(xOffset) * 14f, Location.Y);
                        OnHitWall?.Invoke(new(-1, 1));
                    }
                }
            }
        }
    }


    private static float ModhattanDistance(Vector2 a, Vector2 b, Vector2 l) {
        a = new(Modulo(a.X, l.X), Modulo(a.Y, l.Y));
        b = new(Modulo(b.X, l.X), Modulo(b.Y, l.Y));
        float deltaX = TriangleWave(Math.Abs(a.X - b.X), l.X);
        float deltaY = TriangleWave(Math.Abs(a.Y - b.Y), l.Y);
        return deltaX + deltaY;
    }
    private static float ModhattanMax(Vector2 a, Vector2 b) {
        a = new(Modulo(a.X, Stadium.SCREEN_WIDTH), Modulo(a.Y, Stadium.SCREEN_HEIGHT));
        b = new(Modulo(b.X, Stadium.SCREEN_WIDTH), Modulo(b.Y, Stadium.SCREEN_HEIGHT));
        float deltaX = TriangleWave(Math.Abs(a.X - b.X), Stadium.SCREEN_WIDTH);
        float deltaY = TriangleWave(Math.Abs(a.Y - b.Y), Stadium.SCREEN_HEIGHT);
        return Math.Max(deltaX, deltaY);
    }

    private static float TriangleWave(float a, float b) {
        return a <= b / 2 ? a : b - a;
    }

    private float Encumberance() {
        float carrySlow = HeldPuck == null ? 1 : 0.85f;
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
    protected static float Modulo(float a, float b) { return (a % b + b) % b; }

    internal Puck GetHit(Vector2 knockback, float duration) {
        Machine.CurrentState.MinimumDuration = 0;
        Stun stun = Machine.GetState("Stun") as Stun;
        stun.Init(knockback, duration);
        Machine.SetState("Stun");
        Puck output = HeldPuck;
        HeldPuck?.Drop(Vector2.Zero);
        return output;
    }
}