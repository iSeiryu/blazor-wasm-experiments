using System.Numerics;
using System.Timers;
using BlazorExperiments.UI.Models.SnakeGame;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorExperiments.UI.Shared;

public partial class SnakeGame {
    CanvasComponent _canvas = null!;
    TouchPoint? _previousTouch = null;

    Snake _snake = null!;
    readonly List<Egg> _eggs = [];
    int _cellSize = 1;
    bool _gameOver;
    DateTime _lastTime = DateTime.Now;
    int _level = 1;
    const double EggSpawnChance = 0.005;
    const int SpriteSize = 64;
    TimeSpan _snakeSpeedInMilliseconds = TimeSpan.FromMilliseconds(100);

    void InitializeGame() {
        _cellSize = _canvas.CellSize;
        _eggs.Clear();
        _eggs.Add(new(_cellSize, (int)_canvas.Width, (int)_canvas.Height));
        _snake = new Snake(_cellSize, (int)_canvas.Width, (int)_canvas.Height, 5);
        _snake.SetDirection(new Vector2(1, 0));
        _canvas.Timer.Enabled = true;
        _gameOver = false;
        StateHasChanged();
    }

    async ValueTask GameLoopAsync(ElapsedEventArgs elapsedEvent) {
        var deltaTime = elapsedEvent.SignalTime - _lastTime;
        _lastTime = elapsedEvent.SignalTime;

        if (_snake.Head.Interpolation == 1.0f) {
            var eatenEgg = _eggs.FirstOrDefault(egg => _snake.Ate(egg));
            if (eatenEgg is not null) {
                _eggs.Remove(eatenEgg);
                if (_snake.Body.Count % 6 == 0)
                    IncreaseLevel(_level + 1);
            }
            _snake.SnakeStep();
            //if (_snake.IsDead()) {
            //    await GameOver();
            //    return;
            //}
        }
        _snake.Animate(deltaTime.TotalMilliseconds);
        AddEgg();

        await DrawAsync(elapsedEvent);
    }

    async ValueTask DrawAsync(ElapsedEventArgs elapsedEvent) {
        await using var batch = _canvas.Context.CreateBatch();
        await ClearScreenAsync(batch);
        await DrawText(batch);
        await DrawHead(batch);
        await DrawBody(batch);
        await DrawTail(batch);

        foreach (var egg in _eggs)
            await DrawSprite(batch, 0, 3 * SpriteSize, egg.X, egg.Y);

        if (showFps)
            await _canvas.DrawFps(batch, elapsedEvent);
    }

    async ValueTask DrawText(Batch2D batch) {
        await batch.FillStyleAsync("white");
        await batch.FontAsync("12px serif");
        await batch.FillTextAsync($"Score: {_snake.Body.Count}", _canvas.Width - 55, 10);
        await batch.FillTextAsync($"Level: {_level}", _canvas.Width - 55, 20);
    }

    async ValueTask DrawHead(Batch2D batch) {
        int spriteX, spriteY;

        (spriteX, spriteY) = (4, 0);
        var part = _snake.Head;
        var radius = _cellSize * 0.4;
        await batch.SaveAsync();
        await batch.TranslateAsync(part.AnimationPosition.X, part.AnimationPosition.Y);

        var targetAngle = Math.Atan2(part.Direction.Y, part.Direction.X);
        part.Rotation = part.AngleLerp(part.Rotation, targetAngle, 0.2);

        await batch.RotateAsync(part.Rotation);
        // Draw the image
        await batch.DrawImageAsync(
          "snakeImg",
          spriteX,
          spriteY,
          SpriteSize,
          SpriteSize,
          -radius, // Note the change here
          -radius, // Note the change here
          radius * 2,
          radius * 2
        );

        await batch.RestoreAsync();


    }

