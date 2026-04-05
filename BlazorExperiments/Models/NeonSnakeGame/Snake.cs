using System.Numerics;

namespace BlazorExperiments.UI.Models.NeonSnakeGame;

public class Snake {
    public const int HudH = 50;
    public int CellSize = 1;
    public readonly int Cols;
    public readonly int Rows;
    public int WorldW;
    public int WorldH;
    public const double DeathAnimDuration = 1500;

    public readonly Vector2[] CenterCoords;

    public List<SnakeBodyPart> Parts = [];
    public SnakeBodyPart Head => Parts[0];
    public int Score;
    public List<Egg> Food = [];
    public float StepInterval = 300;
    public float StepAccumulator;
    public List<EatParticle> EatParticles = [];
    public double EatBounce;
    public bool Dead;
    public int Health = 3;
    public double CamX, CamY;
    public List<Vector2> Obstacles = [];
    public int BestScore;
    public double HitFlash;
    public List<HitParticle> HitParticles = [];
    public double ScreenShake;
    public double DeathTimer;
    public List<DeathSegment> DeathSegments = [];
    public bool ShowDeathScreen;
    public Queue<int> KeyQueue = new();

    static readonly string[] EatColors = ["#ffe080", "#ffb840", "#fff4c0", "#ff9420", Neon.TextAccent];
    static readonly string[] DeathColors = [Neon.Danger, "#ff6a9d", Neon.TextAccent, "#8b5dff", "#ffc2ff"];
    static readonly string[] RockColors = ["#3d4a7f", "#4f63a1", "#32406e", "#6b7fd1", "#5868a8"];
    static readonly Vector2[] CardinalDirs = [new(1, 0), new(-1, 0), new(0, 1), new(0, -1)];

    public Snake(int cellSize, int visibleCols, int visibleRows) {
        CellSize = cellSize;
        Cols = visibleCols * 5;
        Rows = visibleRows * 5;
        WorldW = Cols * CellSize;
        WorldH = Rows * CellSize;
        CenterCoords = new Vector2[Cols * Rows];
        float centerOffset = CellSize * 0.5f;
        for (int y = 0; y < Rows; y++)
            for (int x = 0; x < Cols; x++)
                CenterCoords[y * Cols + x] = new Vector2(x * CellSize + centerOffset, y * CellSize + centerOffset);

        int startCol = Cols / 2, startRow = Rows / 2;
        for (int i = 0; i < 3; i++) {
            int idx = GetIndex(startCol - i, startRow);
            Parts.Add(new SnakeBodyPart {
                GridPos = new Vector2(startCol - i, startRow),
                Direction = new Vector2(1, 0),
                TargetScreenPos = CenterCoords[idx],
                CurrentScreenPos = CenterCoords[idx],
                StartScreenPos = CenterCoords[idx]
            });
        }

        int numObstacles = (int)(Cols * Rows * 0.02);
        for (int i = 0; i < numObstacles; i++) SpawnObstacle();
        for (int i = 0; i < 50; i++) SpawnFood();
    }

    public int GetIndex(int col, int row) {
        if (col < 0 || col >= Cols || row < 0 || row >= Rows) return -1;
        return row * Cols + col;
    }

    bool IsOccupied(int px, int py) {
        foreach (var part in Parts)
            if ((int)part.GridPos.X == px && (int)part.GridPos.Y == py) return true;
        foreach (var f in Food)
            if (f.X == px && f.Y == py) return true;
        foreach (var o in Obstacles)
            if ((int)o.X == px && (int)o.Y == py) return true;
        return false;
    }

    void SpawnObstacle() {
        int px, py;
        do { px = Random.Shared.Next(Cols); py = Random.Shared.Next(Rows); }
        while (IsOccupied(px, py));
        Obstacles.Add(new Vector2(px, py));
    }

    void SpawnFood() {
        int px, py;
        do { px = Random.Shared.Next(Cols); py = Random.Shared.Next(Rows); }
        while (IsOccupied(px, py));
        int idx = GetIndex(px, py);
        var cc = CenterCoords[idx];
        Food.Add(new Egg {
            X = px,
            Y = py,
            ScreenX = cc.X,
            ScreenY = cc.Y,
            StartScreenX = cc.X,
            StartScreenY = cc.Y,
            TargetScreenX = cc.X,
            TargetScreenY = cc.Y
        });
    }

