using System.Timers;
using System.Numerics;
using BlazorExperiments.UI.Models.NeonSnakeGame;
using BlazorExperiments.UI.Shared;
using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NeonSnakeGame = BlazorExperiments.UI.Models.NeonSnakeGame;

namespace BlazorExperiments.UI.Pages;

public partial class NeonSnake {
    const int KeyUp = 38;
    const int KeyDown = 40;
    const int KeyLeft = 37;
    const int KeyRight = 39;
    const int ScreenW = 28 * 40;
    const int ScreenH = 18 * 40 + NeonSnakeGame.Snake.HudH;

    CanvasComponent _canvas = null!;
    NeonSnakeGame.Snake _snake = null!;
    int _cellSize = 1;
    DateTime _lastTick;
    TouchPoint? _previousTouch = null;
    bool _audioEnabled = true;
    double _obstacleInset = 0;
    double _obstacleSize = 0;
    readonly List<Vector2> _bodyPoints = new(256);
    static readonly (double Ox, double Oy, double Rx, double Ry, double A)[] EggSpeckles =
        [(-0.35, -0.18, 0.18, 0.09, 0.4), (0.38, 0.2, 0.13, 0.07, -0.6), (-0.1, 0.35, 0.1, 0.06, 0.9)];
    static readonly (double Offset, string Color)[] TailGradientStops = [(0d, "#9bb6ff"), (1d, Neon.SnakeSecondary)];
    int _lastScore, _lastBestScore;
    string _scoreText = "Score: 0";
    string _bestText = "Best: 0";
    const string BestScoreKey = "neonSnakeBestScore";

    public async Task InitializeAsync() {
        _cellSize = _canvas.CellSize;
        _obstacleInset = _cellSize * 0.08;
        _obstacleSize = _cellSize - _obstacleInset * 2;
        var raw = await JS.InvokeAsync<string?>("localStorage.getItem", BestScoreKey);
        int savedBest = int.TryParse(raw, out var v) ? v : 0;
        _snake = new NeonSnakeGame.Snake(_cellSize) {
            BestScore = savedBest
        };

        _lastTick = DateTime.UtcNow;
        _canvas.Timer.Enabled = true;
        StateHasChanged();
    }

    private async ValueTask LoopAsync(ElapsedEventArgs elapsedEvent) {
        var now = elapsedEvent.SignalTime;
        var dt = (now - _lastTick).TotalMilliseconds;
        _lastTick = now;

        var scoreBefore = _snake.Score;
        var healthBefore = _snake.Health;
        var deadBefore = _snake.Dead;

        _snake.Update(dt, ScreenW, ScreenH);

        if (_snake.Score > scoreBefore) await PlaySoundAsync("neonSnakeAudio.playEatSound");
        if (_snake.Health < healthBefore) await PlaySoundAsync("neonSnakeAudio.playHitSound");
        if (!deadBefore && _snake.Dead) {
            await PlaySoundAsync("neonSnakeAudio.playDeathSound");
            await JS.InvokeVoidAsync("localStorage.setItem", BestScoreKey, _snake.BestScore);
        }

        await using var batch = _canvas.Context.CreateBatch();
        await DrawAsync(batch);
    }

    void HandleInput(KeyboardEventArgs e) {
        if (_snake is null) return;

        if (_snake.ShowDeathScreen && (e.Code == "Space" || e.Key == " " || e.Key == "Spacebar")) {
            _snake = new NeonSnakeGame.Snake(_cellSize);
            _lastTick = DateTime.UtcNow;
            return;
        }

        int? mapped = e.Key switch {
            "ArrowUp" => KeyUp,
            "ArrowDown" => KeyDown,
            "ArrowLeft" => KeyLeft,
            "ArrowRight" => KeyRight,
            _ => e.Code switch {
                "ArrowUp" => KeyUp,
                "ArrowDown" => KeyDown,
                "ArrowLeft" => KeyLeft,
                "ArrowRight" => KeyRight,
                _ => null
            }
        };

        if (mapped.HasValue) {
            _snake.KeyQueue.Enqueue(mapped.Value);
        }
    }

    void HandleTouchStart(TouchEventArgs e) {
        if (_snake.ShowDeathScreen) {
            _snake = new NeonSnakeGame.Snake(_cellSize);
            _lastTick = DateTime.UtcNow;
            return;
        }

        _previousTouch = e?.Touches.FirstOrDefault();
    }

    void HandleTouchMove(TouchEventArgs e) {
        if (_previousTouch == null)
            return;

        const int sensitivity = 10;
        var xDiff = _previousTouch.ClientX - e.Touches[0].ClientX;
        var yDiff = _previousTouch.ClientY - e.Touches[0].ClientY;

        if (Math.Abs(xDiff) < sensitivity && Math.Abs(yDiff) < sensitivity)
            return;

        // most significant
        if (Math.Abs(xDiff) > Math.Abs(yDiff)) {
            _snake.KeyQueue.Enqueue(xDiff > 0 ? KeyLeft : KeyRight);
        }
        else {
            _snake.KeyQueue.Enqueue(yDiff > 0 ? KeyUp : KeyDown);
        }

        _previousTouch = e.Touches[^1];
    }

