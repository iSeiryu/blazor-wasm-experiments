using System.Numerics;

namespace BlazorExperiments.UI.Models.SnakeGame;

public class BodyPart(float x, float y, Vector2 direction) {
    public Vector2 Position = new(x, y);
    public Vector2 PrevPosition = new(x, y);
    public Vector2 AnimationPosition { get; private set; } = new(0, 0);
    public double Rotation { get; set; }
    public double Interpolation { get; set; }
    public Vector2 Direction { get; set; } = direction;

    public void ResetInterp() {
        Interpolation = 0.0f;
    }

    public void Animate(double deltaTime, float gameSpeed) {
        Interpolation += deltaTime * gameSpeed / 1_000;
        Interpolation = Math.Min(1.0f, Interpolation); // clamp max value at 1.0

        AnimationPosition = Vector2.Lerp(PrevPosition, Position, (float)Interpolation);
    }

    static double ShortAngleDist(double a0, double a1) {
        var max = Math.PI * 2;
        var da = (a1 - a0) % max;
        return ((2 * da) % max) - da;
    }

    // https://gist.github.com/shaunlebron/8832585
    public double AngleLerp(double a0, double a1, double t) {
        return a0 + ShortAngleDist(a0, a1) * t;
    }
}