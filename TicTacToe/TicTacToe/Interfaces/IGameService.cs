using System;
using TicTacToe.Models;

namespace TicTacToe.Interfaces
{
    public interface IGameService
    {
        void MakeMove(int row, int col);
        GameResult CheckGameStatus();
        void ResetGame();
        char[,] GetBoard();
        event Action<GameResult>? GameEnded;
    }
}