    async ValueTask PlaySoundAsync(string fn) {
        if (!_audioEnabled) return;
        try {
            await JS.InvokeVoidAsync(fn);
        }
        catch (Exception) {
            _audioEnabled = false;
        }
    }

    async ValueTask DrawAsync(Batch2D ctx) {
        await ctx.ClearRectAsync(0, 0, ScreenW, ScreenH);

        await ctx.FillStyleAsync(0, 0, 0, ScreenH,
            (0d, Neon.BgOuter),
            (1d, Neon.BgInner));
        await ctx.FillRectAsync(0, 0, ScreenW, ScreenH);

        await ctx.FillStyleAsync(ScreenW * 0.5, ScreenH * 0.55, ScreenW * 0.2, ScreenW * 0.5, ScreenH * 0.55, ScreenW * 0.85,
            (0d, "rgba(0,0,0,0)"),
            (1d, "rgba(0,0,0,0.38)"));
        await ctx.FillRectAsync(0, 0, ScreenW, ScreenH);

        var shakeX = 0d;
        var shakeY = 0d;
        if (_snake.ScreenShake > 0) {
            var intensity = _snake.ScreenShake * 80;
            shakeX = (Random.Shared.NextDouble() - 0.5) * intensity * 2;
            shakeY = (Random.Shared.NextDouble() - 0.5) * intensity * 2;
        }

        await ctx.SaveAsync();
        await ctx.TranslateAsync(-_snake.CamX + shakeX, -_snake.CamY + NeonSnakeGame.Snake.HudH + shakeY);

        await DrawGridAsync(ctx);
        await DrawObstaclesAsync(ctx);
        await DrawEggsAsync(ctx);

        if (_snake.Dead && _snake.DeathSegments.Count > 0) {
            await DrawDeathSegmentsAsync(ctx);
        }
        else {
            await DrawSnakeBodyAsync(ctx);
            await DrawHeadAsync(ctx);
        }

        await DrawParticlesAsync(ctx);
        await DrawHitParticlesAsync(ctx);

        await ctx.RestoreAsync();

        if (_snake.HitFlash > 0) {
            await ctx.FillStyleAsync($"rgba(255, 63, 114, {_snake.HitFlash * 0.3})");
            await ctx.FillRectAsync(0, NeonSnakeGame.Snake.HudH, ScreenW, ScreenH - NeonSnakeGame.Snake.HudH);
        }

        await DrawHudAsync(ctx);
        if (_snake.ShowDeathScreen) {
            await DrawDeathScreenAsync(ctx);
        }
    }

    async ValueTask DrawGridAsync(Batch2D ctx) {
        var startCol = Math.Max(0, (int)Math.Floor(_snake.CamX / _cellSize));
        var endCol = Math.Min(NeonSnakeGame.Snake.Cols, (int)Math.Ceiling((_snake.CamX + ScreenW) / _cellSize) + 1);
        var startRow = Math.Max(0, (int)Math.Floor(_snake.CamY / _cellSize));
        var endRow = Math.Min(NeonSnakeGame.Snake.Rows, (int)Math.Ceiling((_snake.CamY + ScreenH) / _cellSize) + 1);

        var x0 = startCol * _cellSize;
        var y0 = startRow * _cellSize;
        var w = (endCol - startCol) * _cellSize;
        var h = (endRow - startRow) * _cellSize;

        await ctx.FillStyleAsync(0, y0, 0, y0 + h,
            (0d, Neon.GridBase),
            (1d, "#090f26"));
        await ctx.FillRectAsync(x0, y0, w, h);

        await ctx.StrokeStyleAsync(Neon.GridLine);
        await ctx.LineWidthAsync(1);

        await ctx.BeginPathAsync();
        for (var c = startCol; c <= endCol; c++) {
            var x = c * _cellSize + 0.5;
            await ctx.MoveToAsync(x, y0);
            await ctx.LineToAsync(x, y0 + h);
        }
        await ctx.StrokeAsync();

        await ctx.BeginPathAsync();
        for (var r = startRow; r <= endRow; r++) {
            var y = r * _cellSize + 0.5;
            await ctx.MoveToAsync(x0, y);
            await ctx.LineToAsync(x0 + w, y);
        }
        await ctx.StrokeAsync();

        await ctx.SaveAsync();
        await ctx.StrokeStyleAsync(Neon.Border);
        await ctx.LineWidthAsync(4);
        await ctx.ShadowColorAsync(Neon.BorderGlow);
        await ctx.ShadowBlurAsync(22);
        await ctx.StrokeRectAsync(0, 0, _snake.WorldW, _snake.WorldH);
        await ctx.RestoreAsync();
    }

