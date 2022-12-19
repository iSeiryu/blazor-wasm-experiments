namespace BlazorExperiments.UI.Models.SnakeGame;

public class Cell
{
    public Cell() { }
    public Cell(double x, double y) => (X, Y) = (x, y);

    public double X { get; set; }
    public double Y { get; set; }
}
