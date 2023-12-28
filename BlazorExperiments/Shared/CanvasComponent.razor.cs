using System.Timers;
using BlazorExperiments.UI.Models;
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
    const double Interval = 1_000 / 60; // 60 fps

    double _fps = 0;

    DateTime _lastTimeFpsCalculated = DateTime.Now;
    DateTime _lastTimeCanvasRendered = DateTime.Now;

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            await SetCanvasSize();
            Timer = new Timer(Interval);
            Context = await Canvas.GetContext2DAsync(alpha: Alpha);
            await Context.ScaleAsync(_devicePixelRatio, _devicePixelRatio);
            await Container.FocusAsync();

            if (Initialize != null)
                Initialize();
            else
                await InitializeAsync();

            Timer.Elapsed += async (_, elapsedEvent) => await LoopAsync(elapsedEvent);
            Timer.Enabled = true;
        }
    }

    async Task SetCanvasSize() {
        WindowProperties = await BrowserResizeService.GetWindowProperties(JS);
        var sideBarWidth = WindowProperties.Width > MediaMinWidth ? SideBarWidth : 0;
        var topMenuHeight = sideBarWidth == 0 ? Margin : 0;

        _devicePixelRatio = WindowProperties.DevicePixelRatio;
        Width = (int)(WindowProperties.Width - sideBarWidth - Margin);
        Height = (int)(WindowProperties.Height - Margin - topMenuHeight);
        Width -= Width % CellSize;
        Height -= Height % CellSize;
        _style = $"width: {Width}px; height: {Height}px;";
        StateHasChanged();
    }

    public WindowProperties WindowProperties { get; private set; } = default!;

    public async ValueTask DrawFps(Batch2D batch, ElapsedEventArgs elapsedEvent) {
        if (elapsedEvent.SignalTime - _lastTimeFpsCalculated > TimeSpan.FromMilliseconds(1_200)) {
            _fps = 1 / (DateTime.Now - _lastTimeCanvasRendered).TotalSeconds;
            _lastTimeFpsCalculated = elapsedEvent.SignalTime;
        }

        await batch.ClearRectAsync(0, 0, 120, 30);
        await batch.FontAsync("bold 20px Arial");
        await batch.FillTextAsync($"{_fps:F} FPS", 10, 20);

        _lastTimeCanvasRendered = DateTime.Now;
    }

    public bool IsLessThanMediaMinWidth() => WindowProperties.Width < MediaMinWidth;

    public async ValueTask DisposeAsync() {
        Timer?.Dispose();
        await Context.DisposeAsync();
    }
}