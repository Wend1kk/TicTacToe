using System;
using TicTacToe.Models;
using TicTacToe.Interfaces;

namespace TicTacToe.Services
{
    public class GameService : IGameService
    {
        private char[,] _board = new char[3, 3];
        private char _currentPlayer = 'X';
        private readonly AI _ai;
        public event Action<GameResult>? GameEnded;

        public GameService(AI.Difficulty difficulty = AI.Difficulty.Medium)
        {
            _ai = new AI('O', 'X', difficulty);
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    _board[i, j] = ' ';
            _currentPlayer = 'X';
        }

        public void MakeMove(int row, int col)
        {
            if (_board[row, col] == ' ')
            {
                _board[row, col] = _currentPlayer;
                var result = CheckGameStatus();
                
                if (result == GameResult.InProgress)
                {
                    _currentPlayer = _currentPlayer == 'X' ? 'O' : 'X';
                    if (_currentPlayer == 'O')
                        MakeAIMove();
                }
                else
                {
                    GameEnded?.Invoke(result);
                }
            }
        }

        private void MakeAIMove()
        {
            int bestMove = _ai.GetBestMove(_board);
            int row = bestMove / 3;
            int col = bestMove % 3;
            _board[row, col] = 'O';
            
            var result = CheckGameStatus();
            if (result != GameResult.InProgress)
                GameEnded?.Invoke(result);
            else
                _currentPlayer = 'X';
        }

        public GameResult CheckGameStatus()
        {
            for (int i = 0; i < 3; i++)
                if (_board[i, 0] == _board[i, 1] && _board[i, 1] == _board[i, 2] && _board[i, 0] != ' ')
                    return _board[i, 0] == 'X' ? GameResult.PlayerWin : GameResult.AIWin;

            for (int i = 0; i < 3; i++)
                if (_board[0, i] == _board[1, i] && _board[1, i] == _board[2, i] && _board[0, i] != ' ')
                    return _board[0, i] == 'X' ? GameResult.PlayerWin : GameResult.AIWin;

            if (_board[0, 0] == _board[1, 1] && _board[1, 1] == _board[2, 2] && _board[0, 0] != ' ')
                return _board[0, 0] == 'X' ? GameResult.PlayerWin : GameResult.AIWin;
            
            if (_board[0, 2] == _board[1, 1] && _board[1, 1] == _board[2, 0] && _board[0, 2] != ' ')
                return _board[0, 2] == 'X' ? GameResult.PlayerWin : GameResult.AIWin;

            bool isFull = true;
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (_board[i, j] == ' ') isFull = false;

            return isFull ? GameResult.Draw : GameResult.InProgress;
        }

        public void ResetGame() => InitializeBoard();
        public char[,] GetBoard() => _board;
    }
}