namespace BlazorExperiments.UI.Models.SnakeGame;

public class Snake {
    private readonly int _size;
    private readonly int _xLimit;
    private readonly int _yLimit;
    private SnakeDirection _currentDirection;

    private double _xSpeed,
                   _ySpeed;

    public Snake(int size, int fieldWidth, int fieldHeight) {
        _size = size;
        _xLimit = fieldWidth - size;
        _yLimit = fieldHeight - size;
    }

    public Cell Head => Tail[^1];
    public List<Cell> Tail { get; } = new() { new Cell(0, 0) };

    public void Update() {
        for (var i = 0; i < Tail.Count - 1; i++)
            Tail[i] = Tail[i + 1];

        Tail[^1] = new Cell(Head.X, Head.Y);

        Head.X += _xSpeed;
        Head.Y += _ySpeed;

        if (Head.X > _xLimit)
            Head.X = 0;
        else if (Head.X < 0)
            Head.X = _xLimit;
        else if (Head.Y > _yLimit)
            Head.Y = 0;
        else if (Head.Y < 0)
            Head.Y = _yLimit;
    }

    public void SetDirection(SnakeDirection snakeDirection) {
        //check if the new direction is the opposite of the current direction
        if (Tail.Count > 1 &&
            (
                snakeDirection == SnakeDirection.Up && _currentDirection == SnakeDirection.Down ||
                snakeDirection == SnakeDirection.Down && _currentDirection == SnakeDirection.Up ||
                snakeDirection == SnakeDirection.Left && _currentDirection == SnakeDirection.Right ||
                snakeDirection == SnakeDirection.Right && _currentDirection == SnakeDirection.Left)
            )
            return;

        _currentDirection = snakeDirection;
        switch (snakeDirection) {
            case SnakeDirection.Up:
                _xSpeed = 0;
                _ySpeed = -_size;
                break;
            case SnakeDirection.Down:
                _xSpeed = 0;
                _ySpeed = _size;
                break;
            case SnakeDirection.Left:
                _xSpeed = -_size;
                _ySpeed = 0;
                break;
            case SnakeDirection.Right:
                _xSpeed = _size;
                _ySpeed = 0;
                break;
        }
    }

    public bool Ate(Egg egg) {
        if (egg.X == Head.X && egg.Y == Head.Y) {
            var last = Tail.Last();
            Tail.Add(new Cell(last.X, last.Y));

            return true;
        }

        return false;
    }

    public bool IsDead() {
        for (var i = 0; i < Tail.Count - 1; i++)
            if (Head.X == Tail[i].X && Head.Y == Tail[i].Y)
                return true;

        return false;
    }
}
