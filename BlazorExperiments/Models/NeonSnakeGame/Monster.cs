using System.Numerics;

namespace BlazorExperiments.UI.Models.NeonSnakeGame;

public class Monster {
    public Vector2 GridPos;
    public Vector2 ScreenPos;
    public Vector2 StartScreenPos;
    public Vector2 TargetScreenPos;
    public double MoveAccum;
    public bool Awake;
}
