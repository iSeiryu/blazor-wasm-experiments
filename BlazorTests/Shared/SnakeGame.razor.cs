using System;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using BlazorTests.Models.SnakeGame;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorTests.Shared {
    public partial class SnakeGame : IDisposable {
        private Canvas2DContext _context;
        private BECanvasComponent _canvas;
        private ElementReference _container;
        private Snake _snake;
        private Egg _egg;
        private int _cellSize = 0;
        private bool _gameOver = false;

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                _context = await _canvas.CreateCanvas2DAsync();
                await _container.FocusAsync();
                await InitAsync();
            }
        }

        private async Task InitAsync() {
            _cellSize = (int)_canvas.Width / 20;
            _egg = new Egg(_cellSize, (int)_canvas.Width, (int)_canvas.Height);
            _snake = new Snake(_cellSize, (int)_canvas.Width, (int)_canvas.Height);
            _gameOver = false;

            await GameLoopAsync();
        }

        private async Task GameLoopAsync() {
            if (_gameOver) return;

            if (_snake.Ate(_egg)) {
                _egg.NewLocation();
            }

            _snake.Update();
            await DrawAsync();

            if (_snake.IsDead())
                await GameOver();

            await Task.Delay(150);
            await GameLoopAsync();
        }

        private async Task DrawAsync() {
            await _context.BeginBatchAsync();

            await ClearScreenAsync();
            await _context.SetFillStyleAsync("white");
            await _context.SetFontAsync("12px serif");
            await _context.FillTextAsync("Score: " + _snake.Tail.Count, _canvas.Width - 55, 10);

            foreach (var cell in _snake.Tail) {
                await _context.FillRectAsync(cell.X, cell.Y, _cellSize, _cellSize);
            }

            await _context.SetFillStyleAsync("green");
            await _context.FillRectAsync(_snake.Head.X, _snake.Head.Y, _cellSize, _cellSize);

            await _context.SetFillStyleAsync("yellow");
            await _context.FillRectAsync(_egg.X, _egg.Y, _cellSize, _cellSize);

            await _context.EndBatchAsync();
        }

        private async Task HandleInput(KeyboardEventArgs e) {
            if (_gameOver)
                await InitAsync();

            else if (e.Code == "ArrowDown")  _snake.SetDirection(Direction.Down);
            else if (e.Code == "ArrowUp")    _snake.SetDirection(Direction.Up);
            else if (e.Code == "ArrowLeft")  _snake.SetDirection(Direction.Left);
            else if (e.Code == "ArrowRight") _snake.SetDirection(Direction.Right);
        }

        private TouchPoint _firstTouch = null;
        private async Task HandleTouchStart(TouchEventArgs e) {
            if (_gameOver)
                await InitAsync();

            _firstTouch = e.Touches.FirstOrDefault();
        }

        private void HandleTouchMove(TouchEventArgs e) {
            if (_firstTouch == null) return;

            var xDiff = _firstTouch.ClientX - e.Touches[0].ClientX;
            var yDiff = _firstTouch.ClientY - e.Touches[0].ClientY;

            /*most significant*/
            if ( Math.Abs( xDiff ) > Math.Abs( yDiff ) ) {
                _snake.SetDirection(xDiff > 0 ? Direction.Left : Direction.Right);
            } else {
                _snake.SetDirection(yDiff > 0 ? Direction.Up : Direction.Down);
            }

            _firstTouch = null;
        }

        private async Task ClearScreenAsync() {
            await _context.ClearRectAsync(0, 0, _canvas.Width, _canvas.Height);
            await _context.SetFillStyleAsync("black");
            await _context.FillRectAsync(0, 0, _canvas.Width, _canvas.Height);
        }

        private async Task GameOver() {
            _gameOver = true;

            await _context.SetFillStyleAsync("red");
            await _context.SetFontAsync("48px serif");
            await _context.FillTextAsync("Game Over", _canvas.Width / 4, _canvas.Height / 2);
        }

        public void Dispose() {
            _gameOver = true;
            _context.Dispose();
        }
    }
}
