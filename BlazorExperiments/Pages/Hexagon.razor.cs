using BlazorExperiments.UI.Services;
using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;

namespace BlazorExperiments.UI.Pages;

public partial class Hexagon : IAsyncDisposable
{
    Canvas _canvas;
    Context2D _ctx;

    double _width = 400, _height = 400;
    string _style = "";
    double _devicePixelRatio = 1;
    const int _heightBuffer = 50;
    const int _mediaMinWidth = 641;
    const int _sideBarWidth = 250;

    const double len = 20, count = 50, baseTime = 10, addedTime = 10,
           dieChance = .05, sparkChance = .1,
           sparkDist = 10, sparkSize = 1,

           baseLight = 50, addedLight = 10,
           shadowToTimePropMult = 6,
           baseLightInputMultiplier = .01, addedLightInputMultiplier = .02,

           hueChange = .1;

    static double tick = 0;

    const string _color = "hsl(hue,100%,light%)";
    const double baseRad = Math.PI * 2 / 6;
    static double cx, cy, dieX, dieY;
    readonly List<Line> lines = new();

    protected override async Task OnParametersSetAsync()
    {
        var windowProperties = await BrowserResizeService.GetWindowProperties(JS);
        var sideBarWidth = windowProperties.Width > _mediaMinWidth ? _sideBarWidth : 0;
        var topMenuHeight = sideBarWidth == 0 ? 55 : 0;

        _devicePixelRatio = windowProperties.DevicePixelRatio;
        _width = (int)(windowProperties.Width - sideBarWidth - 50);
        _height = (int)(windowProperties.Height - _heightBuffer - topMenuHeight);
        _style = $"width: {_width}px; height: {_height}px;";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _ctx = await _canvas.GetContext2DAsync();
            await _ctx.ScaleAsync(_devicePixelRatio, _devicePixelRatio);
            await Initialize();
        }
    }

    public async Task Initialize()
    {
        cx = _width / 2;
        cy = _height / 2;

        dieX = _width / 2 / len;
        dieY = _height / 2 / len;

        await _ctx.FillStyleAsync("black");
        await _ctx.FillRectAsync(0, 0, _width, _height);

        // Start the loop
        await Loop();
    }

    private async Task Loop()
    {
        tick++;

        await _ctx.GlobalCompositeOperationAsync(CompositeOperation.Source_Over);
        await _ctx.ShadowBlurAsync(0);
        await _ctx.FillStyleAsync("rgba(0,0,0,0.04)");
        await _ctx.FillRectAsync(0, 0, _width, _height);
        await _ctx.GlobalCompositeOperationAsync(CompositeOperation.Lighter);

        if (lines.Count < count)
            lines.Add(new());

        foreach (var line in lines)
        {
            await Step(line);
        }

        await Task.Delay(1);
        await Loop(); // Keep looping
    }

    async Task Step(Line line)
    {
        line.time++;
        line.cumulativeTime++;

        if (line.time >= line.targetTime)
            line.BeginPhase();

        double prop = line.time / line.targetTime;
        double wave = Math.Sin(prop * Math.PI / 2);
        double _x = line.addedX * wave;
        double _y = line.addedY * wave;

        var newColor = line.color.Replace("light", (baseLight + addedLight * Math.Sin(line.cumulativeTime * line.lightInputMultiplier)).ToString());
        await using var batch = _ctx.CreateBatch();
        await batch.ShadowBlurAsync(prop * shadowToTimePropMult);
        await batch.ShadowColorAsync(newColor);
        await batch.FillStyleAsync(newColor);
        await batch.FillRectAsync(cx + (line.x + _x) * len, cy + (line.y + _y) * len, 2, 2);

        if (Random.Shared.NextDouble() < sparkChance)
            await batch.FillRectAsync(
                cx + (line.x + _x) * len + Random.Shared.NextDouble() * sparkDist * (Random.Shared.NextDouble() < .5 ? 1 : -1) - sparkSize / 2,
                cy + (line.y + _y) * len + Random.Shared.NextDouble() * sparkDist * (Random.Shared.NextDouble() < .5 ? 1 : -1) - sparkSize / 2,
                sparkSize, sparkSize);
    }

    public async ValueTask DisposeAsync()
    {
        await _ctx.DisposeAsync();
    }

    public class Line
    {
        public int time;
        public int cumulativeTime;
        public string color = "";
        public double x, y, addedX, addedY, rad, targetTime, lightInputMultiplier;

        public Line()
        {
            Reset();
        }

        public void Reset()
        {
            x = y = addedX = addedY = rad = 0;
            lightInputMultiplier = baseLightInputMultiplier + addedLightInputMultiplier * Random.Shared.NextDouble();
            color = _color.Replace("hue", (tick * hueChange).ToString());
            cumulativeTime = 0;
            BeginPhase();
        }

        public void BeginPhase()
        {
            x += addedX;
            y += addedY;
            time = 0;
            targetTime = (int)(baseTime + addedTime * Random.Shared.NextDouble());
            rad += baseRad * (Random.Shared.NextDouble() < .5 ? 1 : -1);
            addedX = Math.Cos(rad);
            addedY = Math.Sin(rad);

            if (
                Random.Shared.NextDouble() < dieChance
                || x > dieX
                || x < -dieX
                || y > dieY
                || y < -dieY
            )
                Reset();
        }
    }
}