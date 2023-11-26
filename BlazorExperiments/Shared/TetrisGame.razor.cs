using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Timer = System.Timers.Timer;

namespace BlazorExperiments.UI.Shared;

public partial class TetrisGame : IAsyncDisposable
{
    private Context2D _context;
    private static Timer _timer;
    private double x, y = 50;
    protected Canvas _canvas;
    private ElementReference _container;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _context = await _canvas.GetContext2DAsync();

            await _context.FontAsync("48px serif");
            await _context.StrokeTextAsync("current y: " + y, 100, 50);
            await _container.FocusAsync();

            _timer = new Timer(10);
            _timer.Elapsed += async (_, _) => { await DrawAsync(); };
            _timer.Enabled = true;
        }
    }

    public void Move(KeyboardEventArgs e)
    {
        if (e.Code == "ArrowDown")
        {
            y += 10;
        }
        else if (e.Code == "ArrowUp")
        {
            y -= 10;
        }
        else if (e.Code == "ArrowLeft")
        {
            x -= 10;
        }
        else if (e.Code == "ArrowRight")
        {
            x += 10;
        }
    }

    private async Task DrawAsync()
    {
        await using var batch = await _context.CreateBatchAsync();

        await batch.ClearRectAsync(0, 0, 600, 400);
        await batch.StrokeTextAsync(DateTime.Now.ToString("mm:ss:FFF"), x, y);
        await batch.StrokeRectAsync(75, 140, 150, 110);
        await batch.FillRectAsync(130, 190, 40, 60);

        await batch.BeginPathAsync();
        await batch.MoveToAsync(50, 140);
        await batch.LineToAsync(150, 60);
        await batch.LineToAsync(250, 140);
        await batch.ClosePathAsync();
        await batch.StrokeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _timer.Dispose();
        await _context.DisposeAsync();
    }
}
