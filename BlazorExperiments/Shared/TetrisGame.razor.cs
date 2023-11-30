using System.Timers;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorExperiments.UI.Shared;

public partial class TetrisGame {
    private double _x, _y = 50;
    CanvasComponent _canvas = null!;

    async Task InitializeAsync() {
        await _canvas.Context.FontAsync("48px serif");
        await _canvas.Context.StrokeTextAsync("current y: " + _y, 100, 50);
    }

    public void Move(KeyboardEventArgs e) {
        if (e.Code == "ArrowDown") {
            _y += 10;
        }
        else if (e.Code == "ArrowUp") {
            _y -= 10;
        }
        else if (e.Code == "ArrowLeft") {
            _x -= 10;
        }
        else if (e.Code == "ArrowRight") {
            _x += 10;
        }
    }

    private async ValueTask DrawAsync(ElapsedEventArgs elapsedEvent) {
        await using var batch = _canvas.Context.CreateBatch();

        await batch.ClearRectAsync(0, 0, 600, 400);
        await batch.FillStyleAsync("white");
        await batch.StrokeStyleAsync("white");
        await batch.StrokeTextAsync(DateTime.Now.ToString("mm:ss:FFF"), _x, _y);
        await batch.StrokeRectAsync(75, 140, 150, 110);
        await batch.FillRectAsync(130, 190, 40, 60);

        await batch.BeginPathAsync();
        await batch.MoveToAsync(50, 140);
        await batch.LineToAsync(150, 60);
        await batch.LineToAsync(250, 140);
        await batch.ClosePathAsync();
        await batch.StrokeAsync();
    }
}
