namespace BlazorExperiments.UI.Models.NeonSnakeGame;

public class Egg {
    public int X, Y;
    public double Timer = 5000;
    public bool Running;
    public Vec2 RunDir = new(1, 0);
    public double RunAccum;
    public double ScreenX, ScreenY;
    public double StartScreenX, StartScreenY;
    public double TargetScreenX, TargetScreenY;
}