    async ValueTask DrawBody(Batch2D batch) {
        for (var i = 1; i < _snake.Body.Count - 1; i++) {
            int tx = 1, ty = 0;
            var curr = _snake.Body[i];
            var prev = _snake.Body[i - 1];
            var next = _snake.Body[i + 1];

            if (prev.Position.X < curr.Position.X && next.Position.X > curr.Position.X || next.Position.X < curr.Position.X && prev.Position.X > curr.Position.X) {
                // Horizontal Left-Right
                tx = 1; ty = 0;
            }
            else if (prev.Position.Y < curr.Position.Y && next.Position.Y > curr.Position.Y || next.Position.Y < curr.Position.Y && prev.Position.Y > curr.Position.Y) {
                // Vertical Up-Down
                tx = 2; ty = 1;
            }

            await DrawSprite(batch, tx * SpriteSize, ty * SpriteSize, curr.AnimationPosition.X, curr.AnimationPosition.Y);
        }
    }

    async ValueTask DrawTail(Batch2D batch) {
        int spriteX = 4, spriteY = 2;
        var next = _snake.Body[1];

        if (next.Position.Y < _snake.Tail.Position.Y) {
            // Up
            (spriteX, spriteY) = (3, 2);
        }
        else if (next.Position.X > _snake.Tail.Position.X) {
            // Right
            (spriteX, spriteY) = (4, 2);
        }
        else if (next.Position.Y > _snake.Tail.Position.Y) {
            // Down
            (spriteX, spriteY) = (4, 3);
        }
        else if (next.Position.X < _snake.Tail.Position.X) {
            // Left
            (spriteX, spriteY) = (3, 3);
        }

        await DrawSprite(batch,
                         spriteX * SpriteSize,
                         spriteY * SpriteSize,
                         _snake.Tail.AnimationPosition.X,
                         _snake.Tail.AnimationPosition.Y);
    }

    async ValueTask DrawSprite(Batch2D batch, int spriteX, int spriteY, double cellX, double cellY) {
        await batch.DrawImageAsync("snakeImg",
                                   spriteX,
                                   spriteY,
                                   SpriteSize,
                                   SpriteSize,
                                   cellX,
                                   cellY,
                                   _cellSize + 1,
                                   _cellSize + 1);
    }

    void AddEgg() {
        if (_eggs.Count < 3 && Random.Shared.NextDouble() < EggSpawnChance || _eggs.Count == 0)
            _eggs.Add(new(_cellSize, (int)_canvas.Width, (int)_canvas.Height));
    }

    void IncreaseLevel(int level) {
        _level = level;
        _snake.IncreaseSnakeSpeed();
    }

    void HandleInput(KeyboardEventArgs e) {
        if (_gameOver)
            InitializeGame();

        else if (e.Code == "ArrowDown")
            _snake.SetDirection(new Vector2(0, 1));
        else if (e.Code == "ArrowUp")
            _snake.SetDirection(new Vector2(0, -1));
        else if (e.Code == "ArrowLeft")
            _snake.SetDirection(new Vector2(-1, 0));
        else if (e.Code == "ArrowRight")
            _snake.SetDirection(new Vector2(1, 0));
    }

    void HandleTouchStart(TouchEventArgs e) {
        if (_gameOver)
            InitializeGame();

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
        //if (Math.Abs(xDiff) > Math.Abs(yDiff)) {
        //    _snake.SetDirection(xDiff > 0 ? SnakeDirection.Left : SnakeDirection.Right);
        //}
        //else {
        //    _snake.SetDirection(yDiff > 0 ? SnakeDirection.Up : SnakeDirection.Down);
        //}

        _previousTouch = e.Touches[^1];
    }

    async Task ClearScreenAsync(Batch2D batch) {
        await batch.ClearRectAsync(0, 0, _canvas.Width, _canvas.Height);
        await batch.FillStyleAsync("black");
        await batch.FillRectAsync(0, 0, _canvas.Width, _canvas.Height);
    }

    async Task GameOver() {
        _gameOver = true;
        _canvas.Timer.Enabled = false;
        _level = 1;

        await _canvas.Context.FillStyleAsync("red");
        await _canvas.Context.FontAsync(_canvas.IsLessThanMediaMinWidth() ? "20px serif" : "42px serif");
        await _canvas.Context.FillTextAsync("Game Over", _canvas.Width / 2 - 100, _canvas.Height / 2 - 100);
    }
}
