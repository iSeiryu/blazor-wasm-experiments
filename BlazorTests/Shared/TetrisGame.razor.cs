using System;
using System.Threading.Tasks;
using System.Timers;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorTests.Shared {
    public partial class TetrisGame : IDisposable {
        private Canvas2DContext _context;
        private static Timer _timer;
        private double x, y = 50;
        protected BECanvasComponent _canvasReference;
        private ElementReference _container;

        protected override async Task OnAfterRenderAsync(bool firstRender) {
            if (firstRender) {
                _context = await _canvasReference.CreateCanvas2DAsync();

                await _context.SetFontAsync("48px serif");
                await _context.StrokeTextAsync("current y: " + y, 100, 50);

                _timer = new Timer(10);
                _timer.Elapsed += async (_, _) => { await DrawAsync(); };
                _timer.Enabled = true;
                
                await _container.FocusAsync();
            }
        }

        public void Move(KeyboardEventArgs e) {
            if (e.Code == "ArrowDown") {
                y += 10;
            }
            else if (e.Code == "ArrowUp") {
                y -= 10;
            }
            else if (e.Code == "ArrowLeft") {
                x -= 10;
            }
            else if (e.Code == "ArrowRight") {
                x += 10;
            }
        }

        private async Task DrawAsync() {
            await _context.BeginBatchAsync();

            await _context.ClearRectAsync(0, 0, 600, 400);
            await _context.StrokeTextAsync(DateTime.Now.ToString("mm:ss:FFF"), x, y);
            await _context.StrokeRectAsync(75, 140, 150, 110);
            await _context.FillRectAsync(130, 190, 40, 60);

            await _context.BeginPathAsync();
            await _context.MoveToAsync(50, 140);
            await _context.LineToAsync(150, 60);
            await _context.LineToAsync(250, 140);
            await _context.ClosePathAsync();
            await _context.StrokeAsync();

            await _context.EndBatchAsync();
        }

        public void Dispose() {
            _timer.Dispose();
            _context.Dispose();
        }
    }
}
