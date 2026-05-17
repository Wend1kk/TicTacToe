using System;
namespace TicTacToe.Models
{
    public class PlayerStats
    {
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int TotalGames => Wins + Losses + Draws;
        public double WinRate { get; set; }
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
        public int TotalMovesMade { get; set; }
        public int TotalMovesReceived { get; set; }
        public int GamesPlayedAsX { get; set; }
        public int GamesPlayedAsO { get; set; }
        public int FastestWinMoves { get; set; }
        public int SlowestWinMoves { get; set; }
        public int TotalTimePlayedSeconds { get; set; }
        public DateTime LastGameDate { get; set; }
        public string FavoriteStrategy { get; set; } = "Minimax";
        public bool SoundEnabled { get; set; } = true;
        public string Theme { get; set; } = "Default";
        public int Level { get; set; } = 1;
        public int ExperiencePoints { get; set; }
    }
}