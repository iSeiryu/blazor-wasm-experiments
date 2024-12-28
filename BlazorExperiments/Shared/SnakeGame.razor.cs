﻿using System.Timers;
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

    void InitializeGame() {
        _cellSize = _canvas.CellSize;
        _eggs.Clear();
        _eggs.Add(new(_cellSize, (int)_canvas.Width, (int)_canvas.Height));
        _snake = new Snake(_cellSize, (int)_canvas.Width, (int)_canvas.Height, 5);
        _canvas.Timer.Enabled = true;
        _gameOver = false;
        StateHasChanged();
    }

    async ValueTask GameLoopAsync(ElapsedEventArgs elapsedEvent) {
        var deltaTime = elapsedEvent.SignalTime - _lastTime;
        _lastTime = elapsedEvent.SignalTime;

        if (_snake.Head.Interpolation == 1.0f) {
            _snake.SnakeStep();
            if (_snake.IsDead()) {
                await GameOver();
                return;
            }

            var eatenEgg = _eggs.FirstOrDefault(egg => _snake.Ate(egg));
            if (eatenEgg is not null) {
                _eggs.Remove(eatenEgg);
                if (_snake.Tail.Count % 6 == 0)
                    IncreaseLevel(_level + 1);
            }
        }

        _snake.Animate(deltaTime.TotalMilliseconds);
        AddEgg();

        await DrawAsync(elapsedEvent);
    }

    async ValueTask DrawAsync(ElapsedEventArgs elapsedEvent) {
        await using var batch = _canvas.Context.CreateBatch();
        await ClearScreenAsync(batch);
        await batch.FillStyleAsync("white");
        await batch.FontAsync("12px serif");
        await batch.FillTextAsync($"Score: {_snake.Tail.Count}", _canvas.Width - 55, 10);
        await batch.FillTextAsync($"Level: {_level}", _canvas.Width - 55, 20);

        for (var i = 0; i < _snake.Tail.Count - 1; i++) {
            var cell = _snake.Tail[i];
            if (cell.PrevPosition.X != cell.Position.X)
                await batch.DrawImageAsync("snakeImg", 1 * 64, 0, 64, 64, cell.AnimationPosition.X, cell.AnimationPosition.Y, _cellSize, _cellSize);
            else
                await batch.DrawImageAsync("snakeImg", 2 * 64, 1 * 64, 64, 64, cell.AnimationPosition.X, cell.AnimationPosition.Y, _cellSize, _cellSize);
        }

        if (_snake.Head.PrevPosition.X < _snake.Head.Position.X)
            await batch.DrawImageAsync("snakeImg", 4 * 64, 0, 64, 64, _snake.Head.AnimationPosition.X, _snake.Head.AnimationPosition.Y, _cellSize, _cellSize);
        else if (_snake.Head.PrevPosition.X > _snake.Head.Position.X)
            await batch.DrawImageAsync("snakeImg", 3 * 64, 1 * 64, 64, 64, _snake.Head.AnimationPosition.X, _snake.Head.AnimationPosition.Y, _cellSize, _cellSize);
        else if (_snake.Head.PrevPosition.Y < _snake.Head.Position.Y)
            await batch.DrawImageAsync("snakeImg", 4 * 64, 1 * 64, 64, 64, _snake.Head.AnimationPosition.X, _snake.Head.AnimationPosition.Y, _cellSize, _cellSize);
        else if (_snake.Head.PrevPosition.Y > _snake.Head.Position.Y)
            await batch.DrawImageAsync("snakeImg", 3 * 64, 0, 64, 64, _snake.Head.AnimationPosition.X, _snake.Head.AnimationPosition.Y, _cellSize, _cellSize);

        foreach (var egg in _eggs)
            await batch.DrawImageAsync("snakeImg", 0, 3 * 64, 64, 64, egg.X, egg.Y, _cellSize, _cellSize);

        if (_showFps)
            await _canvas.DrawFps(batch, elapsedEvent);
    }

    void AddEgg() {
        if (_eggs.Count < 5 && Random.Shared.NextDouble() < EggSpawnChance || _eggs.Count == 0)
            _eggs.Add(new(_cellSize, (int)_canvas.Width, (int)_canvas.Height));
    }

    void IncreaseLevel(int level) {
        _level = level;
        _snake.IncreaseSnakeSpeed(_level);
    }

    void HandleInput(KeyboardEventArgs e) {
        if (_gameOver)
            InitializeGame();

        if (e.Code == "ArrowDown")
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
        _level = 1;

        await _canvas.Context.FillStyleAsync("red");
        await _canvas.Context.FontAsync(_canvas.IsLessThanMediaMinWidth() ? "20px serif" : "42px serif");
        await _canvas.Context.FillTextAsync("Game Over", _canvas.Width / 2 - 100, _canvas.Height / 2 - 100);
    }
}