    async ValueTask DrawObstaclesAsync(Batch2D ctx) {
        foreach (var o in _snake.Obstacles) {
            var ox = o.X * _cellSize + _obstacleInset;
            var oy = o.Y * _cellSize + _obstacleInset;

            if (ox + _obstacleSize < _snake.CamX - _cellSize || ox > _snake.CamX + ScreenW + _cellSize) continue;
            if (oy + _obstacleSize < _snake.CamY - _cellSize || oy > _snake.CamY + ScreenH + _cellSize) continue;

            await ctx.SaveAsync();
            await ctx.ShadowColorAsync(Neon.ObstacleGlow);
            await ctx.ShadowBlurAsync(12);
            await ctx.FillStyleAsync(ox, oy, ox + _obstacleSize, oy + _obstacleSize,
                (0d, "#2a3b74"),
                (1d, Neon.ObstacleBase));
            await RockPathAsync(ox, oy);
            await ctx.FillAsync(FillRule.NonZero);
            await ctx.RestoreAsync();

            await ctx.StrokeStyleAsync(Neon.ObstacleEdge);
            await ctx.LineWidthAsync(1.5);
            await RockPathAsync(ox, oy);
            await ctx.StrokeAsync();

            await ctx.StrokeStyleAsync("rgba(170,200,255,0.35)");
            await ctx.LineWidthAsync(1);
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(ox + _obstacleSize * 0.22, oy + _obstacleSize * 0.35);
            await ctx.LineToAsync(ox + _obstacleSize * 0.58, oy + _obstacleSize * 0.5);
            await ctx.LineToAsync(ox + _obstacleSize * 0.74, oy + _obstacleSize * 0.72);
            await ctx.StrokeAsync();
        }

        async ValueTask RockPathAsync(double ox, double oy) {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(ox + _obstacleSize * 0.2, oy + _obstacleSize);
            await ctx.LineToAsync(ox, oy + _obstacleSize * 0.5);
            await ctx.LineToAsync(ox + _obstacleSize * 0.15, oy + _obstacleSize * 0.15);
            await ctx.LineToAsync(ox + _obstacleSize * 0.5, oy);
            await ctx.LineToAsync(ox + _obstacleSize * 0.85, oy + _obstacleSize * 0.1);
            await ctx.LineToAsync(ox + _obstacleSize, oy + _obstacleSize * 0.45);
            await ctx.LineToAsync(ox + _obstacleSize * 0.8, oy + _obstacleSize);
            await ctx.ClosePathAsync();
        }
    }

