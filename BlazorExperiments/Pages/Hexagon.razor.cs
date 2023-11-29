using BlazorExperiments.UI.Services;
using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;
using Timer = System.Timers.Timer;

namespace BlazorExperiments.UI.Pages;

public partial class Hexagon : IAsyncDisposable {
    Canvas _canvas;
    Context2D _ctx;
    Timer _timer;

    double _width = 400, _height = 400;
    string _style = "";
    double _devicePixelRatio = 1;
    const int HeightBuffer = 50;
    const int MediaMinWidth = 641;
    const int SideBarWidth = 250;

    int Count { get; set; } = 100;
    double Len { get; set; } = 20;

    const double BaseTime = 10, AddedTime = 10,
           DieChance = .02, SparkChance = .02,
           SparkDist = 10, SparkSize = 1,

           BaseLight = 50, AddedLight = 10,
           ShadowToTimePropMult = 6,
           BaseLightInputMultiplier = .01, AddedLightInputMultiplier = .02,

           HueChange = .1;

    static double _tick = 0;

    const double BaseRad = Math.PI * 2 / 6;
    static double _cx, _cy, _dieX, _dieY;
    readonly List<Line> _lines = [];

    DateTime _lastTime = DateTime.Now;
    int _timePassed = 0;

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
            _ctx = await _canvas.GetContext2DAsync(alpha: false);
            await _ctx.ScaleAsync(_devicePixelRatio, _devicePixelRatio);
            Initialize();

            _timer = new Timer(1_000 / 60);
            _timer.Elapsed += async (_, _) => await Loop();
            _timer.Enabled = true;
        }
    }

    public void Initialize() {
        _cx = _width / 2;
        _cy = _height / 2;

        _dieX = _width / 2 / Len;
        _dieY = _height / 2 / Len;

        _lines.Clear();
        for (int i = 0; i < Count; i++)
            _lines.Add(new());
    }

    private async ValueTask Loop() {
        _tick++;
        await using var batch = _ctx.CreateBatch();
        await batch.GlobalCompositeOperationAsync(CompositeOperation.Source_Over);
        await batch.ShadowBlurAsync(0);
        await batch.FillStyleAsync("rgba(0,0,0,0.04)");
        await batch.FillRectAsync(0, 0, _width, _height);
        await batch.GlobalCompositeOperationAsync(CompositeOperation.Lighter);

        foreach (var line in _lines)
            await Step(line, batch);

        await DrawFps(batch);
    }

    async ValueTask DrawFps(Batch2D batch) {
        if (_timePassed > 50) {
            var fps = 1 / (DateTime.Now - _lastTime).TotalSeconds;
            _timePassed = 0;

            await batch.ClearRectAsync(0, 0, 120, 30);
            await batch.FontAsync("bold 20px Arial");
            await batch.FillTextAsync($"{fps:F} FPS", 10, 20);
        }
        _lastTime = DateTime.Now;
        _timePassed++;
    }

    async ValueTask Step(Line line, Batch2D batch) {
        line.Time++;
        line.CumulativeTime++;

        if (line.Time >= line.TargetTime)
            line.BeginPhase();

        double prop = line.Time / line.TargetTime;
        double wave = Math.Sin(prop * Math.PI / 2);
        double x = line.AddedX * wave;
        double y = line.AddedY * wave;

        var light = BaseLight + AddedLight * Math.Sin(line.CumulativeTime * line.LightInputMultiplier);
        var newColor = $"hsl({line.Hue},100%,{light}%)";

        await batch.ShadowBlurAsync(prop * ShadowToTimePropMult);
        await batch.ShadowColorAsync(newColor);
        await batch.FillStyleAsync(newColor);
        await batch.FillRectAsync(_cx + (line.X + x) * Len, _cy + (line.Y + y) * Len, 2, 2);

        if (Random.Shared.NextDouble() < SparkChance) {
            var rand = Random.Shared.NextDouble() * SparkDist * (Random.Shared.NextDouble() < .5 ? 1 : -1) - SparkSize / 2;
            await batch.FillRectAsync(
                _cx + (line.X + x) * Len + rand,
                _cy + (line.Y + y) * Len + rand,
                SparkSize,
                SparkSize);
        }
    }

    public async ValueTask DisposeAsync() {
        _timer.Dispose();
        await _ctx.DisposeAsync();
    }

    public sealed class Line {
        public int Time;
        public int CumulativeTime;
        public double Hue;
        public double X, Y, AddedX, AddedY, TargetTime, LightInputMultiplier;
        double _rad;

        public Line() {
            Reset();
        }

        public void Reset() {
            X = Y = AddedX = AddedY = _rad = 0;
            LightInputMultiplier = BaseLightInputMultiplier + AddedLightInputMultiplier * Random.Shared.NextDouble();
            Hue = _tick * HueChange;
            CumulativeTime = 0;
            BeginPhase();
        }

        public void BeginPhase() {
            var rand = Random.Shared.NextDouble();
            X += AddedX;
            Y += AddedY;
            Time = 0;
            TargetTime = (int)(BaseTime + AddedTime * Random.Shared.NextDouble());
            _rad += BaseRad * (rand < .5 ? 1 : -1);
            AddedX = Math.Cos(_rad);
            AddedY = Math.Sin(_rad);

            if (rand <= DieChance
                || X > _dieX
                || X < -_dieX
                || Y > _dieY
                || Y < -_dieY
            )
                Reset();
        }
    }
}