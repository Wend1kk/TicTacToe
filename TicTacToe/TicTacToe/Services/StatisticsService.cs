using TicTacToe.Models;

namespace TicTacToe.Services
{
    public class StatisticsService
    {
        public PlayerStats CurrentStats { get; private set; }

        public StatisticsService()
        {
            CurrentStats = new PlayerStats();
        }

        public void RecordWin()
        {
            CurrentStats.Wins++;
            UpdateWinRate();
            UpdateStreak(true);
            CurrentStats.ExperiencePoints += 10;
            CheckLevelUp();
        }

        public void RecordLoss()
        {
            CurrentStats.Losses++;
            UpdateWinRate();
            UpdateStreak(false);
            CurrentStats.ExperiencePoints += 2;
        }

        public void RecordDraw()
        {
            CurrentStats.Draws++;
            UpdateWinRate();
            CurrentStats.CurrentStreak = 0;
            CurrentStats.ExperiencePoints += 5;
        }

        private void UpdateWinRate()
        {
            int totalGames = CurrentStats.Wins + CurrentStats.Losses + CurrentStats.Draws;
            CurrentStats.WinRate = totalGames > 0 ? (double)CurrentStats.Wins / totalGames * 100 : 0;
        }

        private void UpdateStreak(bool isWin)
        {
            if (isWin)
            {
                CurrentStats.CurrentStreak++;
                if (CurrentStats.CurrentStreak > CurrentStats.BestStreak)
                    CurrentStats.BestStreak = CurrentStats.CurrentStreak;
            }
            else
            {
                CurrentStats.CurrentStreak = 0;
            }
        }

        private void CheckLevelUp()
        {
            int requiredXP = CurrentStats.Level * 100;
            if (CurrentStats.ExperiencePoints >= requiredXP)
                CurrentStats.Level++;
        }

        public void ResetStats() => CurrentStats = new PlayerStats();
    }
}