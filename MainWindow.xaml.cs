using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Linq;

namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        private readonly GameEngine _engine;
        private readonly Brush _snakeBrush = new LinearGradientBrush(Color.FromRgb(0x6A, 0xF0, 0xD6), Color.FromRgb(0x00, 0x8C, 0x6A), 45);
        private readonly Brush _foodBrush = new SolidColorBrush(Color.FromRgb(0x3E, 0xE3, 0xA7));
        private readonly Brush _bonusBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0x66));

        public MainWindow()
        {
            InitializeComponent();

            // create engine: grid 40x30 (responsive)
            _engine = new GameEngine(40, 30);
            _engine.Updated += Engine_Updated;
            _engine.GameOver += Engine_GameOver;
            SizeChanged += MainWindow_SizeChanged;

            RenderAll();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderAll();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e) => _engine.Start();
        private void PauseButton_Click(object sender, RoutedEventArgs e) => _engine.TogglePause();
        private void ResetButton_Click(object sender, RoutedEventArgs e) => _engine.Reset();

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    _engine.SetDirection(Direction.Up);
                    break;
                case Key.Down:
                case Key.S:
                    _engine.SetDirection(Direction.Down);
                    break;
                case Key.Left:
                case Key.A:
                    _engine.SetDirection(Direction.Left);
                    break;
                case Key.Right:
                case Key.D:
                    _engine.SetDirection(Direction.Right);
                    break;
                case Key.Space:
                    _engine.TogglePause();
                    break;
            }
        }

        private void Engine_GameOver(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Game Over! Score: {_engine.Score}", "Neon Snake");
                RenderAll();
            });
        }

        private void Engine_Updated(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(RenderAll);
        }

        private void RenderAll()
        {
            GameCanvas.Children.Clear();

            var cellW = Math.Max(6, GameCanvas.ActualWidth / _engine.GridWidth);
            var cellH = Math.Max(6, GameCanvas.ActualHeight / _engine.GridHeight);

            // background glow
            var glow = new Rectangle
            {
                Width = GameCanvas.ActualWidth,
                Height = GameCanvas.ActualHeight,
                Fill = new LinearGradientBrush(Color.FromRgb(6, 18, 35), Color.FromRgb(2, 12, 22), 90),
                Opacity = 0.25
            };
            Canvas.SetLeft(glow, 0);
            Canvas.SetTop(glow, 0);
            GameCanvas.Children.Add(glow);

            // draw snake
            bool first = true;
            foreach (var segment in _engine.Snake.Segments)
            {
                var rect = new Rectangle
                {
                    Width = cellW - 2,
                    Height = cellH - 2,
                    RadiusX = 6,
                    RadiusY = 6,
                    Fill = first ? _snakeBrush : _snakeBrush,
                    Opacity = first ? 1.0 : 0.92
                };
                Canvas.SetLeft(rect, segment.X * cellW + 1);
                Canvas.SetTop(rect, segment.Y * cellH + 1);
                GameCanvas.Children.Add(rect);
                first = false;
            }

            // draw food
            if (_engine.Food is not null)
            {
                var f = new Ellipse
                {
                    Width = cellW - 6,
                    Height = cellH - 6,
                    Fill = _foodBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.6,
                    Opacity = 0.95
                };
                Canvas.SetLeft(f, _engine.Food.Position.X * cellW + 3);
                Canvas.SetTop(f, _engine.Food.Position.Y * cellH + 3);
                GameCanvas.Children.Add(f);
            }

            // draw bonus
            if (_engine.Bonus is not null)
            {
                var b = new Ellipse
                {
                    Width = cellW - 4,
                    Height = cellH - 4,
                    Fill = _bonusBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.6,
                    Opacity = 0.95
                };
                Canvas.SetLeft(b, _engine.Bonus.Position.X * cellW + 2);
                Canvas.SetTop(b, _engine.Bonus.Position.Y * cellH + 2);
                GameCanvas.Children.Add(b);
            }

            // HUD updates
            ScoreText.Text = $"Score: {_engine.Score}";
            LevelText.Text = $"Level: {_engine.Level}";
            SpeedText.Text = $"Speed: {_engine.SpeedLabel}";
        }
    }
}