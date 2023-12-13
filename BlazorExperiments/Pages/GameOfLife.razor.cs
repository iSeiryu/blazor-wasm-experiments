using System.Timers;
using BlazorExperiments.UI.Shared;
using Excubo.Blazor.Canvas.Contexts;

namespace BlazorExperiments.UI.Pages;
public partial class GameOfLife {
    CanvasComponent _canvas = null!;
    int[,] _board = null!;
    int _rows = 0;
    int _cols = 0;
    const int CellSize = 10;
    DateTime _lastTime = DateTime.Now;
    TimeSpan _refreshSpeedInMilliseconds = TimeSpan.Zero;
    double _milliSeconds = 400;

    void Initialize() {
        _rows = (int)_canvas.Height / CellSize;
        _cols = (int)_canvas.Width / CellSize;
        _board = new int[_rows, _cols];
        StateHasChanged();

        AddGliderShape(_board);
        AddExploderShape(_board, 10, 0);
        AddStableShape(_board, 10, 10);
        if (_cols < 35) {
            AddPulsarShape(_board, 20, 10);
            AddTetrominoShape(_board, 35, 20);
        }
        else {
            AddPulsarShape(_board, 20, 20);
            AddTetrominoShape(_board, 10, 30);
        }

        _refreshSpeedInMilliseconds = TimeSpan.FromMilliseconds(_milliSeconds);

        StateHasChanged();
    }

    async ValueTask Loop(ElapsedEventArgs elapsedEvent) {
        if (elapsedEvent.SignalTime - _lastTime < _refreshSpeedInMilliseconds) {
            return;
        }
        _lastTime = elapsedEvent.SignalTime;

        await using var batch = _canvas.Context.CreateBatch();
        await ClearScreenAsync(batch);
        int[,] newBoard = new int[_rows, _cols];

        for (int row = 0; row < _rows; row++) {
            for (int col = 0; col < _cols; col++) {
                int liveNeighbors = 0;
                liveNeighbors = GetLiveNeighbors(col, row);

                if (_board[row, col] == 1 && (liveNeighbors < 2 || liveNeighbors > 3)) {
                    newBoard[row, col] = 0;
                }
                else if (_board[row, col] == 0 && liveNeighbors == 3) {
                    newBoard[row, col] = 1;
                }
                else {
                    newBoard[row, col] = _board[row, col];
                }

                if (newBoard[row, col] == 1) {
                    await batch.FillStyleAsync("white");
                }
                else {
                    await batch.FillStyleAsync("black");
                }

                await batch.FillRectAsync(col * CellSize, row * CellSize, CellSize, CellSize);
            }
        }
        _board = newBoard;
    }
    private int GetLiveNeighbors(int x, int y) {
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
                if (newRow >= 0 && newRow < _board.GetLength(0) && newCol >= 0 && newCol < _board.GetLength(1)) {
                    // If the neighbor is alive, increment the count
                    if (_board[newRow, newCol] == 1)
                        liveNeighbors++;
                }
            }
        }
        return liveNeighbors;
    }

    async Task ClearScreenAsync(Batch2D batch) {
        await batch.ClearRectAsync(0, 0, _canvas.Width, _canvas.Height);
        await batch.FillStyleAsync("black");
        await batch.FillRectAsync(0, 0, _canvas.Width, _canvas.Height);
    }

    void AddGliderShape(int[,] board) {
        board[1, 1] = 1;
        board[1, 2] = 1;
        board[1, 3] = 1;
        board[2, 1] = 1;
        board[3, 2] = 1;
    }

    void AddExploderShape(int[,] board, int row = 0, int col = 0) {
        board[row, col + 2] = 1;
        board[row + 1, col] = 1;
        board[row + 1, col + 2] = 1;
        board[row + 2, col] = 1;
        board[row + 2, col + 1] = 1;
        board[row + 2, col + 2] = 1;
        board[row + 3, col + 2] = 1;
    }

    void AddStableShape(int[,] board, int row = 0, int col = 0) {
        board[row, col] = 1;
        board[row + 1, col] = 1;
        board[row, col + 1] = 1;
        board[row, col + 2] = 1;
    }

    void AddTetrominoShape(int[,] board, int row = 0, int col = 0) {
        board[row, col] = 1;
        board[row, col + 1] = 1;
        board[row, col + 2] = 1;
        board[row - 1, col + 1] = 1;
    }

    void AddPulsarShape(int[,] board, int row = 0, int col = 0) {
        board[row, col + 2] = 1;
        board[row, col + 3] = 1;
        board[row, col + 4] = 1;
        board[row, col + 8] = 1;
        board[row, col + 9] = 1;
        board[row, col + 10] = 1;
        board[row + 2, col] = 1;
        board[row + 2, col + 5] = 1;
        board[row + 2, col + 7] = 1;
        board[row + 2, col + 12] = 1;
        board[row + 3, col] = 1;
        board[row + 3, col + 5] = 1;
        board[row + 3, col + 7] = 1;
        board[row + 3, col + 12] = 1;
        board[row + 4, col] = 1;
        board[row + 4, col + 5] = 1;
        board[row + 4, col + 7] = 1;
        board[row + 4, col + 12] = 1;
        board[row + 5, col + 2] = 1;
        board[row + 5, col + 3] = 1;
        board[row + 5, col + 4] = 1;
        board[row + 5, col + 8] = 1;
        board[row + 5, col + 9] = 1;
        board[row + 5, col + 10] = 1;
        board[row + 7, col + 2] = 1;
        board[row + 7, col + 3] = 1;
        board[row + 7, col + 4] = 1;
        board[row + 7, col + 8] = 1;
        board[row + 7, col + 9] = 1;
        board[row + 7, col + 10] = 1;
        board[row + 8, col] = 1;
        board[row + 8, col + 5] = 1;
        board[row + 8, col + 7] = 1;
        board[row + 8, col + 12] = 1;
    }
}
