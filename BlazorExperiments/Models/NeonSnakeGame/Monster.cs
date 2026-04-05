namespace BlazorExperiments.UI.Models.NeonSnakeGame;

public class Monster {
    public int X, Y;
    public double ScreenX, ScreenY;
    public double StartScreenX, StartScreenY;
    public double TargetScreenX, TargetScreenY;
    public double MoveAccum;
    public bool Awake;
}
