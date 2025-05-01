using System.Timers;
using Excubo.Blazor.Canvas;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorExperiments.UI.Shared;

public partial class TetrisGame {
    CanvasComponent _canvas = null!;

    // Game constants
    const int Cols = 10;
    const int Rows = 20;
    const int BlockSize = 30;
    readonly string[] _colors = [
        "transparent",  // Empty space
        "#FF0D72",      // I
        "#0DC2FF",      // J
        "#0DFF72",      // L
        "#F538FF",      // O
        "#FF8E0D",      // S
        "#FFE138",      // T
        "#3877FF"       // Z
    ];

    // Tetromino shapes
    readonly int[][][] _shapes = [
        new int[][] { },
        new int[][] { [0, 0, 0, 0], [1, 1, 1, 1], [0, 0, 0, 0], [0, 0, 0, 0] }, // I
        new int[][] { [2, 0, 0], [2, 2, 2], [0, 0, 0] },                        // J
        new int[][] { [0, 0, 3], [3, 3, 3], [0, 0, 0] },                        // L
        new int[][] { [0, 4, 4], [0, 4, 4], [0, 0, 0] },                        // O
        new int[][] { [0, 5, 5], [5, 5, 0], [0, 0, 0] },                        // S
        new int[][] { [0, 6, 0], [6, 6, 6], [0, 0, 0] },                        // T
        new int[][] { [7, 7, 0], [0, 7, 7], [0, 0, 0] }                         // Z
    ];

    // UI constants
    const int MainAreaWidth = Cols * BlockSize;
    const int MainAreaHeight = Rows * BlockSize;
    const int SidebarWidth = 200;
    const int CanvasWidth = MainAreaWidth + SidebarWidth;
    const int CanvasHeight = MainAreaHeight;
    const int NextPieceSize = 120;
    const int NextPieceX = MainAreaWidth + (SidebarWidth - NextPieceSize) / 2;
    const int NextPieceY = 30;
    const int ScorePanelX = MainAreaWidth + 20;
    const int ScorePanelY = NextPieceY + NextPieceSize + 20;
    const int ControlsPanelX = MainAreaWidth + 20;
    const int ControlsPanelY = ScorePanelY + 120;
    const int ButtonWidth = 160;
    const int ButtonHeight = 40;
    const int StartButtonX = MainAreaWidth + (SidebarWidth - ButtonWidth) / 2;
    const int StartButtonY = CanvasHeight - 60;

    // Game variables
    int[][] _board;
    Piece? _currentPiece, _nextPiece;
    int _score = 0;
    int _lines = 0;
    int _level = 1;
    double _dropCounter = 0;
    double _dropInterval = 1000;
    DateTime _lastTime = DateTime.Now;
    bool _gameActive = false;
    bool _paused = false;
    bool _showGameOver = false;
    double _mouseX = 0;
    double _mouseY = 0;

    // Piece class to represent tetrominos
    class Piece(int x, int y, int[][] shape, int type) {
        public int X { get; set; } = x;
        public int Y { get; set; } = y;
        public int[][] Shape { get; set; } = shape;
        public int Type { get; } = type;
    }

    void Initialize() {
        CreateBoard();
        _canvas.Timer.Enabled = true;
        StateHasChanged();
    }
    
    void StartGame()
    {
        CreateBoard();
        _score = 0;
        _lines = 0;
        _level = 1;
        _dropInterval = 1000;
        _gameActive = true;
        _paused = false;
        _showGameOver = false;

        // Create first piece and next piece
        CreatePiece();
        CreateNextPiece();

        _lastTime = DateTime.Now;
    }
    
    async ValueTask GameLoopAsync(ElapsedEventArgs elapsedEvent)
    {
        if (!_gameActive && !_showGameOver)
        {
            await DrawAllAsync();
            return;
        }

        var deltaTime = elapsedEvent.SignalTime - _lastTime;
        _lastTime = elapsedEvent.SignalTime;

        if (_gameActive && !_paused)
        {
            _dropCounter += deltaTime.Milliseconds;
            if (_dropCounter > _dropInterval)
            {
                MoveDown();
                _dropCounter = 0;
            }
        }

        await DrawAllAsync();
    }

