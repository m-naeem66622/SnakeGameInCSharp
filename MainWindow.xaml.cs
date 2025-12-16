using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        private readonly GameEngine _engine;
        private readonly Color _snakeHeadColor = Color.FromRgb(0x7E, 0xFF, 0xC6);
        private readonly Color _snakeTailColor = Color.FromRgb(0x05, 0x3A, 0x32);
        private readonly Color _snakeHighlightColor = Color.FromRgb(0xA3, 0xFF, 0xF7);
        private readonly Brush _foodBrush = new SolidColorBrush(Color.FromRgb(0x3E, 0xE3, 0xA7));
        private readonly Brush _bonusBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xD1, 0x66));
        private readonly Color _boardBaseColor = Color.FromRgb(0x04, 0x0A, 0x12);
        private readonly Color _gridLineColor = Color.FromArgb(110, 0x15, 0x23, 0x3B);
        private readonly List<(int Score, DateTime CompletedAt)> _scoreHistory = new();
        private int _bestScore;
        private Brush? _cachedGridBrush;
        private Size _cachedCellSize = Size.Empty;

        public MainWindow()
        {
            InitializeComponent();

            // create engine: grid 40x30 (responsive)
            _engine = new GameEngine(40, 30);
            _engine.Updated += Engine_Updated;
            _engine.GameOver += Engine_GameOver;
            SizeChanged += MainWindow_SizeChanged;
            Loaded += (_, _) => ApplyResponsiveLayout();

            RenderAll();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RenderAll();
            ApplyResponsiveLayout();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e) => TogglePlayPause();

        private void ScoreHistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new ScoreHistoryWindow(_scoreHistory, _bestScore)
            {
                Owner = this
            };

            historyWindow.ShowDialog();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            HideGameOverOverlay();
            _engine.Reset();
            UpdatePlayPauseButton();
        }

        private void ApplyResponsiveLayout()
        {
            if (ResponsiveLayoutGrid is null || SidebarHost is null || PrimaryColumn is null || SidebarColumn is null || SidebarRow is null)
            {
                return;
            }

            var layoutWidth = ResponsiveLayoutGrid.ActualWidth;
            if (layoutWidth <= 0)
            {
                layoutWidth = ActualWidth - 64;
            }

            var isNarrow = layoutWidth < 1100;

            if (isNarrow)
            {
                Grid.SetColumn(SidebarHost, 0);
                Grid.SetRow(SidebarHost, 1);
                SidebarHost.Margin = new Thickness(0, 24, 0, 0);
                SidebarColumn.Width = new GridLength(0);
                PrimaryColumn.Width = new GridLength(1, GridUnitType.Star);
                SidebarRow.Height = GridLength.Auto;
            }
            else
            {
                Grid.SetColumn(SidebarHost, 1);
                Grid.SetRow(SidebarHost, 0);
                SidebarHost.Margin = new Thickness(24, 0, 0, 0);
                SidebarColumn.Width = new GridLength(1.6, GridUnitType.Star);
                PrimaryColumn.Width = new GridLength(3, GridUnitType.Star);
                SidebarRow.Height = new GridLength(0);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var handled = true;

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
                    TogglePlayPause();
                    break;
                default:
                    handled = false;
                    break;
            }

            if (handled)
            {
                e.Handled = true;
            }
        }

        private void Engine_GameOver(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                RecordScore(_engine.Score);
                ShowGameOverOverlay();
                UpdatePlayPauseButton();
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

            UpdateGridBackground(cellW, cellH);

            // draw snake with neon trail
            var segments = _engine.Snake.Segments.ToList();
            var totalSegments = segments.Count;
            for (var i = 0; i < totalSegments; i++)
            {
                var segment = segments[i];
                var isHead = i == 0;
                var progress = totalSegments <= 1 ? 0d : (double)i / (totalSegments - 1);

                var rect = new Rectangle
                {
                    Width = Math.Max(4, cellW - 4),
                    Height = Math.Max(4, cellH - 4),
                    RadiusX = isHead ? 10 : 8,
                    RadiusY = isHead ? 10 : 8,
                    Fill = CreateSegmentBrush(progress, isHead),
                    Opacity = isHead ? 1.0 : 0.9,
                    StrokeThickness = isHead ? 1.2 : 0
                };

                if (isHead)
                {
                    rect.Stroke = new SolidColorBrush(Color.FromArgb(180, 255, 255, 255));
                }

                rect.Effect = CreateSegmentGlow(progress, isHead);

                Canvas.SetLeft(rect, segment.X * cellW + 2);
                Canvas.SetTop(rect, segment.Y * cellH + 2);
                GameCanvas.Children.Add(rect);
            }

            var baseFoodWidth = Math.Max(6, cellW - 6);
            var baseFoodHeight = Math.Max(6, cellH - 6);

            // draw food
            if (_engine.Food is not null)
            {
                var f = new Ellipse
                {
                    Width = baseFoodWidth,
                    Height = baseFoodHeight,
                    Fill = _foodBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.6,
                    Opacity = 0.95
                };
                Canvas.SetLeft(f, _engine.Food.Position.X * cellW + (cellW - baseFoodWidth) / 2);
                Canvas.SetTop(f, _engine.Food.Position.Y * cellH + (cellH - baseFoodHeight) / 2);
                GameCanvas.Children.Add(f);
            }

            // draw bonus
            if (_engine.Bonus is not null)
            {
                var bonusWidth = Math.Min(cellW - 2, baseFoodWidth * 2);
                var bonusHeight = Math.Min(cellH - 2, baseFoodHeight * 2);
                var b = new Ellipse
                {
                    Width = bonusWidth,
                    Height = bonusHeight,
                    Fill = _bonusBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 0.6,
                    Opacity = 0.95
                };
                Canvas.SetLeft(b, _engine.Bonus.Position.X * cellW + (cellW - bonusWidth) / 2);
                Canvas.SetTop(b, _engine.Bonus.Position.Y * cellH + (cellH - bonusHeight) / 2);
                GameCanvas.Children.Add(b);
            }

            // HUD updates
            ScoreText.Text = _engine.Score.ToString();
            LevelText.Text = _engine.Level.ToString();
            SpeedText.Text = _engine.SpeedLabel;
            _bestScore = Math.Max(_bestScore, _engine.Score);
            UpdateBestScoreDisplay();
            UpdatePlayPauseButton();
        }

        private void UpdateGridBackground(double cellW, double cellH)
        {
            if (GameCanvas is null || cellW <= 0 || cellH <= 0)
            {
                return;
            }

            if (Math.Abs(_cachedCellSize.Width - cellW) < 0.01 && Math.Abs(_cachedCellSize.Height - cellH) < 0.01 && _cachedGridBrush is not null)
            {
                GameCanvas.Background = _cachedGridBrush;
                return;
            }

            var baseBrush = new SolidColorBrush(_boardBaseColor);
            baseBrush.Freeze();
            var gridBrush = new SolidColorBrush(_gridLineColor);
            gridBrush.Freeze();

            var geometry = new RectangleGeometry(new Rect(0, 0, cellW, cellH));
            var pen = new Pen(gridBrush, 0.8)
            {
                DashCap = PenLineCap.Flat,
                StartLineCap = PenLineCap.Flat,
                EndLineCap = PenLineCap.Flat
            };
            pen.Freeze();

            var drawing = new GeometryDrawing(baseBrush, pen, geometry);
            drawing.Freeze();

            var tiledBrush = new DrawingBrush(drawing)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, cellW, cellH),
                ViewportUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None
            };
            tiledBrush.Freeze();

            GameCanvas.Background = tiledBrush;
            _cachedGridBrush = tiledBrush;
            _cachedCellSize = new Size(cellW, cellH);
        }

        private void TogglePlayPause()
        {
            if (!_engine.IsRunning)
            {
                if (IsGameOverOverlayVisible())
                {
                    HideGameOverOverlay();
                    _engine.Reset();
                }

                _engine.Start();
            }
            else
            {
                _engine.TogglePause();
            }

            UpdatePlayPauseButton();
        }

        private void UpdatePlayPauseButton()
        {
            if (PlayPauseButton is null)
            {
                return;
            }

            var showPlay = !_engine.IsRunning || !_engine.IsTicking;
            PlayPauseButton.Content = showPlay ? "Play" : "Pause";

            var accent = TryFindResource("AccentBrush") as Brush ?? new SolidColorBrush(Color.FromRgb(0x66, 0xF7, 0xD5));
            var secondary = TryFindResource("AccentBrushSecondary") as Brush ?? new SolidColorBrush(Color.FromRgb(0x8A, 0xA5, 0xFF));

            PlayPauseButton.Background = showPlay ? accent : new SolidColorBrush(Color.FromRgb(0x1A, 0x23, 0x38));
            PlayPauseButton.BorderBrush = showPlay ? accent : secondary;
            PlayPauseButton.Foreground = showPlay ? new SolidColorBrush(Color.FromRgb(0x04, 0x12, 0x1F)) : Brushes.White;
        }

        private void UpdateBestScoreDisplay()
        {
            if (BestScoreText is null)
            {
                return;
            }

            BestScoreText.Text = $"Best: {_bestScore}";
        }

        private void ShowGameOverOverlay()
        {
            if (GameOverOverlay is null || GameOverScoreText is null)
            {
                return;
            }

            GameOverScoreText.Text = $"Score: {_engine.Score}";
            GameOverOverlay.Visibility = Visibility.Visible;
        }

        private void HideGameOverOverlay()
        {
            if (GameOverOverlay is null)
            {
                return;
            }

            GameOverOverlay.Visibility = Visibility.Collapsed;
        }

        private bool IsGameOverOverlayVisible() => GameOverOverlay is not null && GameOverOverlay.Visibility == Visibility.Visible;

        private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            HideGameOverOverlay();
            _engine.Reset();
            _engine.Start();
            UpdatePlayPauseButton();
        }

        private void RecordScore(int score)
        {
            if (score < 0)
            {
                score = 0;
            }

            _scoreHistory.Add((score, DateTime.Now));
            if (score > _bestScore)
            {
                _bestScore = score;
            }

            UpdateBestScoreDisplay();
        }

        private Brush CreateSegmentBrush(double progress, bool isHead)
        {
            var startColor = InterpolateColor(_snakeHeadColor, _snakeTailColor, progress * 0.6);
            var highlight = InterpolateColor(_snakeHighlightColor, _snakeTailColor, progress);
            var angle = isHead ? 20 : 70;

            return new LinearGradientBrush(highlight, startColor, angle)
            {
                MappingMode = BrushMappingMode.RelativeToBoundingBox
            };
        }

        private DropShadowEffect CreateSegmentGlow(double progress, bool isHead)
        {
            var glowColor = InterpolateColor(_snakeHighlightColor, _snakeTailColor, progress);
            return new DropShadowEffect
            {
                Color = glowColor,
                BlurRadius = isHead ? 26 : 18,
                ShadowDepth = 0,
                Opacity = isHead ? 0.95 : 0.65
            };
        }

        private static Color InterpolateColor(Color start, Color end, double progress)
        {
            progress = Math.Clamp(progress, 0d, 1d);
            byte r = (byte)(start.R + (end.R - start.R) * progress);
            byte g = (byte)(start.G + (end.G - start.G) * progress);
            byte b = (byte)(start.B + (end.B - start.B) * progress);
            return Color.FromRgb(r, g, b);
        }
    }
}