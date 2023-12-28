namespace BlazorExperiments.UI.Models.SnakeGame;

public class Snake(int size, int fieldWidth, int fieldHeight, float snakeSpeed) {
    readonly int _size = size;
    readonly int _xLimit = fieldWidth - size;
    readonly int _yLimit = fieldHeight - size;
    readonly float _snakeSpeed = snakeSpeed;
    SnakeDirection _currentDirection;

    float _xSpeed,
          _ySpeed;

    public BodyPart Head => Tail[^1];
    public List<BodyPart> Tail { get; } = [new(0, 0, 0, 0)];

    public void Update(double deltaTime) {
        if (Head.Interpolation == 1.0f)
            SnakeStep();

        for (var i = 0; i < Tail.Count; i++)
            Tail[i].Interpolate(deltaTime, _snakeSpeed);
    }

    void SnakeStep() {
        for (var i = 0; i < Tail.Count - 1; i++) {
            Tail[i] = Tail[i + 1];
            Tail[i].ResetInterp();
        }

        Tail[^1] = new(Head.Position.X + _xSpeed,
                       Head.Position.Y + _ySpeed,
                       Head.Position.X,
                       Head.Position.Y);

        if (Head.Position.X > _xLimit)
            Head.Position.X = 0;
        else if (Head.Position.X < 0)
            Head.Position.X = _xLimit;
        else if (Head.Position.Y > _yLimit)
            Head.Position.Y = 0;
        else if (Head.Position.Y < 0)
            Head.Position.Y = _yLimit;
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
        if (egg.X == Head.Position.X && egg.Y == Head.Position.Y) {
            Tail.Add(new(Head.Position.X, Head.Position.Y, Head.Position.X, Head.Position.Y));

            return true;
        }

        return false;
    }

    public bool IsDead() {
        for (var i = 0; i < Tail.Count - 1; i++)
            if (Head.Position.X == Tail[i].Position.X && Head.Position.Y == Tail[i].Position.Y)
                return true;

        return false;
    }
}
