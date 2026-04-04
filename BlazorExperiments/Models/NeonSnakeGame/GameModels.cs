using System.Numerics;

namespace BlazorExperiments.UI.Models.NeonSnakeGame;

public class SnakeBodyPart {
    public Vector2 GridPos;
    public Vector2 Direction;
    public Vector2 TargetScreenPos;
    public Vector2 CurrentScreenPos;
    public Vector2 StartScreenPos;
}

public struct EatParticle {
    public double X, Y, Vx, Vy, Life;
    public string Color = "";
    public EatParticle() { }
}

public struct HitParticle {
    public double X, Y, Vx, Vy, Life, Size, Rotation, RotSpeed;
    public string Color = "";
    public HitParticle() { }
}

public struct DeathSegment {
    public double X, Y, Vx, Vy, R, Rotation, RotSpeed, Alpha;
    public bool IsHead;
}

public static class Neon {
    public const string BgOuter = "#040814";
    public const string BgInner = "#0a1130";
    public const string GridBase = "#0b1330";
    public const string GridLine = "rgba(88,205,255,0.1)";
    public const string Border = "#35f4ff";
    public const string BorderGlow = "rgba(153,82,255,0.75)";
    public const string SnakePrimary = "#21f6ff";
    public const string SnakeSecondary = "#5a8bff";
    public const string SnakeGlow = "rgba(33,246,255,0.75)";
    public const string SnakeEye = "#ff5de5";
    public const string FoodPrimary = "#ff36db";
    public const string FoodSecondary = "#ff84ff";
    public const string FoodGlow = "rgba(255, 74, 235, 0.9)";
    public const string FoodStem = "#77ffd2";
    public const string ObstacleBase = "#141d3d";
    public const string ObstacleEdge = "#6f8cff";
    public const string ObstacleGlow = "rgba(99,123,255,0.6)";
    public const string HudBgTop = "#080f26";
    public const string HudBgBottom = "#0e1b3f";
    public const string HudLine = "#33f1ff";
    public const string TextPrimary = "#88f8ff";
    public const string TextAccent = "#ff63ec";
    public const string TextMuted = "#8ca0d8";
    public const string HeartOn = "#ff4de2";
    public const string HeartOff = "#2b335a";
    public const string Danger = "#ff3f72";
    public const string DangerGlow = "rgba(255,63,114,0.75)";
}
