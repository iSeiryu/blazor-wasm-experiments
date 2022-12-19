using System;
using System.Linq;
using System.Threading.Tasks;
using BlazorExperiments.UI.Models.SnakeGame;
using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorExperiments.UI.Shared;

public partial class SnakeGame : IAsyncDisposable
{

    internal static int CellSize = 25;
    private Context2D _context;
    private Canvas _canvas;
    private ElementReference _container;
    private TouchPoint _previousTouch = null;

    private int _width = 400,
                _height = 400;

    private Snake _snake;
    private Egg _egg;
    private int _cellSize = 0;
    private bool _gameOver;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _context = await _canvas.GetContext2DAsync();
            await _container.FocusAsync();
            await InitAsync();
        }
    }

    protected override void OnParametersSet()
    {
        InitalizeGame();
    }

    private async Task InitAsync()
    {
        InitalizeGame();
        await GameLoopAsync();
    }

    private void InitalizeGame()
    {
        _width = possibleGameSize - CellSize;
        _height = possibleGameSize - CellSize;
        _cellSize = _width / CellSize;
        _egg = new Egg(_cellSize, _width, _height);
        _snake = new Snake(_cellSize, _width, _height);
        _gameOver = false;
    }

    private async Task GameLoopAsync()
    {
        if (_gameOver) return;

        if (_snake.Ate(_egg))
        {
            _egg.NewLocation();
        }

        _snake.Update();
        await DrawAsync();

        if (_snake.IsDead())
            await GameOver();

        await Task.Delay(150);
        await GameLoopAsync();
    }

    private async Task DrawAsync()
    {
        await using var batch = await _context.CreateBatchAsync();

        await ClearScreenAsync();
        await batch.FillStyleAsync("white");
        await batch.FontAsync("12px serif");
        await batch.FillTextAsync("Score: " + _snake.Tail.Count, _width - 55, 10);

        foreach (var cell in _snake.Tail)
        {
            await batch.FillRectAsync(cell.X, cell.Y, _cellSize, _cellSize);
        }

        await batch.FillStyleAsync("green");
        await batch.FillRectAsync(_snake.Head.X, _snake.Head.Y, _cellSize, _cellSize);

        await batch.FillStyleAsync("yellow");
        await batch.FillRectAsync(_egg.X, _egg.Y, _cellSize, _cellSize);
    }

    private async Task HandleInput(KeyboardEventArgs e)
    {
        if (_gameOver)
            await InitAsync();

        else if (e.Code == "ArrowDown") _snake.SetDirection(SnakeDirection.Down);
        else if (e.Code == "ArrowUp") _snake.SetDirection(SnakeDirection.Up);
        else if (e.Code == "ArrowLeft") _snake.SetDirection(SnakeDirection.Left);
        else if (e.Code == "ArrowRight") _snake.SetDirection(SnakeDirection.Right);

        Console.WriteLine(e.Code);
    }

    private async Task HandleTouchStart(TouchEventArgs e)
    {
        if (_gameOver)
            await InitAsync();

        _previousTouch = e.Touches.FirstOrDefault();
    }

    private void HandleTouchMove(TouchEventArgs e)
    {
        if (_previousTouch == null) return;

        var xDiff = _previousTouch.ClientX - e.Touches[0].ClientX;
        var yDiff = _previousTouch.ClientY - e.Touches[0].ClientY;

        // most significant
        if (Math.Abs(xDiff) > Math.Abs(yDiff))
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
        await _context.FontAsync("48px serif");
        await _context.FillTextAsync("Game Over", _width / 4, _height / 2);
    }

    public async ValueTask DisposeAsync()
    {
        _gameOver = true;
        await _context.DisposeAsync();
    }
}
