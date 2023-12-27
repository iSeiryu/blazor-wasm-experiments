namespace BlazorExperiments.UI.Components;

public class Button(string text, double x, double y, Action onClick) {
    public string Text { get; } = text;
    public Action OnClick { get; } = onClick;
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Width { get; private set; }
    public double Height { get; private set; }

    internal void Show() {
        Width = 30;
        Height = 25;
    }

    internal void Hide() {
        Width = 0;
        Height = 0;
    }

    internal bool IsStartButtonClicked(double x, double y) {
        return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
    }
}
