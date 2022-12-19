using System;

namespace BlazorExperiments.UI.Models.SnakeGame;

public class Egg : Cell
{
    private readonly int _size;
    private readonly int _xLimit;
    private readonly int _yLimit;
    private readonly Random _random = new();

    public Egg(int size, int xLimit, int yLimit)
    {
        _size = size;
        _xLimit = xLimit;
        _yLimit = yLimit;

        NewLocation();
    }

    public void NewLocation()
    {
        X = _random.Next(0, _xLimit) / _size * _size;
        Y = _random.Next(0, _yLimit) / _size * _size;
    }
}
