using System.Text.Json;
using TicTacToe.Models;
using System;
using System.IO;
namespace TicTacToe.Services
{
    public class FileStorageService
    {
        private readonly string _filePath;

        public FileStorageService()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (string.IsNullOrEmpty(documentsPath))
                documentsPath = AppDomain.CurrentDomain.BaseDirectory;
            
            _filePath = Path.Combine(documentsPath, "tictactoe_stats.json");
        }

        public void SaveStats(PlayerStats stats)
        {
            try
            {
                stats.LastGameDate = DateTime.Now;
                string json = JsonSerializer.Serialize(stats, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving stats: {ex.Message}");
            }
        }

        public PlayerStats LoadStats()
        {
            if (!File.Exists(_filePath))
                return new PlayerStats();

            try
            {
                string json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<PlayerStats>(json) ?? new PlayerStats();
            }
            catch
            {
                return new PlayerStats();
            }
        }
    }
}