using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System.Threading.Tasks;
using TicTacToe.Services;

namespace TicTacToe;

public partial class MainWindow : Window
{
    private const char CX    = 'X';
    private const char CO    = 'O';
    private const char Empty = ' ';
    private char      _currentPlayer = CX;
    private char[,]   _board         = new char[3, 3];
    private bool      _gameOver      = false;
    private bool      _botThinking   = false;
    private bool                   _vsBot      = false;
    private char                   _botSymbol  = CO;
    private AI.Difficulty   _difficulty = AI.Difficulty.Easy;
    private AI              _ai         = new(CO, CX, AI.Difficulty.Easy);
    private int _scoreX    = 0;
    private int _scoreO    = 0;
    private int _scoreDraw = 0;
    
    private static readonly int[][] WinLines =
    [
        [0,1,2], [3,4,5], [6,7,8],   
        [0,3,6], [1,4,7], [2,5,8], 
        [0,4,8], [2,4,6],            
    ];
    private Button[,] _cells = null!;

    public MainWindow()
    {
        InitializeComponent();

        _cells = new Button[3, 3]
        {
            { Btn00, Btn01, Btn02 },
            { Btn10, Btn11, Btn12 },
            { Btn20, Btn21, Btn22 },
        };
        InitBoard();
        UpdateStatus();
        SetActiveToggle(BtnEasy,   new[] { BtnMedium, BtnHard },   "active-green");
        SetActiveToggle(BtnModePvP, new[] { BtnModeBot },           "active-blue");
    }
    private async void Cell_Click(object? sender, RoutedEventArgs e)
    {
        if (_gameOver || _botThinking) return;
        if (sender is not Button btn)  return;
        var (row, col) = FindCell(btn);
        if (row < 0 || _board[row, col] != Empty) return;
        PlaceMarker(row, col, _currentPlayer);
        if (TryFinishGame()) return;
        _currentPlayer = Opponent(_currentPlayer);
        UpdateStatus();
        if (_vsBot && _currentPlayer == _botSymbol)
            await DoBotMoveAsync();
    }
    
    private async Task DoBotMoveAsync()
    {
        _botThinking = true;
        SetAllCellsEnabled(false);
        StatusText.Text       = "🤖  Бот думає...";
        StatusText.Foreground = Brush("#A78BFA");
        await Task.Delay(420);
        int move = _ai.GetBestMove(_board);
        if (move >= 0)
            PlaceMarker(move / 3, move % 3, _botSymbol);
        _botThinking = false;
        SetAllCellsEnabled(true);
        if (TryFinishGame()) return;
        _currentPlayer = Opponent(_currentPlayer);
        UpdateStatus();
    }

    private void PlaceMarker(int row, int col, char player)
    {
        _board[row, col] = player;
        var btn = _cells[row, col];
        btn.Content   = player.ToString();
        btn.IsEnabled = false;
        btn.Foreground = player == CX
            ? Brush("#60A5FA")  
            : Brush("#F472B6");
        btn.Classes.Add(player == CX ? "x-mark" : "o-mark");
    }
    
    private bool TryFinishGame()
    {
        var winLine = FindWinLine(_currentPlayer);
        if (winLine != null)
        {
            HighlightWin(winLine);
            string name = PlayerName(_currentPlayer);
            StatusText.Text       = $"🎉  {name} переміг!";
            StatusText.Foreground = Brush("#34D399");
            if (_currentPlayer == CX) { _scoreX++;  ScoreX.Text    = _scoreX.ToString(); }
            else                      { _scoreO++;  ScoreO.Text    = _scoreO.ToString(); }
            _gameOver = true;
            return true;
        }
        if (IsBoardFull())
        {
            StatusText.Text       = "🤝  Нічия!";
            StatusText.Foreground = Brush("#FBBF24");
            _scoreDraw++;
            ScoreDraw.Text = _scoreDraw.ToString();
            _gameOver      = true;
            return true;
        }
        return false;
    }
    
    private void NewGame_Click(object? sender, RoutedEventArgs e)
        => _ = StartNewGameAsync();

