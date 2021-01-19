using System;
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
        private int _size = 0;
        private bool _gameOver = false;

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                _size = (int)_canvas.Width / 20;
                _egg = new Egg(_size, (int) _canvas.Width, (int) _canvas.Height);
                _snake = new Snake(_size, (int) _canvas.Width, (int) _canvas.Height);
                _context = await _canvas.CreateCanvas2DAsync();
                
                await _container.FocusAsync();
                await GameLoopAsync();
            }
        }

        private async Task GameLoopAsync() {
            if (_gameOver) return;
            if (_egg.X == _snake.X && _egg.Y == _snake.Y) {
                _snake.Eat();
                _egg.NewLocation();
            }
            _snake.Update();
            await DrawAsync();
            
            await Task.Delay(150);
            await GameLoopAsync();
        }

        private async Task DrawAsync() {
            await _context.BeginBatchAsync();
            
            await ClearScreen();
            await _context.SetFillStyleAsync("white");
            await _context.SetFontAsync("12px serif");
            await _context.FillTextAsync("Score: " + _snake.Total, _canvas.Width - 55, 10);
            
            await _context.FillRectAsync(_snake.X, _snake.Y, _size, _size);
            await _context.SetFillStyleAsync("red");
            await _context.FillRectAsync(_egg.X, _egg.Y, _size, _size);
            
            await _context.EndBatchAsync();
        }

        private void HandleInput(KeyboardEventArgs e) {
            if (e.Code == "ArrowDown" ||
                e.Code == "ArrowUp"   ||
                e.Code == "ArrowLeft" ||
                e.Code == "ArrowRight") {
                _snake.SetDirection(e.Code.Replace("Arrow", ""));
            }
        }

        private async Task ClearScreen() {
            await _context.ClearRectAsync(0, 0, _canvas.Width, _canvas.Height);
            await _context.SetFillStyleAsync("black");
            await _context.FillRectAsync(0, 0, _canvas.Width, _canvas.Height);
        }

        public void Dispose() {
            _gameOver = true;
            _context.Dispose();
        }
    }
}
