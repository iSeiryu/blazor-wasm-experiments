using System.Timers;
using BlazorExperiments.UI.Components;
using BlazorExperiments.UI.Shared;
using Excubo.Blazor.Canvas.Contexts;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorExperiments.UI.Pages;
public partial class GameOfLife {
    CanvasComponent _canvas = null!;
    Button _button = null!;
    bool[,] _board = null!;
    int _rows = 0;
    int _cols = 0;
    long _generation = 0;
    bool _gameStopped = true;
    const int Padding = 2;
    const int CellSize = 30;
    const int ButtonSize = CellSize - Padding;
    DateTime _lastTime = DateTime.Now;
    TimeSpan _refreshSpeedInMilliseconds = TimeSpan.Zero;
    double _milliSeconds = 400;

    async Task Initialize() {
        _refreshSpeedInMilliseconds = TimeSpan.FromMilliseconds(_milliSeconds);
        _rows = (int)(_canvas.Height) / CellSize;
        _cols = (int)(_canvas.Width) / CellSize;
        _board = new bool[_rows, _cols];
        _generation = 0;
        _button = new Button("Start", 10, 10, Start);
        _button.Show();
        await DrawGrid();
        await DrawButton();

        StateHasChanged();

        //AddGliderShape(ref _board);
        //AddExploderShape(ref _board, 10, 0);
        //AddStableShape(ref _board, 10, 10);
        //if (_cols < 35) {
        //    AddPulsarShape(ref _board, 20, 10);
        //    AddTetrominoShape(ref _board, 35, 20);
        //}
        //else {
        //    AddPulsarShape(ref _board, 20, 20);
        //    AddTetrominoShape(ref _board, 10, 30);
        //}

        //_canvas.Timer.Enabled = true;

        //StateHasChanged();
    }

    void ChangeSpeed() {
        _refreshSpeedInMilliseconds = TimeSpan.FromMilliseconds(_milliSeconds);
    }

    async ValueTask Loop(ElapsedEventArgs elapsedEvent) {
        if (_gameStopped) {
            return;
        }

        if (elapsedEvent.SignalTime - _lastTime < _refreshSpeedInMilliseconds) {
            return;
        }
        _lastTime = elapsedEvent.SignalTime;

        var newBoard = new bool[_rows, _cols];
        var boardHasChanged = false;
        _generation++;

        for (int row = 0; row < _rows; row++) {
            for (int col = 0; col < _cols; col++) {
                int liveNeighbors = GetLiveNeighbors(col, row);

                if (_board[row, col] && (liveNeighbors < 2 || liveNeighbors > 3)) {
                    boardHasChanged = true;
                    newBoard[row, col] = false;
                }
                else if (!_board[row, col] && liveNeighbors == 3) {
                    boardHasChanged = true;
                    newBoard[row, col] = true;
                }
                else {
                    newBoard[row, col] = _board[row, col];
                }
            }
        }

        _board = newBoard;
        await DrawGrid();

        if (!boardHasChanged)
            await Stop();

        StateHasChanged();
    }

    int GetLiveNeighbors(int x, int y) {
        int liveNeighbors = 0;

        // Check the 8 surrounding cells
        for (int row = -1; row <= 1; row++) {
            for (int col = -1; col <= 1; col++) {
                // Skip the cell itself
                if (row == 0 && col == 0)
                    continue;

                int newRow = y + row;
                int newCol = x + col;

                // Check if the coordinates are within the board boundaries
                if (newRow >= 0 && newRow < _rows && newCol >= 0 && newCol < _cols) {
                    // If the neighbor is alive, increment the count
                    if (_board[newRow, newCol])
                        liveNeighbors++;
                }
            }
        }
        return liveNeighbors;
    }

    async ValueTask DrawGrid() {
        await using var batch = _canvas.Context.CreateBatch();
        await ClearScreenAsync(batch);
        await SetDefaultScreenProperties(batch);

        for (int row = 0; row < _rows; row++) {
            for (int col = 0; col < _cols; col++) {
                var x = col * CellSize;
                var y = row * CellSize;

                if (_board[row, col]) {
                    await batch.FillRectAsync(x, y, ButtonSize, ButtonSize);
                }

                await batch.StrokeRectAsync(x, y, ButtonSize, ButtonSize);
            }
        }
    }

    public async ValueTask DrawButton(bool mouseOver = false) {
        await using var batch = _canvas.Context.CreateBatch();
        await batch.FillStyleAsync(mouseOver ? "green" : "blue");
        await batch.FillRectAsync(_button.X, _button.Y, _button.Width, _button.Height);
        await batch.FillStyleAsync("white");
        await batch.ShadowBlurAsync(10);
        await batch.FillTextAsync(_button.Text, _button.X + 5, _button.Y + 15);
    }

