using System.Timers;
using BlazorExperiments.UI.Shared;
using Excubo.Blazor.Canvas.Contexts;

namespace BlazorExperiments.UI.Pages;
public partial class GameOfLife {
    CanvasComponent _canvas = null!;
    int[,] _board = null!;
    int _rows, _cols, _cellSize;
    DateTime _lastTime = DateTime.Now;
    readonly TimeSpan _snakeSpeedInMilliseconds = TimeSpan.FromMilliseconds(400);

    void Initialize() {
        _cellSize = 10;
        _rows = (int)_canvas.Height / _cellSize;
        _cols = (int)_canvas.Width / _cellSize;
        _board = new int[_rows, _cols];

        AddGliderShape(_board);
        AddExploderShape(_board, 10, 0);
        AddPulsarShape(_board, 20, 20);
        AddStableShape(_board, 10, 10);
        AddTetrominoShape(_board, 5, 30);

        StateHasChanged();
    }

    async ValueTask Loop(ElapsedEventArgs elapsedEvent) {
        if (elapsedEvent.SignalTime - _lastTime < _snakeSpeedInMilliseconds) {
            return;
        }
        _lastTime = elapsedEvent.SignalTime;

        await using var batch = _canvas.Context.CreateBatch();
        await ClearScreenAsync(batch);
        int[,] newBoard = new int[_rows, _cols];

        for (int i = 0; i < _rows; i++) {
            for (int j = 0; j < _cols; j++) {
                int liveNeighbors = 0;
                liveNeighbors = GetLiveNeighbors(i, j);

                if (_board[i, j] == 1 && (liveNeighbors < 2 || liveNeighbors > 3)) {
                    newBoard[i, j] = 0;
                }
                else if (_board[i, j] == 0 && liveNeighbors == 3) {
                    newBoard[i, j] = 1;
                }
                else {
                    newBoard[i, j] = _board[i, j];
                }

                if (newBoard[i, j] == 1) {
                    await batch.FillStyleAsync("white");
                }
                else {
                    await batch.FillStyleAsync("black");
                }

                await batch.FillRectAsync(j * _cellSize, i * _cellSize, _cellSize, _cellSize);
            }
        }
        _board = newBoard;
    }
    private int GetLiveNeighbors(int x, int y) {
        int liveNeighbors = 0;

        // Check the 8 surrounding cells
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                // Skip the cell itself
                if (i == 0 && j == 0)
                    continue;

                int newX = x + i;
                int newY = y + j;

                // Check if the coordinates are within the board boundaries
                if (newX >= 0 && newX < _board.GetLength(0) && newY >= 0 && newY < _board.GetLength(1)) {
                    // If the neighbor is alive, increment the count
                    if (_board[newX, newY] == 1)
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

    void AddExploderShape(int[,] board, int x = 0, int y = 0) {
        board[x, y + 2] = 1;
        board[x + 1, y] = 1;
        board[x + 1, y + 2] = 1;
        board[x + 2, y] = 1;
        board[x + 2, y + 1] = 1;
        board[x + 2, y + 2] = 1;
        board[x + 3, y + 2] = 1;
    }

    void AddStableShape(int[,] board, int x = 0, int y = 0) {
        board[x, y] = 1;
        board[x + 1, y] = 1;
        board[x, y + 1] = 1;
        board[x, y + 2] = 1;
    }

    void AddTetrominoShape(int[,] board, int x = 0, int y = 0) {
        board[x, y] = 1;
        board[x, y + 1] = 1;
        board[x, y + 2] = 1;
        board[x - 1, y + 1] = 1;
    }

    void AddPulsarShape(int[,] board, int x = 0, int y = 0) {
        board[x, y + 2] = 1;
        board[x, y + 3] = 1;
        board[x, y + 4] = 1;
        board[x, y + 8] = 1;
        board[x, y + 9] = 1;
        board[x, y + 10] = 1;
        board[x + 2, y] = 1;
        board[x + 2, y + 5] = 1;
        board[x + 2, y + 7] = 1;
        board[x + 2, y + 12] = 1;
        board[x + 3, y] = 1;
        board[x + 3, y + 5] = 1;
        board[x + 3, y + 7] = 1;
        board[x + 3, y + 12] = 1;
        board[x + 4, y] = 1;
        board[x + 4, y + 5] = 1;
        board[x + 4, y + 7] = 1;
        board[x + 4, y + 12] = 1;
        board[x + 5, y + 2] = 1;
        board[x + 5, y + 3] = 1;
        board[x + 5, y + 4] = 1;
        board[x + 5, y + 8] = 1;
        board[x + 5, y + 9] = 1;
        board[x + 5, y + 10] = 1;
        board[x + 7, y + 2] = 1;
        board[x + 7, y + 3] = 1;
        board[x + 7, y + 4] = 1;
        board[x + 7, y + 8] = 1;
        board[x + 7, y + 9] = 1;
        board[x + 7, y + 10] = 1;
        board[x + 8, y] = 1;
        board[x + 8, y + 5] = 1;
        board[x + 8, y + 7] = 1;
        board[x + 8, y + 12] = 1;
    }
}
