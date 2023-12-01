using System.Timers;
using BlazorExperiments.UI.Services;
using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace BlazorExperiments.UI.Shared;
public partial class CanvasComponent : IAsyncDisposable {
    string _style = "";
    double _devicePixelRatio = 1;
    const int Margin = 50;
    const int MediaMinWidth = 641;
    const int SideBarWidth = 250;

    double _fps = 0;

    DateTime _lastTimeFpsCalculated = DateTime.Now;
    DateTime _lastTimeCanvasRendered = DateTime.Now;

    async Task SetCanvasSize() {
        var windowProperties = await BrowserResizeService.GetWindowProperties(JS);
        var sideBarWidth = windowProperties.Width > MediaMinWidth ? SideBarWidth : 0;
        var topMenuHeight = sideBarWidth == 0 ? Margin : 0;

        _devicePixelRatio = windowProperties.DevicePixelRatio;
        Width = (int)(windowProperties.Width - sideBarWidth - Margin);
        Height = (int)(windowProperties.Height - Margin - topMenuHeight);
        Width -= Width % CellSize;
        Height -= Height % CellSize;
        _style = $"width: {Width}px; height: {Height}px;";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            var interval = 1_000 / 60; // 60 fps

            await SetCanvasSize();
            Timer = new Timer(interval);
            Context = await Canvas.GetContext2DAsync(alpha: Alpha);

            if (Initialize != null)
                Initialize();
            else
                await InitializeAsync();

            await Context.ScaleAsync(_devicePixelRatio, _devicePixelRatio);
            await Container.FocusAsync();

            Timer.Elapsed += async (_, elapsedEvent) => await LoopAsync(elapsedEvent);
            Timer.Enabled = true;
        }
    }

    public async ValueTask DrawFps(Batch2D batch, ElapsedEventArgs elapsedEvent) {
        if (elapsedEvent.SignalTime - _lastTimeFpsCalculated > TimeSpan.FromSeconds(1)) {
            _fps = 1 / (DateTime.Now - _lastTimeCanvasRendered).TotalSeconds;
            _lastTimeFpsCalculated = elapsedEvent.SignalTime;
        }

        await batch.ClearRectAsync(0, 0, 120, 30);
        await batch.FontAsync("bold 20px Arial");
        await batch.FillTextAsync($"{_fps:F} FPS", 10, 20);

        _lastTimeCanvasRendered = DateTime.Now;
    }

    public async ValueTask DisposeAsync() {
        Timer?.Dispose();
        await Context.DisposeAsync();
    }
}