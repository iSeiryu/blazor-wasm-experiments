using System.Timers;
using BlazorExperiments.UI.Shared;
using Excubo.Blazor.Canvas.Contexts;

namespace BlazorExperiments.UI.Pages;
public partial class GameOfLife {
    CanvasComponent _canvas = null!;
    bool[,] _board = null!;
    int _rows = 0;
    int _cols = 0;
    long _generation = 0;
    const int CellSize = 10;
    DateTime _lastTime = DateTime.Now;
    TimeSpan _refreshSpeedInMilliseconds = TimeSpan.Zero;
    double _milliSeconds = 400;

    void Initialize() {
        _rows = (int)_canvas.Height / CellSize;
        _cols = (int)_canvas.Width / CellSize;
        _board = new bool[_rows, _cols];
        _generation = 0;
        StateHasChanged();

        AddGliderShape(ref _board);
        AddExploderShape(ref _board, 10, 0);
        AddStableShape(ref _board, 10, 10);
        if (_cols < 35) {
            AddPulsarShape(ref _board, 20, 10);
            AddTetrominoShape(ref _board, 35, 20);
        }
        else {
            AddPulsarShape(ref _board, 20, 20);
            AddTetrominoShape(ref _board, 10, 30);
        }

        _refreshSpeedInMilliseconds = TimeSpan.FromMilliseconds(_milliSeconds);
        _canvas.Timer.Enabled = true;

        StateHasChanged();
    }

    async ValueTask Loop(ElapsedEventArgs elapsedEvent) {
        if (elapsedEvent.SignalTime - _lastTime < _refreshSpeedInMilliseconds) {
            return;
        }
        _lastTime = elapsedEvent.SignalTime;

        await using var batch = _canvas.Context.CreateBatch();
        await ClearScreenAsync(batch);
        await batch.FillStyleAsync("white");

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

                if (newBoard[row, col]) {
                    await batch.FillRectAsync(col * CellSize, row * CellSize, CellSize, CellSize);
                }

            }
        }
        _board = newBoard;
        if (!boardHasChanged)
            Pause();

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

    void Pause() {
        _canvas.Timer.Enabled = false;
    }

    async Task ClearScreenAsync(Batch2D batch) {
        await batch.ClearRectAsync(0, 0, _canvas.Width, _canvas.Height);
        await batch.FillStyleAsync("black");
        await batch.FillRectAsync(0, 0, _canvas.Width, _canvas.Height);
    }

    void AddGliderShape(ref bool[,] board) {
        board[1, 1] = true;
        board[1, 2] = true;
        board[1, 3] = true;
        board[2, 1] = true;
        board[3, 2] = true;
    }

    void AddExploderShape(ref bool[,] board, int row = 0, int col = 0) {
        board[row, col + 2] = true;
        board[row + 1, col] = true;
        board[row + 1, col + 2] = true;
        board[row + 2, col] = true;
        board[row + 2, col + 1] = true;
        board[row + 2, col + 2] = true;
        board[row + 3, col + 2] = true;
    }

    void AddStableShape(ref bool[,] board, int row = 0, int col = 0) {
        board[row, col] = true;
        board[row + 1, col] = true;
        board[row, col + 1] = true;
        board[row, col + 2] = true;
    }

    void AddTetrominoShape(ref bool[,] board, int row = 0, int col = 0) {
        board[row, col] = true;
        board[row, col + 1] = true;
        board[row, col + 2] = true;
        board[row - 1, col + 1] = true;
    }

    void AddPulsarShape(ref bool[,] board, int row = 0, int col = 0) {
        board[row, col + 2] = true;
        board[row, col + 3] = true;
        board[row, col + 4] = true;
        board[row, col + 8] = true;
        board[row, col + 9] = true;
        board[row, col + 10] = true;
        board[row + 2, col] = true;
        board[row + 2, col + 5] = true;
        board[row + 2, col + 7] = true;
        board[row + 2, col + 12] = true;
        board[row + 3, col] = true;
        board[row + 3, col + 5] = true;
        board[row + 3, col + 7] = true;
        board[row + 3, col + 12] = true;
        board[row + 4, col] = true;
        board[row + 4, col + 5] = true;
        board[row + 4, col + 7] = true;
        board[row + 4, col + 12] = true;
        board[row + 5, col + 2] = true;
        board[row + 5, col + 3] = true;
        board[row + 5, col + 4] = true;
        board[row + 5, col + 8] = true;
        board[row + 5, col + 9] = true;
        board[row + 5, col + 10] = true;
        board[row + 7, col + 2] = true;
        board[row + 7, col + 3] = true;
        board[row + 7, col + 4] = true;
        board[row + 7, col + 8] = true;
        board[row + 7, col + 9] = true;
        board[row + 7, col + 10] = true;
        board[row + 8, col] = true;
        board[row + 8, col + 5] = true;
        board[row + 8, col + 7] = true;
        board[row + 8, col + 12] = true;
    }
}
