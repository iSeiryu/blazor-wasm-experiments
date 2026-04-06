using System.Numerics;

namespace BlazorExperiments.UI.Models.NeonSnakeGame;

public class Egg {
    public Vector2 GridPos;
    public double Timer = 5000;
    public bool Running;
    public Vector2 RunDir = new(1, 0);
    public double RunAccum;
    public Vector2 ScreenPos;
    public Vector2 StartScreenPos;
    public Vector2 TargetScreenPos;
}

