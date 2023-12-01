using System.Timers;
using BlazorExperiments.UI.Shared;
using Excubo.Blazor.Canvas;
using Excubo.Blazor.Canvas.Contexts;

namespace BlazorExperiments.UI.Pages;

public partial class Hexagon {
    CanvasComponent _canvas = null!;

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

    public void Initialize() {
        _canvas.Timer.Enabled = false;
        _cx = _canvas.Width / 2;
        _cy = _canvas.Height / 2;

        _dieX = _canvas.Width / 2 / Len;
        _dieY = _canvas.Height / 2 / Len;

        _lines.Clear();
        for (int i = 0; i < Count; i++)
            _lines.Add(new());

        _canvas.Timer.Enabled = true;
        StateHasChanged();
    }

    private async ValueTask Loop(ElapsedEventArgs elapsedEvent) {
        _tick++;
        await using var batch = _canvas.Context.CreateBatch();
        await batch.GlobalCompositeOperationAsync(CompositeOperation.Source_Over);
        await batch.ShadowBlurAsync(0);
        await batch.FillStyleAsync("rgba(0,0,0,0.04)");
        await batch.FillRectAsync(0, 0, _canvas.Width, _canvas.Height);
        await batch.GlobalCompositeOperationAsync(CompositeOperation.Lighter);

        foreach (var line in _lines)
            await Step(line, batch);

        await _canvas.DrawFps(batch, elapsedEvent);
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