    async ValueTask DrawEggsAsync(Batch2D ctx) {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        foreach (var egg in _snake.Food) {
            var cx = egg.ScreenX;
            var cy = egg.ScreenY;

            if (cx + _cellSize * 1.5 < _snake.CamX || cx - _cellSize * 1.5 > _snake.CamX + ScreenW) continue;
            if (cy + _cellSize * 1.5 < _snake.CamY || cy - _cellSize * 1.5 > _snake.CamY + ScreenH) continue;

            var bob = egg.Running ? 0 : Math.Sin(now * 0.003 + egg.X * 2.1 + egg.Y * 3.7) * 2.5;
            var dy = cy + bob;
            var er = _cellSize * 0.22;
            var eh = _cellSize * 0.3;
            var walkPhase = (now * 0.007) % (Math.PI * 2);

            if (egg.Running) {
                for (var si = 0; si < 2; si++) {
                    var side = si == 0 ? -1 : 1;
                    var extend = 0.65 + 0.35 * Math.Sin(walkPhase + (si == 0 ? 0 : Math.PI));
                    var lx = cx + side * er * 0.5;
                    var ly = dy + eh * 0.7;
                    var lw = er * 0.48;
                    var lh = eh * 0.72 * extend;

                    await ctx.FillStyleAsync("#ffdc96");
                    await ctx.FillRectAsync(lx - lw / 2, ly, lw, lh);

                    var footDirX = egg.RunDir.X != 0 ? egg.RunDir.X * er * 0.2 : 0;
                    await ctx.FillStyleAsync("#ffb855");
                    await ctx.BeginPathAsync();
                    await ctx.EllipseAsync(lx + footDirX, ly + lh, lw * 0.95, lw * 0.52, 0, 0, Math.PI * 2);
                    await ctx.FillAsync(FillRule.NonZero);
                }
            }

            await ctx.FillStyleAsync(cx, dy, er * 0.1, cx, dy, er * 2.6,
                (0d, egg.Running ? "rgba(255,195,50,0.55)" : "rgba(90,255,215,0.45)"),
                (1d, "rgba(0,0,0,0)"));
            await ctx.BeginPathAsync();
            await ctx.EllipseAsync(cx, dy, er * 2.4, eh * 2.4, 0, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);

            await ctx.SaveAsync();
            await ctx.ShadowColorAsync(egg.Running ? "rgba(255,160,30,0.85)" : "rgba(60,220,185,0.8)");
            await ctx.ShadowBlurAsync(18);
            if (egg.Running) {
                await ctx.FillStyleAsync(cx - er * 0.3, dy - eh * 0.32, eh * 0.05, cx, dy, eh * 1.15,
                    (0d, "#fff8e0"),
                    (0.55, "#ffe070"),
                    (1d, "#ff9020"));
            }
            else {
                await ctx.FillStyleAsync(cx - er * 0.3, dy - eh * 0.32, eh * 0.05, cx, dy, eh * 1.15,
                    (0d, "#eefffa"),
                    (0.55, "#88f0d8"),
                    (1d, "#28b09a"));
            }

            await ctx.BeginPathAsync();
            await ctx.EllipseAsync(cx, dy, er, eh, 0, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);
            await ctx.RestoreAsync();

            await ctx.StrokeStyleAsync(egg.Running ? "rgba(255,200,80,0.9)" : "rgba(110,255,225,0.8)");
            await ctx.LineWidthAsync(1.5);
            await ctx.BeginPathAsync();
            await ctx.EllipseAsync(cx, dy, er, eh, 0, 0, Math.PI * 2);
            await ctx.StrokeAsync();

            await ctx.FillStyleAsync(egg.Running ? "rgba(175,85,0,0.28)" : "rgba(25,135,115,0.28)");
            foreach (var s in EggSpeckles) {
                await ctx.BeginPathAsync();
                await ctx.EllipseAsync(cx + s.Ox * er, dy + s.Oy * eh, s.Rx * er, s.Ry * eh, s.A, 0, Math.PI * 2);
                await ctx.FillAsync(FillRule.NonZero);
            }

            await ctx.FillStyleAsync("rgba(255,255,255,0.58)");
            await ctx.BeginPathAsync();
            await ctx.EllipseAsync(cx - er * 0.28, dy - eh * 0.32, er * 0.38, eh * 0.2, -0.3, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);

            if (egg.Running) {
                var rd = egg.RunDir;
                var eyeR = er * 0.165;
                var eyeCx = cx + rd.X * er * 0.38;
                var eyeCy = dy + rd.Y * eh * 0.38;
                var perpX = -rd.Y;
                var perpY = rd.X;

                for (var side = -1; side <= 1; side += 2) {
                    var ex = eyeCx + perpX * er * 0.28 * side;
                    var ey = eyeCy + perpY * eh * 0.28 * side;
                    await ctx.FillStyleAsync("rgba(248,252,255,0.95)");
                    await ctx.BeginPathAsync();
                    await ctx.ArcAsync(ex, ey, eyeR, 0, Math.PI * 2);
                    await ctx.FillAsync(FillRule.NonZero);

                    await ctx.FillStyleAsync("#18103a");
                    await ctx.BeginPathAsync();
                    await ctx.ArcAsync(ex + rd.X * eyeR * 0.22, ey + rd.Y * eyeR * 0.22, eyeR * 0.55, 0, Math.PI * 2);
                    await ctx.FillAsync(FillRule.NonZero);

                    await ctx.FillStyleAsync("rgba(255,255,255,0.88)");
                    await ctx.BeginPathAsync();
                    await ctx.ArcAsync(ex - eyeR * 0.2, ey - eyeR * 0.2, eyeR * 0.24, 0, Math.PI * 2);
                    await ctx.FillAsync(FillRule.NonZero);
                }
            }

            if (!egg.Running) {
                var frac = Math.Max(0, egg.Timer / 5000);
                var ringR = eh * 1.48;
                await ctx.StrokeStyleAsync("rgba(60,200,175,0.2)");
                await ctx.LineWidthAsync(2.2);
                await ctx.BeginPathAsync();
                await ctx.ArcAsync(cx, dy, ringR, 0, Math.PI * 2);
                await ctx.StrokeAsync();

                var tColor = frac > 0.6 ? "#38ffda" : frac > 0.3 ? "#ffd050" : "#ff6830";
                await ctx.SaveAsync();
                await ctx.StrokeStyleAsync(tColor);
                await ctx.ShadowColorAsync(tColor);
                await ctx.ShadowBlurAsync(10);
                await ctx.LineWidthAsync(2.2);
                await ctx.LineCapAsync(LineCap.Round);
                await ctx.BeginPathAsync();
                await ctx.ArcAsync(cx, dy, ringR, -Math.PI / 2, -Math.PI / 2 + frac * Math.PI * 2);
                await ctx.StrokeAsync();
                await ctx.RestoreAsync();
            }
        }
    }

    (double X, double Y)? GetNearestFoodPos() => _snake.GetNearestFoodPos();

