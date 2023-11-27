﻿using BlazorExperiments.UI.Models.SnakeGame;
using BlazorExperiments.UI.Services;
using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorExperiments.UI.Shared;

public partial class SnakeGame : IAsyncDisposable
{
    const int CellSize = 20;
    private Context2D _context;
    private Canvas _canvas;
    private ElementReference _container;
    private TouchPoint? _previousTouch = null;

    string _style = "";
    private double _devicePixelRatio = 1;
    private const int _heightBuffer = 50;
    private const int _mediaMinWidth = 641;
    private const int _sideBarWidth = 250;
    private int _width = 400,
                _height = 400;

    private Snake _snake;
    private Egg _egg;
    private int _cellSize = 0;
    private bool _gameOver;

    protected override async Task OnParametersSetAsync()
    {
        var windowProperties = await BrowserResizeService.GetWindowProperties(JS);
        var sideBarWidth = windowProperties.Width > _mediaMinWidth ? _sideBarWidth : 0;
        var topMenuHeight = sideBarWidth == 0 ? 55 : 0;

        _devicePixelRatio = windowProperties.DevicePixelRatio;
        _width = (int)(windowProperties.Width - sideBarWidth - 50);
        _height = (int)(windowProperties.Height - _heightBuffer - topMenuHeight);
        _width = _width - (_width % CellSize);
        _height = _height - (_height % CellSize);
        _style = $"width: {_width}px; height: {_height}px;";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _context = await _canvas.GetContext2DAsync();
            await _context.ScaleAsync(_devicePixelRatio, _devicePixelRatio);
            await _container.FocusAsync();
            await InitAsync();
        }
    }

    private async Task InitAsync()
    {
        InitalizeGame();
        await GameLoopAsync();
    }

    private void InitalizeGame()
    {
        _cellSize = CellSize;
        _egg = new Egg(_cellSize, _width, _height);
        _snake = new Snake(_cellSize, _width, _height);
        _gameOver = false;
    }

    private async Task GameLoopAsync()
    {
        if (_gameOver)
            return;

        if (_snake.Ate(_egg))
        {
            _egg.NewLocation();
        }

        _snake.Update();
        await DrawAsync();

        if (_snake.IsDead())
            await GameOver();

        await Task.Delay(100);
        await GameLoopAsync();
    }

    private async Task DrawAsync()
    {
        await using var batch = _context.CreateBatch();

        await ClearScreenAsync();
        await batch.FillStyleAsync("white");
        await batch.FontAsync("12px serif");
        await batch.FillTextAsync("Score: " + _snake.Tail.Count, _width - 55, 10);

        await batch.FillStyleAsync("green");
        foreach (var cell in _snake.Tail)
        {
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

    private async Task HandleInput(KeyboardEventArgs e)
    {
        if (_gameOver)
            await InitAsync();

        else if (e.Code == "ArrowDown")
            _snake.SetDirection(SnakeDirection.Down);
        else if (e.Code == "ArrowUp")
            _snake.SetDirection(SnakeDirection.Up);
        else if (e.Code == "ArrowLeft")
            _snake.SetDirection(SnakeDirection.Left);
        else if (e.Code == "ArrowRight")
            _snake.SetDirection(SnakeDirection.Right);
    }

    private async Task HandleTouchStart(TouchEventArgs e)
    {
        if (_gameOver)
            await InitAsync();

        _previousTouch = e?.Touches.FirstOrDefault();
    }

    private void HandleTouchMove(TouchEventArgs e)
    {
        if (_previousTouch == null)
            return;

        const int sensitivity = 5;
        var xDiff = Math.Abs(_previousTouch.ClientX - e.Touches[0].ClientX);
        var yDiff = Math.Abs(_previousTouch.ClientY - e.Touches[0].ClientY);

        if (xDiff < sensitivity && yDiff < sensitivity)
            return;

        // most significant
        if (xDiff > yDiff)
        {
            _snake.SetDirection(xDiff > 0 ? SnakeDirection.Left : SnakeDirection.Right);
        }
        else
        {
            _snake.SetDirection(yDiff > 0 ? SnakeDirection.Up : SnakeDirection.Down);
        }

        _previousTouch = e.Touches[^1];
    }

    private async Task ClearScreenAsync()
    {
        await _context.ClearRectAsync(0, 0, _width, _height);
        await _context.FillStyleAsync("black");
        await _context.FillRectAsync(0, 0, _width, _height);
    }

    private async Task GameOver()
    {
        _gameOver = true;

        await _context.FillStyleAsync("red");
        await _context.FontAsync("42px serif");
        await _context.FillTextAsync("Game Over", _width / 4, _height / 2);
    }

    public async ValueTask DisposeAsync()
    {
        _gameOver = true;
        await _context.DisposeAsync();
    }
}
