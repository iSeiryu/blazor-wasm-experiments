using System.Numerics;

namespace BlazorExperiments.UI.Models.SnakeGame;

public class BodyPart {
    public BodyPart(float x, float y, float prevX, float prevY) {
        Position = new(x, y);
        PrevPosition = new(prevX, prevY);
        AnimationPosition = new(0, 0);
    }

    public Vector2 Position;
    public Vector2 PrevPosition { get; }
    public Vector2 AnimationPosition { get; private set; }
    public double Interpolation { get; private set; }

    // New target position arrived, reset interpolation
    public void ResetInterp() {
        Interpolation = 0.0f;
    }

    // Interpolate between current position towards target position
    public void Interpolate(double deltaTime, float gameSpeed) {
        Interpolation += deltaTime * gameSpeed;
        Interpolation = Math.Min(1.0f, Interpolation); // clamp max value at 1.0

        AnimationPosition = Vector2.Lerp(PrevPosition, Position, (float)Interpolation);
    }
}