    async ValueTask DrawSnakeBodyAsync(Batch2D ctx) {
        var baseR = _cellSize * 0.44;
        var tailR = baseR * 0.35;
        var bodyPoints = _bodyPoints;
        bodyPoints.Clear();
        for (var i = _snake.Parts.Count - 1; i >= 0; i--) {
            bodyPoints.Add(_snake.Parts[i].CurrentScreenPos);
        }

        if (bodyPoints.Count < 2) return;

        async ValueTask TraceBodyPathAsync() {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(bodyPoints[0].X, bodyPoints[0].Y);
            for (var i = 1; i < bodyPoints.Count - 1; i++) {
                var p = bodyPoints[i];
                var n = bodyPoints[i + 1];
                var mx = (p.X + n.X) * 0.5;
                var my = (p.Y + n.Y) * 0.5;
                await ctx.QuadraticCurveToAsync(p.X, p.Y, mx, my);
            }

            var headJoin = bodyPoints[^1];
            await ctx.LineToAsync(headJoin.X, headJoin.Y);
        }

        var tail = bodyPoints[0];
        var nearTail = bodyPoints[1];
        var tailAngle = Math.Atan2(nearTail.Y - tail.Y, nearTail.X - tail.X);

        await ctx.LineCapAsync(LineCap.Round);
        await ctx.LineJoinAsync(LineJoin.Round);

        await ctx.SaveAsync();
        await ctx.StrokeStyleAsync("rgba(0,0,0,0.25)");
        await ctx.LineWidthAsync(baseR * 2.2);
        await ctx.TranslateAsync(1.5, 2);
        await TraceBodyPathAsync();
        await ctx.StrokeAsync();
        await ctx.RestoreAsync();

        await ctx.SaveAsync();
        await ctx.ShadowColorAsync(Neon.SnakeGlow);
        await ctx.ShadowBlurAsync(20);
        await ctx.StrokeStyleAsync(tail.X, tail.Y, _snake.Head.CurrentScreenPos.X, _snake.Head.CurrentScreenPos.Y,
            (0d, Neon.SnakeSecondary),
            (0.55, "#4fb9ff"),
            (1d, Neon.SnakePrimary));
        await ctx.LineWidthAsync(baseR * 2.05);
        await TraceBodyPathAsync();
        await ctx.StrokeAsync();
        await ctx.RestoreAsync();

        await ctx.StrokeStyleAsync("rgba(170, 238, 255, 0.7)");
        await ctx.LineWidthAsync(baseR * 1.12);
        await TraceBodyPathAsync();
        await ctx.StrokeAsync();

        await ctx.StrokeStyleAsync("rgba(255,255,255,0.24)");
        await ctx.LineWidthAsync(baseR * 0.48);
        await ctx.SaveAsync();
        await ctx.TranslateAsync(0, -baseR * 0.14);
        await TraceBodyPathAsync();
        await ctx.StrokeAsync();
        await ctx.RestoreAsync();

        await ctx.SaveAsync();
        await ctx.TranslateAsync(tail.X, tail.Y);
        await ctx.RotateAsync(tailAngle);
        await ctx.FillStyleAsync(-tailR * 0.5, -tailR * 0.2, tailR * 0.2, 0, 0, tailR * 1.4, TailGradientStops);
        await ctx.BeginPathAsync();
        await ctx.EllipseAsync(0, 0, tailR * 1.2, tailR * 0.9, 0, 0, Math.PI * 2);
        await ctx.FillAsync(FillRule.NonZero);
        await ctx.RestoreAsync();
    }