    void CreateBoard()
    {
        _board = new int[Rows][];
        for (int i = 0; i < Rows; i++)
        {
            _board[i] = new int[Cols];
        }
    }
    
    async Task DrawAllAsync()
    {
        // Clear the entire canvas
        await _canvas.Context.ClearRectAsync(0, 0, CanvasWidth, CanvasHeight);

        // Draw main game area background
        await _canvas.Context.FillStyleAsync("#111");
        await _canvas.Context.FillRectAsync(0, 0, MainAreaWidth, MainAreaHeight);

        // Draw sidebar background
        await _canvas.Context.FillStyleAsync("#222");
        await _canvas.Context.FillRectAsync(MainAreaWidth, 0, SidebarWidth, CanvasHeight);

        // Draw game board and current piece
        await DrawBoardAsync();
        if (_gameActive)
        {
            await DrawPieceAsync();
        }

        // Draw sidebar content
        await DrawNextPieceAreaAsync();
        await DrawScorePanelAsync();
        await DrawControlsPanelAsync();
        await DrawStartButtonAsync();

        // Draw overlay screens
        if (_showGameOver)
        {
            await DrawGameOverScreenAsync();
        }
        else if (_paused)
        {
            await DrawPausedScreenAsync();
        }
    }

    // Draw game board
    async Task DrawBoardAsync()
    {
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                int blockValue = _board[row][col];
                await DrawBlockAsync(col, row, blockValue);
            }
        }
    }

    // Draw a single block
    async Task DrawBlockAsync(int x, int y, int type)
    {
        if (type == 0)
        {
            // Draw grid for empty blocks
            await _canvas.Context.FillStyleAsync("#111");
            await _canvas.Context.FillRectAsync(x * BlockSize, y * BlockSize, BlockSize, BlockSize);
            await _canvas.Context.StrokeStyleAsync("#333");
            await _canvas.Context.LineWidthAsync(1);
            await _canvas.Context.StrokeRectAsync(x * BlockSize, y * BlockSize, BlockSize, BlockSize);
        }
        else
        {
            // Draw colored blocks
            await _canvas.Context.FillStyleAsync(_colors[type]);
            await _canvas.Context.FillRectAsync(x * BlockSize, y * BlockSize, BlockSize, BlockSize);

            // Draw highlight
            await _canvas.Context.FillStyleAsync("rgba(255, 255, 255, 0.2)");
            await _canvas.Context.FillRectAsync(x * BlockSize, y * BlockSize, BlockSize, 5);
            await _canvas.Context.FillRectAsync(x * BlockSize, y * BlockSize, 5, BlockSize);

            // Draw shadow
            await _canvas.Context.FillStyleAsync("rgba(0, 0, 0, 0.4)");
            await _canvas.Context.FillRectAsync(x * BlockSize + BlockSize - 5, y * BlockSize, 5, BlockSize);
            await _canvas.Context.FillRectAsync(x * BlockSize, y * BlockSize + BlockSize - 5, BlockSize, 5);

            // Draw border
            await _canvas.Context.StrokeStyleAsync("black");
            await _canvas.Context.LineWidthAsync(1);
            await _canvas.Context.StrokeRectAsync(x * BlockSize, y * BlockSize, BlockSize, BlockSize);
        }
    }

    // Draw current piece
    async Task DrawPieceAsync()
    {
        if (_currentPiece == null) return;

        var piece = _currentPiece.Shape;
        var type = _currentPiece.Type;

        for (int row = 0; row < piece.Length; row++)
        {
            for (int col = 0; col < piece[row].Length; col++)
            {
                if (piece[row][col] != 0)
                {
                    await DrawBlockAsync(_currentPiece.X + col, _currentPiece.Y + row, type);
                }
            }
        }
    }

    // Draw next piece area
    async Task DrawNextPieceAreaAsync()
    {
        // Draw next piece area header
        await _canvas.Context.FillStyleAsync("white");
        await _canvas.Context.FontAsync("18px Arial");
        await _canvas.Context.TextAlignAsync(TextAlign.Center);
        await _canvas.Context.FillTextAsync("NEXT PIECE", MainAreaWidth + SidebarWidth / 2, 20);

        // Draw next piece area background
        await _canvas.Context.FillStyleAsync("#111");
        await _canvas.Context.FillRectAsync(NextPieceX, NextPieceY, NextPieceSize, NextPieceSize);

        // Draw next piece
        if (_nextPiece != null)
        {
            var piece = _nextPiece.Shape;
            var type = _nextPiece.Type;
            var blockSize = BlockSize * 0.8;

            var offsetX = NextPieceX + (NextPieceSize - piece[0].Length * blockSize) / 2;
            var offsetY = NextPieceY + (NextPieceSize - piece.Length * blockSize) / 2;

            for (int row = 0; row < piece.Length; row++)
            {
                for (int col = 0; col < piece[row].Length; col++) {
                    if (piece[row][col] == 0) continue;

                    // Draw colored blocks
                    await _canvas.Context.FillStyleAsync(_colors[type]);
                    await _canvas.Context.FillRectAsync(offsetX + col * blockSize, offsetY + row * blockSize, blockSize, blockSize);

                    // Draw highlight
                    await _canvas.Context.FillStyleAsync("rgba(255, 255, 255, 0.2)");
                    await _canvas.Context.FillRectAsync(offsetX + col * blockSize, offsetY + row * blockSize, blockSize, 4);
                    await _canvas.Context.FillRectAsync(offsetX + col * blockSize, offsetY + row * blockSize, 4, blockSize);

                    // Draw shadow
                    await _canvas.Context.FillStyleAsync("rgba(0, 0, 0, 0.4)");
                    await _canvas.Context.FillRectAsync(offsetX + col * blockSize + blockSize - 4, offsetY + row * blockSize, 4, blockSize);
                    await _canvas.Context.FillRectAsync(offsetX + col * blockSize, offsetY + row * blockSize + blockSize - 4, blockSize, 4);

                    // Draw border
                    await _canvas.Context.StrokeStyleAsync("black");
                    await _canvas.Context.LineWidthAsync(1);
                    await _canvas.Context.StrokeRectAsync(offsetX + col * blockSize, offsetY + row * blockSize, blockSize, blockSize);
                }
            }
        }
    }

    // Draw score panel
    async Task DrawScorePanelAsync()
    {
        await _canvas.Context.FillStyleAsync("white");
        await _canvas.Context.FontAsync("16px Arial");
        await _canvas.Context.TextAlignAsync(TextAlign.Left);
        await _canvas.Context.FillTextAsync($"Score: {_score}", ScorePanelX, ScorePanelY);
        await _canvas.Context.FillTextAsync($"Lines: {_lines}", ScorePanelX, ScorePanelY + 30);
        await _canvas.Context.FillTextAsync($"Level: {_level}", ScorePanelX, ScorePanelY + 60);
    }

    // Draw controls panel
    async Task DrawControlsPanelAsync()
    {
        await _canvas.Context.FillStyleAsync("white");
        await _canvas.Context.FontAsync("16px Arial");
        await _canvas.Context.TextAlignAsync(TextAlign.Left);
        await _canvas.Context.FillTextAsync("CONTROLS", ControlsPanelX, ControlsPanelY);
        await _canvas.Context.FillTextAsync("← → : Move", ControlsPanelX, ControlsPanelY + 30);
        await _canvas.Context.FillTextAsync("↑ : Rotate", ControlsPanelX, ControlsPanelY + 60);
        await _canvas.Context.FillTextAsync("↓ : Soft Drop", ControlsPanelX, ControlsPanelY + 90);
        await _canvas.Context.FillTextAsync("P : Pause", ControlsPanelX, ControlsPanelY + 120);
    }

    // Draw start button
    async Task DrawStartButtonAsync()
    {
        // Check if mouse is over button
        bool isHovered = _mouseX >= StartButtonX && _mouseX <= StartButtonX + ButtonWidth &&
                         _mouseY >= StartButtonY && _mouseY <= StartButtonY + ButtonHeight;

        // Draw button background
        await _canvas.Context.FillStyleAsync(isHovered ? "#6BCF70" : "#4CAF50");
        await _canvas.Context.FillRectAsync(StartButtonX, StartButtonY, ButtonWidth, ButtonHeight);

        // Draw button text
        await _canvas.Context.FillStyleAsync("white");
        await _canvas.Context.FontAsync("16px Arial");
        await _canvas.Context.TextAlignAsync(TextAlign.Center);
        await _canvas.Context.TextBaseLineAsync(TextBaseLine.Middle);
        await _canvas.Context.FillTextAsync(_gameActive ? "RESTART" : "START GAME", StartButtonX + ButtonWidth / 2, StartButtonY + ButtonHeight / 2);

        // Reset text baseline
        await _canvas.Context.TextBaseLineAsync(TextBaseLine.Alphabetic);
    }

    // Draw game over screen
    async Task DrawGameOverScreenAsync()
    {
        // Draw semi-transparent background
        await _canvas.Context.FillStyleAsync("rgba(0, 0, 0, 0.8)");
        await _canvas.Context.FillRectAsync(0, 0, MainAreaWidth, MainAreaHeight);

        // Draw game over text
        await _canvas.Context.FillStyleAsync("white");
        await _canvas.Context.FontAsync("28px Arial");
        await _canvas.Context.TextAlignAsync(TextAlign.Center);
        await _canvas.Context.FillTextAsync("GAME OVER!", MainAreaWidth / 2, MainAreaHeight / 2 - 40);

        // Draw score
        await _canvas.Context.FontAsync("20px Arial");
        await _canvas.Context.FillTextAsync($"Your score: {_score}", MainAreaWidth / 2, MainAreaHeight / 2);

        // Draw play again message
        await _canvas.Context.FontAsync("16px Arial");
        await _canvas.Context.FillTextAsync("Click the START button to play again", MainAreaWidth / 2, MainAreaHeight / 2 + 40);
    }

    // Draw paused screen
    async ValueTask DrawPausedScreenAsync()
    {
        // Draw semi-transparent background
        await _canvas.Context.FillStyleAsync("rgba(0, 0, 0, 0.8)");
        await _canvas.Context.FillRectAsync(0, 0, MainAreaWidth, MainAreaHeight);

        // Draw paused text
        await _canvas.Context.FillStyleAsync("white");
        await _canvas.Context.FontAsync("28px Arial");
        await _canvas.Context.TextAlignAsync(TextAlign.Center);
        await _canvas.Context.FillTextAsync("PAUSED", MainAreaWidth / 2, MainAreaHeight / 2 - 20);

        // Draw instruction
        await _canvas.Context.FontAsync("16px Arial");
        await _canvas.Context.FillTextAsync("Press P to resume", MainAreaWidth / 2, MainAreaHeight / 2 + 20);
    }

    // Create a new piece
    void CreatePiece()
    {
        if (_nextPiece != null)
        {
            _currentPiece = _nextPiece;
        }
        else
        {
            var type = new Random().Next(1, 8);
            _currentPiece = new Piece(
                x: (int)Math.Floor(Cols / 2.0) - (int)Math.Floor(_shapes[type][0].Length / 2.0),
                y: 0,
                shape: _shapes[type],
                type: type
            );
        }

        // Check if game is over
        if (Collision(0, 0))
        {
            GameOver();
        }
    }

    void CreateNextPiece()
    {
        var type = new Random().Next(1, 8);
        _nextPiece = new Piece(
            x: (int)Math.Floor(Cols / 2.0) - (int)Math.Floor(_shapes[type][0].Length / 2.0),
            y: 0,
            shape: _shapes[type],
            type: type
        );
    }

    bool Collision(int offsetX, int offsetY, int[][]? rotatedPiece = null)
    {
        var piece = rotatedPiece ?? _currentPiece.Shape;

        for (int row = 0; row < piece.Length; row++)
        {
            for (int col = 0; col < piece[row].Length; col++)
            {
                if (piece[row][col] != 0)
                {
                    var newX = _currentPiece.X + col + offsetX;
                    var newY = _currentPiece.Y + row + offsetY;

                    if (
                        newX < 0 ||
                        newX >= Cols ||
                        newY >= Rows ||
                        (newY >= 0 && _board[newY][newX] != 0)
                    )
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void MoveLeft()
    {
        if (!Collision(-1, 0))
        {
            _currentPiece.X--;
        }
    }

    void MoveRight()
    {
        if (!Collision(1, 0))
        {
            _currentPiece.X++;
        }
    }

    void MoveDown()
    {
        if (!Collision(0, 1))
        {
            _currentPiece.Y++;
            return;
        }

        MergePiece();
        ClearLines();
        CreatePiece();
        CreateNextPiece();

        // Speed up the game as level increases
        _dropInterval = Math.Max(100, 1000 - (_level - 1) * 50);
    }

    // Rotate piece
    void Rotate()
    {
        var piece = _currentPiece.Shape;
        var rotated = new int[piece[0].Length][];

        for (int i = 0; i < piece[0].Length; i++)
        {
            rotated[i] = new int[piece.Length];
            for (int j = piece.Length - 1; j >= 0; j--)
            {
                rotated[i][piece.Length - 1 - j] = piece[j][i];
            }
        }

        // Try rotation
        if (!Collision(0, 0, rotated))
        {
            _currentPiece.Shape = rotated;
        }
        // Try wall kicks if rotation causes collision
        else if (!Collision(-1, 0, rotated))
        {
            _currentPiece.X--;
            _currentPiece.Shape = rotated;
        }
        else if (!Collision(1, 0, rotated))
        {
            _currentPiece.X++;
            _currentPiece.Shape = rotated;
        }
        else if (!Collision(0, -1, rotated))
        {
            _currentPiece.Y--;
            _currentPiece.Shape = rotated;
        }
    }

    void MergePiece()
    {
        var piece = _currentPiece.Shape;
        var type = _currentPiece.Type;

        for (int row = 0; row < piece.Length; row++)
        {
            for (int col = 0; col < piece[row].Length; col++)
            {
                if (piece[row][col] != 0)
                {
                    var boardRow = _currentPiece.Y + row;
                    var boardCol = _currentPiece.X + col;

                    if (boardRow >= 0)
                    {
                        _board[boardRow][boardCol] = type;
                    }
                }
            }
        }
    }

    void ClearLines()
    {
        int linesCleared = 0;

        for (int row = Rows - 1; row >= 0; row--)
        {
            if (_board[row].All(cell => cell != 0))
            {
                // Remove the line
                for (int r = row; r > 0; r--)
                {
                    _board[r] = _board[r - 1].ToArray();
                }
                // Add empty line at the top
                _board[0] = new int[Cols];

                linesCleared++;
                row++; // Check the same row again (now with new content)
            }
        }

        if (linesCleared > 0)
        {
            UpdateScore(linesCleared);
        }
    }

    void UpdateScore(int linesCleared = 0)
    {
        if (linesCleared > 0)
        {
            // Scoring system: more points for clearing multiple lines at once
            int[] points = [0, 100, 300, 500, 800];
            _score += points[Math.Min(linesCleared, 4)] * _level;
            _lines += linesCleared;

            // Level up every 10 lines
            _level = (int)Math.Floor(_lines / 10.0) + 1;
        }
    }

    void GameOver()
    {
        _gameActive = false;
        _showGameOver = true;
    }

    // Toggle pause
    void TogglePause()
    {
        _paused = !_paused;
    }

    public void Move(KeyboardEventArgs e) {
        if (!_gameActive && !_showGameOver)
        {
            if (e.Code == "Space")
            {
                StartGame();
            }
            return;
        }

        if (_showGameOver)
        {
            return;
        }
        if (e.Code == "KeyP")
        {
            TogglePause();
            return;
        }

        if (_paused)
        {
            return;
        }

        switch (e.Code)
        {
            case "ArrowLeft":
                MoveLeft();
                break;
            case "ArrowRight":
                MoveRight();
                break;
            case "ArrowDown":
                MoveDown();
                break;
            case "ArrowUp":
                Rotate();
                break;
        }
    }

    ValueTask HandleMouseUp(MouseEventArgs arg) {
        double x = arg.OffsetX, y = arg.OffsetY;
        if (x is >= StartButtonX and <= StartButtonX + ButtonWidth &&
            y is >= StartButtonY and <= StartButtonY + ButtonHeight)
        {
            StartGame();
        }
        
        return ValueTask.CompletedTask;
    }
}