    void Start() {
        _gameStopped = false;
        _button.Hide();
    }

    async ValueTask Stop() {
        _gameStopped = true;
        _button.Show();
        await DrawGrid();
        await DrawButton();
    }

    async ValueTask HandleMouse(MouseEventArgs e) {
        if (_button.IsStartButtonClicked(e.OffsetX, e.OffsetY)) {
            Start();
            return;
        }

        var x = (int)(e.OffsetX / CellSize);
        var y = (int)(e.OffsetY / CellSize);
        var newValue = !_board[y, x];
        _board[y, x] = newValue;

        if (newValue) {
            await using var batch = _canvas.Context.CreateBatch();
            await SetDefaultScreenProperties(batch);
            await batch.FillRectAsync(x * CellSize, y * CellSize, ButtonSize, ButtonSize);
            await batch.StrokeRectAsync(x * CellSize, y * CellSize, ButtonSize, ButtonSize);
        }
    }

    async ValueTask ClearScreenAsync(Batch2D batch) {
        await batch.ClearRectAsync(0, 0, _canvas.Width, _canvas.Height);
        await batch.FillStyleAsync("rgba(5, 39, 103, 1)");
        await batch.FillRectAsync(0, 0, _canvas.Width, _canvas.Height);
    }

    async ValueTask SetDefaultScreenProperties(Batch2D batch) {
        await batch.FillStyleAsync("lightgreen");
        await batch.ShadowColorAsync("rgba(10, 10, 10, 1)");
        await batch.ShadowOffsetXAsync(2);
        await batch.ShadowOffsetYAsync(2);
        await batch.ShadowBlurAsync(0);
    }

    //void AddGliderShape(ref bool[,] board) {
    //    board[1, 1] = true;
    //    board[1, 2] = true;
    //    board[1, 3] = true;
    //    board[2, 1] = true;
    //    board[3, 2] = true;
    //}

    //void AddExploderShape(ref bool[,] board, int row = 0, int col = 0) {
    //    board[row, col + 2] = true;
    //    board[row + 1, col] = true;
    //    board[row + 1, col + 2] = true;
    //    board[row + 2, col] = true;
    //    board[row + 2, col + 1] = true;
    //    board[row + 2, col + 2] = true;
    //    board[row + 3, col + 2] = true;
    //}

    //void AddStableShape(ref bool[,] board, int row = 0, int col = 0) {
    //    board[row, col] = true;
    //    board[row + 1, col] = true;
    //    board[row, col + 1] = true;
    //    board[row, col + 2] = true;
    //}

    //void AddTetrominoShape(ref bool[,] board, int row = 0, int col = 0) {
    //    board[row, col] = true;
    //    board[row, col + 1] = true;
    //    board[row, col + 2] = true;
    //    board[row - 1, col + 1] = true;
    //}

    //void AddPulsarShape(ref bool[,] board, int row = 0, int col = 0) {
    //    board[row, col + 2] = true;
    //    board[row, col + 3] = true;
    //    board[row, col + 4] = true;
    //    board[row, col + 8] = true;
    //    board[row, col + 9] = true;
    //    board[row, col + 10] = true;
    //    board[row + 2, col] = true;
    //    board[row + 2, col + 5] = true;
    //    board[row + 2, col + 7] = true;
    //    board[row + 2, col + 12] = true;
    //    board[row + 3, col] = true;
    //    board[row + 3, col + 5] = true;
    //    board[row + 3, col + 7] = true;
    //    board[row + 3, col + 12] = true;
    //    board[row + 4, col] = true;
    //    board[row + 4, col + 5] = true;
    //    board[row + 4, col + 7] = true;
    //    board[row + 4, col + 12] = true;
    //    board[row + 5, col + 2] = true;
    //    board[row + 5, col + 3] = true;
    //    board[row + 5, col + 4] = true;
    //    board[row + 5, col + 8] = true;
    //    board[row + 5, col + 9] = true;
    //    board[row + 5, col + 10] = true;
    //    board[row + 7, col + 2] = true;
    //    board[row + 7, col + 3] = true;
    //    board[row + 7, col + 4] = true;
    //    board[row + 7, col + 8] = true;
    //    board[row + 7, col + 9] = true;
    //    board[row + 7, col + 10] = true;
    //    board[row + 8, col] = true;
    //    board[row + 8, col + 5] = true;
    //    board[row + 8, col + 7] = true;
    //    board[row + 8, col + 12] = true;
    //}
}
