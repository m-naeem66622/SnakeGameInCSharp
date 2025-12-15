using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace SnakeGame
{
    public partial class ScoreHistoryWindow : Window
    {
        public ScoreHistoryWindow(IEnumerable<(int Score, DateTime CompletedAt)> scores, int bestScore)
        {
            InitializeComponent();

            var ordered = scores
                .OrderByDescending(entry => entry.Score)
                .ThenBy(entry => entry.CompletedAt)
                .Select((entry, index) => new ScoreHistoryRow
                {
                    Rank = index + 1,
                    Score = entry.Score,
                    Timestamp = entry.CompletedAt.ToLocalTime().ToString("g"),
                    IsBest = entry.Score == bestScore && bestScore > 0
                })
                .ToList();

            ScoresList.ItemsSource = ordered;
            BestScoreLabel.Text = $"Best Score: {bestScore}";
            EmptyHint.Visibility = ordered.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private sealed class ScoreHistoryRow
        {
            public int Rank { get; init; }
            public int Score { get; init; }
            public string Timestamp { get; init; } = string.Empty;
            public bool IsBest { get; init; }
        }
    }
}
