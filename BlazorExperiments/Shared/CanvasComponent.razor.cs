using BlazorExperiments.UI.Services;
using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components;
using Timer = System.Timers.Timer;

namespace BlazorExperiments.UI.Shared;
public partial class CanvasComponent {
    private Context2D _context;
    private static Timer _timer;
    protected Canvas _canvas;
    private ElementReference _container;

    double _width = 400, _height = 400;
    string _style = "";
    double _devicePixelRatio = 1;
    const int HeightBuffer = 50;
    const int MediaMinWidth = 641;
    const int SideBarWidth = 250;

    protected override async Task OnParametersSetAsync() {
        var windowProperties = await BrowserResizeService.GetWindowProperties(JS);
        var sideBarWidth = windowProperties.Width > MediaMinWidth ? SideBarWidth : 0;
        var topMenuHeight = sideBarWidth == 0 ? 55 : 0;

        _devicePixelRatio = windowProperties.DevicePixelRatio;
        _width = (int)(windowProperties.Width - sideBarWidth - 50);
        _height = (int)(windowProperties.Height - HeightBuffer - topMenuHeight);
        _style = $"width: {_width}px; height: {_height}px;";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender) {
        if (firstRender) {
            _context = await _canvas.GetContext2DAsync();
            await _context.ScaleAsync(_devicePixelRatio, _devicePixelRatio);
            await _container.FocusAsync();
            await InitializeAsync();

            _timer = new Timer(10);
            _timer.Elapsed += async (_, _) => await LoopAsync();
            _timer.Enabled = true;
        }
    }
}