    Vector2 RandomRunDir() => CardinalDirs[Random.Shared.Next(CardinalDirs.Length)];

    static int DirectionIndex(Vector2 dir) {
        if (dir.X == 1 && dir.Y == 0) return 0;
        if (dir.X == -1 && dir.Y == 0) return 1;
        if (dir.X == 0 && dir.Y == 1) return 2;
        if (dir.X == 0 && dir.Y == -1) return 3;
        return -1;
    }

    bool EggBlocked(int x, int y, Egg skipEgg) {
        if (x < 0 || x >= Cols || y < 0 || y >= Rows) return true;
        foreach (var o in Obstacles)
            if ((int)o.X == x && (int)o.Y == y) return true;
        foreach (var f in Food)
            if (f != skipEgg && f.X == x && f.Y == y) return true;
        return false;
    }

    void StepEgg(Egg egg) {
        Span<int> order = stackalloc int[4] { 0, 1, 2, 3 };
        for (int i = order.Length - 1; i > 0; i--) {
            int j = Random.Shared.Next(i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        if (Random.Shared.NextDouble() < 0.72) {
            int preferred = DirectionIndex(egg.RunDir);
            if (preferred >= 0) {
                for (int i = 0; i < order.Length; i++) {
                    if (order[i] != preferred) continue;
                    (order[0], order[i]) = (order[i], order[0]);
                    break;
                }
            }
        }

        foreach (int idx in order) {
            var d = CardinalDirs[idx];
            int nx = egg.X + (int)d.X, ny = egg.Y + (int)d.Y;
            if (!EggBlocked(nx, ny, egg)) { egg.X = nx; egg.Y = ny; egg.RunDir = d; return; }
        }
    }

    public void Die() {
        Dead = true;
        DeathTimer = 0;
        ShowDeathScreen = false;
        ScreenShake = 1.0;
        if (Score > BestScore) BestScore = Score;
        DeathSegments.Clear();
        double baseR = CellSize * 0.44, tailR = baseR * 0.35;
        int len = Math.Max(1, Parts.Count - 1);
        for (int i = 0; i < Parts.Count; i++) {
            var part = Parts[i];
            double angle = Random.Shared.NextDouble() * Math.PI * 2;
            double speed = 2 + Random.Shared.NextDouble() * 4;
            DeathSegments.Add(new DeathSegment {
                X = part.CurrentScreenPos.X,
                Y = part.CurrentScreenPos.Y,
                Vx = Math.Cos(angle) * speed,
                Vy = Math.Sin(angle) * speed,
                R = baseR - (baseR - tailR) * (i / (double)len),
                RotSpeed = (Random.Shared.NextDouble() - 0.5) * 0.15,
                Alpha = 1.0,
                IsHead = i == 0
            });
        }
        double hx = Head.CurrentScreenPos.X, hy = Head.CurrentScreenPos.Y;
        for (int p = 0; p < 16; p++) {
            double angle = (p / 16.0) * Math.PI * 2 + Random.Shared.NextDouble() * 0.3;
            double speed = 2 + Random.Shared.NextDouble() * 3;
            EatParticles.Add(new EatParticle {
                X = hx,
                Y = hy,
                Vx = Math.Cos(angle) * speed,
                Vy = Math.Sin(angle) * speed,
                Life = 1.0,
                Color = DeathColors[p % 5]
            });
        }
    }

    public void LoseHeart() {
        Health--;
        HitFlash = 1.0;
        ScreenShake = 0.6;
        double hx = Head.CurrentScreenPos.X, hy = Head.CurrentScreenPos.Y;
        for (int p = 0; p < 10; p++) {
            double angle = Random.Shared.NextDouble() * Math.PI * 2;
            double speed = 2 + Random.Shared.NextDouble() * 3;
            HitParticles.Add(new HitParticle {
                X = hx,
                Y = hy,
                Vx = Math.Cos(angle) * speed,
                Vy = Math.Sin(angle) * speed,
                Life = 1.0,
                Size = 3 + Random.Shared.NextDouble() * 5,
                Color = RockColors[Random.Shared.Next(5)],
                Rotation = Random.Shared.NextDouble() * Math.PI * 2,
                RotSpeed = (Random.Shared.NextDouble() - 0.5) * 0.3
            });
        }
        if (Health <= 0) Die();
    }

    public void Update(double deltaTime, double screenW, double screenH) {
        if (ScreenShake > 0) ScreenShake = Math.Max(0, ScreenShake - deltaTime / 500);
        if (HitFlash > 0) HitFlash = Math.Max(0, HitFlash - deltaTime / 400);

        for (int i = HitParticles.Count - 1; i >= 0; i--) {
            var p = HitParticles[i];
            p.X += p.Vx; p.Y += p.Vy; p.Vy += 0.12;
            p.Rotation += p.RotSpeed; p.Life -= deltaTime / 500;
            if (p.Life <= 0) { HitParticles[i] = HitParticles[^1]; HitParticles.RemoveAt(HitParticles.Count - 1); }
            else HitParticles[i] = p;
        }
        for (int i = EatParticles.Count - 1; i >= 0; i--) {
            var p = EatParticles[i];
            p.X += p.Vx; p.Y += p.Vy; p.Vy += 0.05;
            p.Life -= deltaTime / 600;
            if (p.Life <= 0) { EatParticles[i] = EatParticles[^1]; EatParticles.RemoveAt(EatParticles.Count - 1); }
            else EatParticles[i] = p;
        }

        if (Dead) {
            DeathTimer += deltaTime;
            double progress = Math.Min(1, DeathTimer / DeathAnimDuration);
            for (int si = 0; si < DeathSegments.Count; si++) {
                var seg = DeathSegments[si];
                seg.X += seg.Vx; seg.Y += seg.Vy;
                seg.Vy += 0.06; seg.Vx *= 0.995;
                seg.Rotation += seg.RotSpeed;
                seg.Alpha = Math.Max(0, 1 - progress * 0.8);
                DeathSegments[si] = seg;
            }
            if (DeathTimer >= DeathAnimDuration) ShowDeathScreen = true;
            return;
        }

        float prog = StepAccumulator / StepInterval;
        foreach (var part in Parts) {
            part.CurrentScreenPos.X = part.StartScreenPos.X + (part.TargetScreenPos.X - part.StartScreenPos.X) * prog;
            part.CurrentScreenPos.Y = part.StartScreenPos.Y + (part.TargetScreenPos.Y - part.StartScreenPos.Y) * prog;
        }

        StepAccumulator += (float)deltaTime;
        if (StepAccumulator >= StepInterval) {
            StepAccumulator -= StepInterval;
            GridStep();
        }

        CamX = Head.CurrentScreenPos.X - screenW / 2;
        CamY = Head.CurrentScreenPos.Y - screenH / 2;
        if (EatBounce > 0) EatBounce = Math.Max(0, EatBounce - deltaTime / 300);

        foreach (var egg in Food) {
            if (!egg.Running) {
                int idx = GetIndex(egg.X, egg.Y);
                if (idx < 0) continue;
                var cc = CenterCoords[idx];
                egg.ScreenX = cc.X; egg.ScreenY = cc.Y;
                bool onScreen = cc.X > CamX - CellSize && cc.X < CamX + screenW + CellSize &&
                                cc.Y > CamY - CellSize && cc.Y < CamY + screenH + CellSize;
                if (onScreen) {
                    egg.Timer -= deltaTime;
                    if (egg.Timer <= 0) {
                        egg.Running = true; egg.RunDir = RandomRunDir(); egg.RunAccum = 0;
                        egg.StartScreenX = cc.X; egg.StartScreenY = cc.Y;
                        egg.TargetScreenX = cc.X; egg.TargetScreenY = cc.Y;
                    }
                }
            }
            else {
                double t = egg.RunAccum / 500.0;
                egg.ScreenX = egg.StartScreenX + (egg.TargetScreenX - egg.StartScreenX) * t;
                egg.ScreenY = egg.StartScreenY + (egg.TargetScreenY - egg.StartScreenY) * t;
                egg.RunAccum += deltaTime;
                if (egg.RunAccum >= 500) {
                    egg.RunAccum -= 500;
                    egg.StartScreenX = egg.TargetScreenX; egg.StartScreenY = egg.TargetScreenY;
                    StepEgg(egg);
                    int idx = GetIndex(egg.X, egg.Y);
                    if (idx >= 0) { var nc = CenterCoords[idx]; egg.TargetScreenX = nc.X; egg.TargetScreenY = nc.Y; }
                }
            }
        }
    }

    void CheckFoodCollision() {
        double eatDistSq = CellSize * (double)CellSize;
        for (int fi = Food.Count - 1; fi >= 0; fi--) {
            var egg = Food[fi];
            double dx = Head.CurrentScreenPos.X - egg.ScreenX, dy = Head.CurrentScreenPos.Y - egg.ScreenY;
            if (dx * dx + dy * dy >= eatDistSq) continue;
            Score++;
            for (int p = 0; p < 8; p++) {
                double angle = (p / 8.0) * Math.PI * 2 + Random.Shared.NextDouble() * 0.5;
                double speed = 1.5 + Random.Shared.NextDouble() * 2;
                EatParticles.Add(new EatParticle {
                    X = egg.ScreenX,
                    Y = egg.ScreenY,
                    Vx = Math.Cos(angle) * speed,
                    Vy = Math.Sin(angle) * speed,
                    Life = 1.0,
                    Color = EatColors[p % 5]
                });
            }
            EatBounce = 1.0;
            Food.RemoveAt(fi);
            var tail = Parts[^1];
            int bx = (int)tail.GridPos.X - (int)tail.Direction.X;
            int by = (int)tail.GridPos.Y - (int)tail.Direction.Y;
            int idx = GetIndex(bx, by);
            var newPart = new SnakeBodyPart {
                GridPos = new Vector2(bx, by),
                Direction = tail.Direction,
                TargetScreenPos = idx >= 0 ? CenterCoords[idx] : tail.TargetScreenPos
            };
            newPart.CurrentScreenPos = newPart.TargetScreenPos;
            newPart.StartScreenPos = newPart.TargetScreenPos;
            Parts.Add(newPart);
            SpawnFood();
        }
    }

    void ProcessInput() {
        bool processed = false;
        var dir = Head.Direction;
        while (KeyQueue.Count > 0 && !processed) {
            int key = KeyQueue.Dequeue();
            switch (key) {
                case 38: if (dir.Y != 1) { Head.Direction = new Vector2(0, -1); processed = true; } break;
                case 40: if (dir.Y != -1) { Head.Direction = new Vector2(0, 1); processed = true; } break;
                case 37: if (dir.X != 1) { Head.Direction = new Vector2(-1, 0); processed = true; } break;
                case 39: if (dir.X != -1) { Head.Direction = new Vector2(1, 0); processed = true; } break;
            }
        }
    }

    void GridStep() {
        ProcessInput();
        CheckFoodCollision();
        for (int i = 0; i < Parts.Count; i++) {
            var part = Parts[i];
            part.StartScreenPos = part.TargetScreenPos;
            part.GridPos += part.Direction;
            if (i == 0) {
                int gx = (int)part.GridPos.X, gy = (int)part.GridPos.Y;
                if (gx < 0 || gx >= Cols || gy < 0 || gy >= Rows) { Die(); return; }
                for (int oi = Obstacles.Count - 1; oi >= 0; oi--) {
                    if ((int)Obstacles[oi].X == gx && (int)Obstacles[oi].Y == gy) {
                        Obstacles.RemoveAt(oi); LoseHeart();
                        if (Dead) return; break;
                    }
                }
            }
            int idx = GetIndex((int)part.GridPos.X, (int)part.GridPos.Y);
            if (idx >= 0) part.TargetScreenPos = CenterCoords[idx];
        }
        for (int i = Parts.Count - 1; i > 0; i--)
            Parts[i].Direction = Parts[i - 1].Direction;
    }

    public (double X, double Y)? GetNearestFoodPos() {
        double hx = Head.CurrentScreenPos.X, hy = Head.CurrentScreenPos.Y;
        double bestDist = double.MaxValue;
        (double X, double Y)? bestPos = null;
        foreach (var f in Food) {
            double dx = f.ScreenX - hx, dy = f.ScreenY - hy;
            double d = dx * dx + dy * dy;
            if (d < bestDist) { bestDist = d; bestPos = (f.ScreenX, f.ScreenY); }
        }
        return bestPos;
    }
}

