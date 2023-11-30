using BlazorExperiments.UI.Services;
using Excubo.Blazor.Canvas;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace BlazorExperiments.UI.Shared;
public partial class CanvasComponent : IAsyncDisposable {
    string _style = "";
    double _devicePixelRatio = 1;
    const int Margin = 50;
    const int MediaMinWidth = 641;
    const int SideBarWidth = 250;

    protected override async Task OnParametersSetAsync() {
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

            Timer = new Timer(interval);
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

    public async ValueTask DisposeAsync() {
        Timer?.Dispose();
        await Context.DisposeAsync();
    }
}