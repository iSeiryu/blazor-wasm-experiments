using System.Timers;
using BlazorExperiments.UI.Models.SnakeGame;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorExperiments.UI.Shared;

public partial class SnakeGame {
    CanvasComponent _canvas = null!;
    TouchPoint? _previousTouch = null;

    Snake _snake = null!;
    Egg _egg = null!;
    int _cellSize = 0;
    bool _gameOver;
    DateTime _lastTime = DateTime.Now;
    TimeSpan _snakeSpeedInMilliseconds = TimeSpan.Zero;
    int _level = 4;

    void InitalizeGame() {
        UpdateSnakeSpeed(_level);
        _cellSize = _canvas.CellSize;
        _egg = new Egg(_cellSize, (int)_canvas.Width, (int)_canvas.Height);
        _snake = new Snake(_cellSize, (int)_canvas.Width, (int)_canvas.Height);
        _gameOver = false;
    }

    async ValueTask GameLoopAsync(ElapsedEventArgs elapsedEvent) {
        if (_gameOver)
            return;

        if (elapsedEvent.SignalTime - _lastTime > _snakeSpeedInMilliseconds) {
            if (_snake.Ate(_egg)) {
                _egg.NewLocation();
                if (_snake.Tail.Count % 10 == 0)
                    UpdateSnakeSpeed(_level + 1);
            }

            _snake.Update();
            _lastTime = elapsedEvent.SignalTime;

            if (_snake.IsDead())
                await GameOver();
        }

        await DrawAsync();
    }

    async ValueTask DrawAsync() {
        await using var batch = _canvas.Context.CreateBatch();

        await ClearScreenAsync();
        await batch.FillStyleAsync("white");
        await batch.FontAsync("12px serif");
        await batch.FillTextAsync("Score: " + _snake.Tail.Count, _canvas.Width - 55, 10);
        await batch.FillTextAsync("Level: " + _level, _canvas.Width - 55, 20);

        await batch.FillStyleAsync("green");
        foreach (var cell in _snake.Tail) {
            await batch.FillRectAsync(cell.X, cell.Y, _cellSize, _cellSize);
            await batch.StrokeStyleAsync("white");
            await batch.StrokeRectAsync(cell.X, cell.Y, _cellSize, _cellSize);
        }

        await batch.FillStyleAsync("brown");
        await batch.FillRectAsync(_snake.Head.X, _snake.Head.Y, _cellSize, _cellSize);
        await batch.StrokeStyleAsync("white");
        await batch.StrokeRectAsync(_snake.Head.X, _snake.Head.Y, _cellSize, _cellSize);

        await batch.FillStyleAsync("yellow");
        await batch.FillRectAsync(_egg.X, _egg.Y, _cellSize, _cellSize);
    }

    void UpdateSnakeSpeed(int speed) {
        _level = speed;
        _snakeSpeedInMilliseconds = TimeSpan.FromMilliseconds(1_000 / _level);
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

    async Task ClearScreenAsync() {
        await _canvas.Context.ClearRectAsync(0, 0, _canvas.Width, _canvas.Height);
        await _canvas.Context.FillStyleAsync("black");
        await _canvas.Context.FillRectAsync(0, 0, _canvas.Width, _canvas.Height);
    }

    async Task GameOver() {
        _gameOver = true;

        await _canvas.Context.FillStyleAsync("red");
        await _canvas.Context.FontAsync("42px serif");
        await _canvas.Context.FillTextAsync("Game Over", _canvas.Width / 4, _canvas.Height / 2);
    }
}
