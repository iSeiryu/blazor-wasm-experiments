using System.Numerics;
using System.Runtime.CompilerServices;

namespace BlazorExperiments.UI.Models.SnakeGame;

public class Snake {
    readonly int _size;
    readonly int _xLimit;
    readonly int _yLimit;
    float _snakeSpeed;
    Vector2 _currentDirection;

    float _xSpeed,
          _ySpeed;

    public Snake(int size, int fieldWidth, int fieldHeight, float snakeSpeed) {
        _size = size;
        _xLimit = fieldWidth - size;
        _yLimit = fieldHeight - size;
        _snakeSpeed = snakeSpeed;
        _currentDirection = Vector2.UnitX; //right
        _xSpeed = _size;

        var head = new BodyPart(_size * 5, _size, _currentDirection);
        Body = [head];

        for (var i = 0; i < 7; i++) {
            var part = new BodyPart(_size * 5, _size, _currentDirection);
            Body.Add(part);

            part.Position.X = Tail.Position.X - _size;
            part.PrevPosition = new Vector2(part.Position.X, part.Position.Y);
            part.Direction = _currentDirection;
        }
    }

    public BodyPart Head => Body[^1];
    public BodyPart Tail => Body[0];
    public List<BodyPart> Body { get; }
    public void IncreaseSnakeSpeed() => _snakeSpeed += 1;

    public void Animate(double deltaTime) {
        for (var i = 0; i < Body.Count; i++) {
            Body[i].Animate(deltaTime, _snakeSpeed);
        }
    }

    public void SnakeStep() {
        for (var i = 0; i < Body.Count - 1; i++) {
            Body[i] = Body[i + 1];
            Body[i].ResetInterp();
        }

        Body[^1] = new(Head.Position.X + _xSpeed,
                       Head.Position.Y + _ySpeed,
                       _currentDirection);
    }

    public void SetDirection(Vector2 snakeDirection) {
        //check if the new direction is the opposite of the current direction
        //if (
        //    Body.Count > 1 &&
        //       (
        //        snakeDirection == SnakeDirection.Up && _currentDirection == Vector2.UnitY ||
        //        snakeDirection == SnakeDirection.Down && _currentDirection == SnakeDirection.Up ||
        //        snakeDirection == SnakeDirection.Left && _currentDirection == SnakeDirection.Right ||
        //        snakeDirection == SnakeDirection.Right && _currentDirection == SnakeDirection.Left
        //       ) ||
        //    snakeDirection == _currentDirection
        //   )
        //    return;

        _currentDirection = snakeDirection;
        _xSpeed = _currentDirection.X * _size;
        _ySpeed = _currentDirection.Y * _size;
    }

    public bool Ate(Egg egg) {
        if (egg.X == Head.Position.X && egg.Y == Head.Position.Y) {
            Body.Insert(0, new(Tail.PrevPosition.X, Tail.PrevPosition.Y, Tail.Direction));

            return true;
        }

        return false;
    }

    public bool IsDead() {
        for (var i = 0; i < Body.Count - 1; i++) {
            if (Head.Position.X < 0 || Head.Position.X > _xLimit || Head.Position.Y < 0 || Head.Position.Y > _yLimit)
                return true;
            if (Head.Position.X == Body[i].Position.X && Head.Position.Y == Body[i].Position.Y)
                return true;
        }

        return false;
    }
}
