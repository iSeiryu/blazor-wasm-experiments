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
    TimeSpan _snakeSpeedInMilliseconds = TimeSpan.Zero;
    int _level = 0;

    void InitalizeGame() {
        IncreaseLevel(1);
        _cellSize = _canvas.CellSize;
        _eggs.Clear();
        _eggs.Add(new(_cellSize, (int)_canvas.Width, (int)_canvas.Height));
        _snake = new Snake(_cellSize, (int)_canvas.Width, (int)_canvas.Height);
        _canvas.Timer.Enabled = true;
        _gameOver = false;
        StateHasChanged();
    }

    async ValueTask GameLoopAsync(ElapsedEventArgs elapsedEvent) {
        var eatenEgg = _eggs.FirstOrDefault(egg => _snake.Ate(egg));
        if (eatenEgg is not null) {
            _eggs.Remove(eatenEgg);
            if (_snake.Tail.Count % 6 == 0)
                IncreaseLevel(_level + 1);
        }

        const double eggSpawnChance = 0.005;
        if (Random.Shared.NextDouble() < eggSpawnChance && _eggs.Count < 5 || _eggs.Count == 0)
            AddEgg();

        if (elapsedEvent.SignalTime - _lastTime > _snakeSpeedInMilliseconds) {
            _snake.Update();
            _lastTime = elapsedEvent.SignalTime;

            if (_snake.IsDead()) {
                await GameOver();
                return;
            }
        }

        await DrawAsync(elapsedEvent);
    }

    async ValueTask DrawAsync(ElapsedEventArgs elapsedEvent) {
        await using var batch = _canvas.Context.CreateBatch();

        await ClearScreenAsync(batch);
        await batch.FillStyleAsync("white");
        await batch.FontAsync("12px serif");
        await batch.FillTextAsync("Score: " + _snake.Tail.Count, _canvas.Width - 55, 10);
        await batch.FillTextAsync("Level: " + _level, _canvas.Width - 55, 20);

        await batch.ShadowBlurAsync(50);
        await batch.ShadowColorAsync("darkgreen");
        await batch.FillStyleAsync("green");
        foreach (var cell in _snake.Tail) {
            await batch.FillRectAsync(cell.X, cell.Y, _cellSize, _cellSize);
            await batch.StrokeStyleAsync("white");
            await batch.StrokeRectAsync(cell.X, cell.Y, _cellSize, _cellSize);
        }

        await batch.ShadowColorAsync("red");
        await batch.FillStyleAsync("brown");
        await batch.FillRectAsync(_snake.Head.X, _snake.Head.Y, _cellSize, _cellSize);
        await batch.StrokeStyleAsync("white");
        await batch.StrokeRectAsync(_snake.Head.X, _snake.Head.Y, _cellSize, _cellSize);

        await batch.ShadowBlurAsync(0);
        await batch.FillStyleAsync("yellow");
        foreach (var egg in _eggs)
            await batch.FillRectAsync(egg.X, egg.Y, _cellSize, _cellSize);

        if (showFps)
            await _canvas.DrawFps(batch, elapsedEvent);
    }

    void AddEgg() {
        _eggs.Add(new(_cellSize, (int)_canvas.Width, (int)_canvas.Height));
    }

    void IncreaseLevel(int level) {
        _level = level;
        _snakeSpeedInMilliseconds = TimeSpan.FromMilliseconds(1_000 / 3 / _level);
    }

    void HandleInput(KeyboardEventArgs e) {
        if (_gameOver)
            InitalizeGame();

        else if (e.Code == "ArrowDown")
            _snake.SetDirection(SnakeDirection.Down);
        else if (e.Code == "ArrowUp")
            _snake.SetDirection(SnakeDirection.Up);
        else if (e.Code == "ArrowLeft")
            _snake.SetDirection(SnakeDirection.Left);
        else if (e.Code == "ArrowRight")
            _snake.SetDirection(SnakeDirection.Right);
    }

    void HandleTouchStart(TouchEventArgs e) {
        if (_gameOver)
            InitalizeGame();

        _previousTouch = e?.Touches.FirstOrDefault();
    }

    void HandleTouchMove(TouchEventArgs e) {
        if (_previousTouch == null)
            return;

        const int sensitivity = 5;
        var xDiff = _previousTouch.ClientX - e.Touches[0].ClientX;
        var yDiff = _previousTouch.ClientY - e.Touches[0].ClientY;

        if (Math.Abs(xDiff) < sensitivity && Math.Abs(yDiff) < sensitivity)
            return;

        // most significant
        if (Math.Abs(xDiff) > Math.Abs(yDiff)) {
            _snake.SetDirection(xDiff > 0 ? SnakeDirection.Left : SnakeDirection.Right);
        }
        else {
            _snake.SetDirection(yDiff > 0 ? SnakeDirection.Up : SnakeDirection.Down);
        }

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

        await _canvas.Context.FillStyleAsync("red");
        await _canvas.Context.FontAsync("42px serif");
        await _canvas.Context.FillTextAsync("Game Over", _canvas.Width / 4, _canvas.Height / 2);
    }
}