    async ValueTask DrawHeadAsync(Batch2D ctx) {
        var pos = _snake.Head.CurrentScreenPos;
        var r = _cellSize * 0.48;
        var bounce = Math.Sin(_snake.EatBounce * Math.PI) * 4;
        var hr = r + bounce;

        var dir = _snake.Head.Direction;
        var fx = dir.X;
        var fy = dir.Y;
        var angle = Math.Atan2(fy, fx);

        async ValueTask HeadPathAsync() {
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(hr * 0.92, 0);
            await ctx.BezierCurveToAsync(hr * 0.72, -hr * 0.62, hr * 0.02, -hr * 0.9, -hr * 0.38, -hr * 0.76);
            await ctx.BezierCurveToAsync(-hr * 0.84, -hr * 0.46, -hr * 0.84, hr * 0.46, -hr * 0.38, hr * 0.76);
            await ctx.BezierCurveToAsync(hr * 0.02, hr * 0.9, hr * 0.72, hr * 0.62, hr * 0.92, 0);
            await ctx.ClosePathAsync();
        }

        await ctx.SaveAsync();
        await ctx.TranslateAsync(pos.X + 2, pos.Y + 3);
        await ctx.RotateAsync(angle);
        await ctx.FillStyleAsync("rgba(0,0,0,0.28)");
        await HeadPathAsync();
        await ctx.FillAsync(FillRule.NonZero);
        await ctx.RestoreAsync();

        await ctx.SaveAsync();
        await ctx.TranslateAsync(pos.X, pos.Y);
        await ctx.RotateAsync(angle);

        await ctx.SaveAsync();
        await ctx.ShadowColorAsync(Neon.SnakeGlow);
        await ctx.ShadowBlurAsync(26);
        await ctx.FillStyleAsync(-hr * 0.5, -hr * 0.55, hr * 0.6, hr * 0.4,
            (0d, "#aafdff"),
            (0.4, "#38baff"),
            (1d, "#1862ee"));
        await HeadPathAsync();
        await ctx.FillAsync(FillRule.NonZero);
        await ctx.RestoreAsync();

        await ctx.StrokeStyleAsync("rgba(170, 238, 255, 0.7)");
        await ctx.LineWidthAsync(1.4);
        await HeadPathAsync();
        await ctx.StrokeAsync();

        await ctx.FillStyleAsync("rgba(255,255,255,0.3)");
        await ctx.BeginPathAsync();
        await ctx.EllipseAsync(-hr * 0.14, -hr * 0.36, hr * 0.4, hr * 0.17, -0.35, 0, Math.PI * 2);
        await ctx.FillAsync(FillRule.NonZero);

        await ctx.FillStyleAsync("rgba(255, 145, 215, 0.28)");
        await ctx.BeginPathAsync();
        await ctx.EllipseAsync(hr * 0.14, -hr * 0.54, hr * 0.14, hr * 0.09, 0, 0, Math.PI * 2);
        await ctx.FillAsync(FillRule.NonZero);
        await ctx.BeginPathAsync();
        await ctx.EllipseAsync(hr * 0.14, hr * 0.54, hr * 0.14, hr * 0.09, 0, 0, Math.PI * 2);
        await ctx.FillAsync(FillRule.NonZero);

        var foodPos = GetNearestFoodPos();
        var eyeR = hr * 0.21;
        var eyeX = hr * 0.06;
        var eyeY = hr * 0.5;

        for (var side = -1; side <= 1; side += 2) {
            var ex = eyeX;
            var ey = eyeY * side;

            await ctx.FillStyleAsync("rgba(238,255,255,0.98)");
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(ex, ey, eyeR, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);

            await ctx.StrokeStyleAsync("rgba(130, 195, 255, 0.8)");
            await ctx.LineWidthAsync(1.2);
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(ex, ey, eyeR, 0, Math.PI * 2);
            await ctx.StrokeAsync();

            var lookX = 1d;
            var lookY = 0d;
            if (foodPos.HasValue) {
                var dx = foodPos.Value.X - pos.X;
                var dy = foodPos.Value.Y - pos.Y;
                var lx = dx * fx + dy * fy;
                var ly = dx * (-fy) + dy * fx;
                var d = Math.Sqrt(lx * lx + ly * ly);
                if (d > 0.01) {
                    lookX = lx / d;
                    lookY = ly / d;
                }
            }

            var pupilX = ex + lookX * eyeR * 0.28;
            var pupilY = ey + lookY * eyeR * 0.28;

            await ctx.FillStyleAsync("#2a2060");
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(pupilX, pupilY, eyeR * 0.48, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);

            await ctx.FillStyleAsync("rgba(255,255,255,0.92)");
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(pupilX - eyeR * 0.16, pupilY - eyeR * 0.14, eyeR * 0.18, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);

            await ctx.FillStyleAsync("rgba(255,255,255,0.5)");
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(pupilX + eyeR * 0.1, pupilY + eyeR * 0.14, eyeR * 0.08, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);
        }

        await ctx.FillStyleAsync("rgba(25, 45, 105, 0.5)");
        await ctx.BeginPathAsync();
        await ctx.ArcAsync(hr * 0.46, -hr * 0.1, hr * 0.042, 0, Math.PI * 2);
        await ctx.FillAsync(FillRule.NonZero);
        await ctx.BeginPathAsync();
        await ctx.ArcAsync(hr * 0.46, hr * 0.1, hr * 0.042, 0, Math.PI * 2);
        await ctx.FillAsync(FillRule.NonZero);

        if (_snake.EatBounce > 0.3) {
            await ctx.FillStyleAsync("#ff60bc");
            await ctx.BeginPathAsync();
            await ctx.EllipseAsync(hr * 0.76, 0, hr * 0.32, hr * 0.60, 0, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);
            await ctx.FillStyleAsync("#ffd8f4");
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(hr * 0.76, 0, hr * 0.152, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);
        }

        if (_snake.EatBounce < 0.15) {
            var tonguePhase = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % 1800) / 1800d;
            if (tonguePhase < 0.1) {
                var tLen = hr * 0.24 * (1 + Math.Sin((tonguePhase / 0.1) * Math.PI));
                var baseX = hr * 0.9;
                await ctx.StrokeStyleAsync("#ff7ed4");
                await ctx.LineWidthAsync(1.7);
                await ctx.LineCapAsync(LineCap.Round);
                await ctx.BeginPathAsync();
                await ctx.MoveToAsync(baseX, 0);
                await ctx.LineToAsync(baseX + tLen, 0);
                await ctx.LineToAsync(baseX + tLen + hr * 0.07, -hr * 0.055);
                await ctx.MoveToAsync(baseX + tLen, 0);
                await ctx.LineToAsync(baseX + tLen + hr * 0.07, hr * 0.055);
                await ctx.StrokeAsync();
            }
        }

