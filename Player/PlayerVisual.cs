using System.Linq;
using Godot;

namespace FullContactLacrosse.Player;

public partial class PlayerVisual : Node2D {
    [Export] Player parent;
    Image img;
    public override void _EnterTree() {
        parent = GetParent<Player>();
    }
    public override void _Draw() {
        if (parent.HeldPuck != null) {
            Vector2 puckVelocity = parent.ThrowDirection * parent.Role.ThrowForce * parent.Charge;
            Vector2[] points = [.. Puck.CalculateDisplacement(parent.Location, puckVelocity, 1f / (Mathf.Pi * 100))];
            //GD.Print($"Aim: {puckVelocity} Path: {points}");
            DrawPolyline(points, new(0, 0, 0, 0.05f), 2);
            DrawRect(new(new Vector2(points[^1].X - 2, points[^1].Y - 2), new(4, 4)), new(0, 0, 0, 0.25f));
            //Debug.WriteLine($"");
        }
        for (int x = 0; x < 16; x++) {
            for (int y = 0; y < 16; y++) {
                DrawRect(
                    new(
                        new(
                            Modulo(Mathf.RoundToInt(parent.Location.X) + x - 8, Stadium.SCREEN_WIDTH),
                            Modulo(Mathf.RoundToInt(parent.Location.Y) + y - 8, Stadium.SCREEN_HEIGHT)
                        ),
                        new(1, 1)
                    ),
                    img.GetPixel(x, y)
                );
            }
        }


        //DrawRect(new(Stadium.GetCell(location) * 16, 16, 16), new(1, 0, 0));
    }
    public void SetBodyTexture(Texture2D value) {
        img = value.GetImage();
    }

    static int Modulo(int a, int b) { return (a % b + b) % b; }
}