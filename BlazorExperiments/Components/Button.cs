using Excubo.Blazor.Canvas.Contexts;

namespace BlazorExperiments.UI.Components;

public class Button(string text, double x, double y, Action onClick) {
    public string Text { get; } = text;
    public Action OnClick { get; } = onClick;
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Width { get; private set; }
    public double Height { get; private set; }

    public async ValueTask Draw(Batch2D batch) {
        await batch.FillStyleAsync("white");
        await batch.FillRectAsync(X, Y, Width, Height);
        await batch.FillStyleAsync("black");
        await batch.FillTextAsync(Text, X + 5, Y + 15);
    }

    internal void Show() {
        Width = 30;
        Height = 25;
    }

    internal void Hide() {
        Width = 0;
        Height = 0;
    }
}