        await ctx.RestoreAsync();
    }

    async ValueTask DrawParticlesAsync(Batch2D ctx) {
        foreach (var p in _snake.EatParticles) {
            var size = 4 * p.Life;
            await ctx.GlobalAlphaAsync(p.Life);
            await ctx.FillStyleAsync(p.Color);
            await ctx.ShadowColorAsync(p.Color);
            await ctx.ShadowBlurAsync(14);

            await ctx.BeginPathAsync();
            for (var s = 0; s < 5; s++) {
                var a = (s / 5d) * Math.PI * 2 - Math.PI / 2;
                var ox = Math.Cos(a) * size;
                var oy = Math.Sin(a) * size;
                if (s == 0) {
                    await ctx.MoveToAsync(p.X + ox, p.Y + oy);
                }
                else {
                    await ctx.LineToAsync(p.X + ox, p.Y + oy);
                }

                var ia = a + Math.PI / 5;
                await ctx.LineToAsync(p.X + Math.Cos(ia) * size * 0.4, p.Y + Math.Sin(ia) * size * 0.4);
            }

            await ctx.ClosePathAsync();
            await ctx.FillAsync(FillRule.NonZero);
        }

        await ctx.GlobalAlphaAsync(1);
        await ctx.ShadowBlurAsync(0);
    }

    async ValueTask DrawHitParticlesAsync(Batch2D ctx) {
        foreach (var p in _snake.HitParticles) {
            await ctx.GlobalAlphaAsync(p.Life);
            await ctx.FillStyleAsync(p.Color);
            await ctx.SaveAsync();
            await ctx.TranslateAsync(p.X, p.Y);
            await ctx.RotateAsync(p.Rotation);
            await ctx.ShadowColorAsync(p.Color);
            await ctx.ShadowBlurAsync(10);

            var s = p.Size;
            await ctx.BeginPathAsync();
            await ctx.MoveToAsync(-s, -s * 0.6);
            await ctx.LineToAsync(-s * 0.3, -s);
            await ctx.LineToAsync(s * 0.6, -s * 0.8);
            await ctx.LineToAsync(s, -s * 0.1);
            await ctx.LineToAsync(s * 0.7, s * 0.8);
            await ctx.LineToAsync(-s * 0.5, s);
            await ctx.ClosePathAsync();
            await ctx.FillAsync(FillRule.NonZero);

            await ctx.RestoreAsync();
        }

        await ctx.GlobalAlphaAsync(1);
        await ctx.ShadowBlurAsync(0);
    }

    async ValueTask DrawDeathSegmentsAsync(Batch2D ctx) {
        var progress = Math.Min(1, _snake.DeathTimer / NeonSnakeGame.Snake.DeathAnimDuration);
        var flashRate = Math.Max(3, 15 - progress * 12);
        var flashOn = Math.Sin(_snake.DeathTimer * 0.01 * flashRate) > 0;

        for (var i = _snake.DeathSegments.Count - 1; i >= 0; i--) {
            var seg = _snake.DeathSegments[i];
            if (seg.Alpha <= 0) continue;

            await ctx.GlobalAlphaAsync(seg.Alpha);
            await ctx.SaveAsync();
            await ctx.TranslateAsync(seg.X, seg.Y);
            await ctx.RotateAsync(seg.Rotation);

            var r = seg.R * (1 - progress * 0.4);
            await ctx.FillStyleAsync("rgba(0,0,0,0.22)");
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(1.5, 2, r, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);

            var baseColor = i % 2 == 0 ? Neon.SnakePrimary : Neon.SnakeSecondary;
            await ctx.FillStyleAsync(flashOn ? Neon.Danger : baseColor);
            await ctx.ShadowColorAsync(flashOn ? Neon.DangerGlow : Neon.SnakeGlow);
            await ctx.ShadowBlurAsync(14);
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(0, 0, r, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);
            await ctx.ShadowBlurAsync(0);

            await ctx.FillStyleAsync("rgba(255,255,255,0.35)");
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(-r * 0.25, -r * 0.25, r * 0.35, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);

            if (seg.IsHead && progress < 0.6) {
                var eyeOff = r * 0.25;
                await ctx.StrokeStyleAsync("rgba(255, 220, 245, 0.9)");
                await ctx.LineWidthAsync(2.5);
                await ctx.LineCapAsync(LineCap.Round);
                for (var side = -1; side <= 1; side += 2) {
                    var ex = eyeOff * side;
                    var ey = -r * 0.1;
                    var xs = r * 0.15;
                    await ctx.BeginPathAsync();
                    await ctx.MoveToAsync(ex - xs, ey - xs);
                    await ctx.LineToAsync(ex + xs, ey + xs);
                    await ctx.MoveToAsync(ex + xs, ey - xs);
                    await ctx.LineToAsync(ex - xs, ey + xs);
                    await ctx.StrokeAsync();
                }
            }

            await ctx.RestoreAsync();
        }

        await ctx.GlobalAlphaAsync(1);
    }

    async ValueTask DrawHudAsync(Batch2D ctx) {
        await ctx.FillStyleAsync(0, 0, 0, NeonSnakeGame.Snake.HudH,
            (0d, Neon.HudBgTop),
            (1d, Neon.HudBgBottom));
        await ctx.FillRectAsync(0, 0, ScreenW, NeonSnakeGame.Snake.HudH);
        await ctx.FillStyleAsync(Neon.HudLine);
        await ctx.FillRectAsync(0, NeonSnakeGame.Snake.HudH - 2, ScreenW, 2);

        await ctx.FillStyleAsync(Neon.TextPrimary);
        await ctx.ShadowColorAsync(Neon.SnakeGlow);
        await ctx.ShadowBlurAsync(10);
        await ctx.FontAsync("bold 22px monospace");
        await ctx.TextAlignAsync(TextAlign.Left);
        if (_snake.Score != _lastScore) { _lastScore = _snake.Score; _scoreText = $"Score: {_snake.Score}"; }
        await ctx.FillTextAsync(_scoreText, 15, 33);
        await ctx.ShadowBlurAsync(0);

        var heartSize = 14d;
        var heartGap = 36d;
        var heartsW = 3 * heartGap;
        var hx0 = (ScreenW - heartsW) / 2d + heartGap / 2d;
        var hy = 26d;
        for (var i = 0; i < 3; i++) {
            var cx = hx0 + i * heartGap;
            await DrawHeartAsync(ctx, cx, hy, heartSize, i < _snake.Health ? Neon.HeartOn : Neon.HeartOff);
        }

        await ctx.FillStyleAsync(Neon.TextMuted);
        await ctx.FontAsync("18px monospace");
        await ctx.TextAlignAsync(TextAlign.Right);
        if (_snake.BestScore != _lastBestScore) { _lastBestScore = _snake.BestScore; _bestText = $"Best: {_snake.BestScore}"; }
        await ctx.FillTextAsync(_bestText, ScreenW - 15, 33);
        await ctx.TextAlignAsync(TextAlign.Left);
    }

    async ValueTask DrawHeartAsync(Batch2D ctx, double cx, double cy, double size, string color) {
        var isOff = color == Neon.HeartOff;
        if (!isOff) {
            await ctx.SaveAsync();
            await ctx.ShadowColorAsync("rgba(255, 77, 226, 0.75)");
            await ctx.ShadowBlurAsync(12);
        }

        await ctx.FillStyleAsync(color);
        await ctx.BeginPathAsync();
        await ctx.MoveToAsync(cx, cy + size * 0.35);
        await ctx.BezierCurveToAsync(cx, cy - size * 0.2, cx - size, cy - size * 0.6, cx - size, cy + size * 0.05);
        await ctx.BezierCurveToAsync(cx - size, cy + size * 0.55, cx, cy + size * 0.9, cx, cy + size * 1.1);
        await ctx.BezierCurveToAsync(cx, cy + size * 0.9, cx + size, cy + size * 0.55, cx + size, cy + size * 0.05);
        await ctx.BezierCurveToAsync(cx + size, cy - size * 0.6, cx, cy - size * 0.2, cx, cy + size * 0.35);
        await ctx.ClosePathAsync();
        await ctx.FillAsync(FillRule.NonZero);
        if (!isOff) {
            await ctx.RestoreAsync();
        }

        if (!isOff) {
            await ctx.FillStyleAsync("rgba(255,255,255,0.45)");
            await ctx.BeginPathAsync();
            await ctx.ArcAsync(cx - size * 0.35, cy + size * 0.05, size * 0.25, 0, Math.PI * 2);
            await ctx.FillAsync(FillRule.NonZero);
        }
    }

    async ValueTask DrawDeathScreenAsync(Batch2D ctx) {
        await ctx.FillStyleAsync(0, NeonSnakeGame.Snake.HudH, 0, ScreenH,
            (0d, "rgba(7,12,31,0.7)"),
            (1d, "rgba(2,4,13,0.8)"));
        await ctx.FillRectAsync(0, NeonSnakeGame.Snake.HudH, ScreenW, ScreenH - NeonSnakeGame.Snake.HudH);

        await ctx.FillStyleAsync(Neon.Danger);
        await ctx.ShadowColorAsync(Neon.DangerGlow);
        await ctx.ShadowBlurAsync(18);
        await ctx.FontAsync("bold 48px monospace");
        await ctx.TextAlignAsync(TextAlign.Center);
        var cy = NeonSnakeGame.Snake.HudH + (ScreenH - NeonSnakeGame.Snake.HudH) / 2d;
        await ctx.FillTextAsync("Game Over", ScreenW / 2d, cy - 30);

        await ctx.ShadowBlurAsync(0);
        await ctx.FillStyleAsync("#f2f8ff");
        await ctx.FontAsync("24px monospace");
        await ctx.FillTextAsync($"Score: {_snake.Score}", ScreenW / 2d, cy + 15);
        if (_snake.Score >= _snake.BestScore && _snake.Score > 0) {
            await ctx.FillStyleAsync(Neon.TextAccent);
            await ctx.FontAsync("20px monospace");
            await ctx.FillTextAsync("New Best!", ScreenW / 2d, cy + 45);
        }

        await ctx.FontAsync("18px monospace");
        await ctx.FillStyleAsync(Neon.TextMuted);
        await ctx.FillTextAsync("Press SPACE to restart", ScreenW / 2d, cy + 80);
        await ctx.TextAlignAsync(TextAlign.Start);
    }
}
