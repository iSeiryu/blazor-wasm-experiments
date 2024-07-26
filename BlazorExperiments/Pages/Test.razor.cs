using System.Numerics;
using System.Timers;
using BlazorExperiments.UI.Shared;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.JSInterop;

namespace BlazorExperiments.UI.Pages;

public record Hero(Vector2 Position, Vector2 PrevPosition);

public partial class Test {
    CanvasComponent _canvas = null!;
    const int Size = 50;
    readonly Hero _hero = new(new Vector2(150 + Size, 200 - Size), new Vector2(150, 200));
    Vector2 _animationPosition = Vector2.Zero;
    double _interpolation = 0.0f;
    DateTime _lastRender = DateTime.Now;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        await JS.InvokeVoidAsync("eval", "snakeImg = document.getElementById('snake-img')");
    }

    void Nothing() {
        _animationPosition = _hero.PrevPosition;
    }

    async ValueTask Loop(ElapsedEventArgs elapsedEvent) {
        var deltaTime = elapsedEvent.SignalTime - _lastRender;
        _lastRender = elapsedEvent.SignalTime;

        Update(deltaTime.TotalMilliseconds);

        await using var batch = _canvas.Context.CreateBatch();
        await batch.ClearRectAsync(0, 0, _canvas.Width, _canvas.Height);
        await DrawGrid(batch);

        await batch.DrawImageAsync("snakeImg",
                                   64,
                                   0,
                                   64,
                                   64,
                                   _animationPosition.X,
                                   _animationPosition.Y,
                                   Size,
                                   Size);
    }

    void Update(double milliseconds) {
        _interpolation += milliseconds / 2_000;
        _animationPosition = Vector2.Lerp(_hero.PrevPosition, _hero.Position, (float)_interpolation);
        _interpolation = Math.Min(1.0f, _interpolation); // clamp max value at 1.0
        if (_interpolation == 1.0f) {
            _interpolation = 0.0f;
        }
    }

    async ValueTask DrawGrid(Batch2D batch) {
        await batch.ClearRectAsync(0, 0, _canvas.Width, _canvas.Height);
        await batch.FillStyleAsync("rgba(5, 39, 103, 1)");

        for (int row = 0; row < 30; row++) {
            for (int col = 0; col < 30; col++) {
                var x = col * Size;
                var y = row * Size;

                await batch.StrokeRectAsync(x, y, Size, Size);
            }
        }
    }
}