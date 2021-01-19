using System.Collections.Generic;

namespace BlazorTests.Models.SnakeGame {
    public class Snake {
        private readonly int _size;
        private readonly int _xLimit;
        private readonly int _yLimit;

        private double _xSpeed,
                       _ySpeed;
        private readonly List<int> _tail = new();

        public Snake(int size, int fieldWidth, int fieldHeight) {
            _size = size;
            _xLimit = fieldWidth - size;
            _yLimit = fieldHeight - size;
        }
        
        public double X { get; private set; }
        public double Y { get; private set; }
        public int Total { get; private set; } = 1;

        public void Update() {
            if (X > _xLimit)
                X = 0;
            else if (X < 0)
                X = _xLimit;
            else if (Y > _yLimit)
                Y = 0;
            else if (Y < 0)
                Y = _yLimit;
            
            X += _xSpeed;
            Y += _ySpeed;
        }

        public void SetDirection(string direction) {
            switch (direction) {
                case "Up":
                    _xSpeed = 0;
                    _ySpeed = -_size;
                    break;
                case "Down":
                    _xSpeed = 0;
                    _ySpeed = _size;
                    break;
                case "Left":
                    _xSpeed = -_size;
                    _ySpeed = 0;
                    break;
                case "Right":
                    _xSpeed = _size;
                    _ySpeed = 0;
                    break;
            }
        }

        public void Eat() {
            Total++;
        }
    }
}