    private async Task StartNewGameAsync()
    {
        _gameOver      = false;
        _botThinking   = false;
        _currentPlayer = CX;
        InitBoard();
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            var btn = _cells[r, c];
            btn.Content    = "";
            btn.IsEnabled  = true;
            btn.Foreground = new SolidColorBrush(Colors.Transparent);
            btn.Classes.Remove("x-mark");
            btn.Classes.Remove("o-mark");
            btn.Classes.Remove("winner");
        }
        UpdateStatus();
        if (_vsBot && _botSymbol == CX)
            await DoBotMoveAsync();
    }

    private void ResetScore_Click(object? sender, RoutedEventArgs e)
    {
        _scoreX = _scoreO = _scoreDraw = 0;
        ScoreX.Text = ScoreO.Text = ScoreDraw.Text = "0";
        _ = StartNewGameAsync();
    }
    
    private void ModeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        _vsBot = btn == BtnModeBot;
        SetActiveToggle(BtnModePvP, new[] { BtnModeBot }, "active-blue");
        SetActiveToggle(BtnModeBot, new[] { BtnModePvP }, "active-blue");
        if (_vsBot)
        {
            BtnModePvP.Classes.Remove("active-blue");
            if (!BtnModeBot.Classes.Contains("active-blue"))
                BtnModeBot.Classes.Add("active-blue");
        }
        else
        {
            BtnModeBot.Classes.Remove("active-blue");
            if (!BtnModePvP.Classes.Contains("active-blue"))
                BtnModePvP.Classes.Add("active-blue");
        }
        DifficultyPanel.IsVisible = _vsBot;
        LabelLeft.Text  = _vsBot ? "Гравець" : "X";
        LabelRight.Text = _vsBot ? "Бот"      : "O";
        _scoreX = _scoreO = _scoreDraw = 0;
        ScoreX.Text = ScoreO.Text = ScoreDraw.Text = "0";
        _ = StartNewGameAsync();
    }

    private void DiffButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        foreach (var b in new[] { BtnEasy, BtnMedium, BtnHard })
        {
            b.Classes.Remove("active-green");
            b.Classes.Remove("active-yellow");
            b.Classes.Remove("active-red");
        }
        if (btn == BtnEasy)
        {
            _difficulty = AI.Difficulty.Easy;
            BtnEasy.Classes.Add("active-green");
        }
        else if (btn == BtnMedium)
        {
            _difficulty = AI.Difficulty.Medium;
            BtnMedium.Classes.Add("active-yellow");
        }
        else
        {
            _difficulty = AI.Difficulty.Hard;
            BtnHard.Classes.Add("active-red");
        }
        _ai = new AI(_botSymbol, CX, _difficulty);
        _ = StartNewGameAsync();
    }

    private void InitBoard()
    {
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            _board[r, c] = Empty;
    }

    private (int row, int col) FindCell(Button btn)
    {
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            if (_cells[r, c] == btn) return (r, c);
        return (-1, -1);
    }

    private int[]? FindWinLine(char player)
    {
        foreach (var line in WinLines)
        {
            if (_board[line[0]/3, line[0]%3] == player &&
                _board[line[1]/3, line[1]%3] == player &&
                _board[line[2]/3, line[2]%3] == player)
                return line;
        }
        return null;
    }

    private void HighlightWin(int[] line)
    {
        foreach (int idx in line)
        {
            var btn = _cells[idx / 3, idx % 3];
            btn.Classes.Remove("x-mark");
            btn.Classes.Remove("o-mark");
            btn.Classes.Add("winner");
            btn.Foreground = Brush("#34D399"); 
        }
    }

    private bool IsBoardFull()
    {
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            if (_board[r, c] == Empty) return false;
        return true;
    }

    private void SetAllCellsEnabled(bool enabled)
    {
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            if (_board[r, c] == Empty)
                _cells[r, c].IsEnabled = enabled;
    }

    private void UpdateStatus()
    {
        if (_gameOver) return;

        bool isBotTurn = _vsBot && _currentPlayer == _botSymbol;
        StatusText.Text = isBotTurn
            ? "🤖  Хід бота..."
            : $"⚡  Хід гравця {_currentPlayer}";

        StatusText.Foreground = _currentPlayer == CX
            ? Brush("#60A5FA")
            : Brush("#F472B6");
    }

    private string PlayerName(char p)
    {
        if (_vsBot && p == _botSymbol) return "🤖 Бот";
        return $"Гравець {p}";
    }

    private static char Opponent(char p) => p == CX ? CO : CX;

    private static SolidColorBrush Brush(string hex)
        => new(Avalonia.Media.Color.Parse(hex));
    
    private static void SetActiveToggle(Button active, Button[] others, string cls)
    {
        foreach (var b in others)
            b.Classes.Remove(cls);

        if (!active.Classes.Contains(cls))
            active.Classes.Add(cls);
    }
}