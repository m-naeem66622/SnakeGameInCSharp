using System;
using System.Timers;

namespace SnakeGame
{
    public sealed class GameEngine
    {
        private readonly Random _rnd = new();
        private readonly System.Timers.Timer _tickTimer;
        private readonly System.Timers.Timer _bonusTimer;
        private bool _isRunning;

        public int GridWidth { get; }
        public int GridHeight { get; }

        public Snake Snake { get; private set; }
        public Food? Food { get; private set; }
        public BonusFood? Bonus { get; private set; }

        public int Score { get; private set; }
        public int Level { get; private set; } = 1;
        public string SpeedLabel => _tickTimer.Interval switch
        {
            <= 80 => "Very Fast",
            <= 130 => "Fast",
            <= 200 => "Normal",
            _ => "Slow"
        };

        public event EventHandler? Updated;
        public event EventHandler? GameOver;

        public GameEngine(int gridWidth, int gridHeight)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;

            _tickTimer = new System.Timers.Timer(200);
            _tickTimer.Elapsed += (_, _) => Tick();

            _bonusTimer = new System.Timers.Timer(10000);
            _bonusTimer.Elapsed += (_, _) => TrySpawnBonus();

            Reset();
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _tickTimer.Start();
            _bonusTimer.Start();
            Updated?.Invoke(this, EventArgs.Empty);
        }

        public void TogglePause()
        {
            if (_tickTimer.Enabled)
            {
                _tickTimer.Stop();
                _bonusTimer.Stop();
            }
            else
            {
                _tickTimer.Start();
                _bonusTimer.Start();
            }
            Updated?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            _tickTimer.Stop();
            _bonusTimer.Stop();

            Score = 0;
            Level = 1;
            Snake = Snake.CreateCentered(GridWidth / 2, GridHeight / 2);
            Food = SpawnFood();
            Bonus = null;
            _tickTimer.Interval = 200;
            _isRunning = false;

            Updated?.Invoke(this, EventArgs.Empty);
        }

        public void SetDirection(Direction d) => Snake.ChangeDirection(d);

        internal void Tick()
        {
            // Move snake and check collisions
            Snake.Move();

            // Wall collision
            var head = Snake.Head;
            if (head.X < 0 || head.Y < 0 || head.X >= GridWidth || head.Y >= GridHeight || Snake.CollidesWithSelf())
            {
                _tickTimer.Stop();
                _bonusTimer.Stop();
                GameOver?.Invoke(this, EventArgs.Empty);
                return;
            }

            // Food
            if (Food is not null && head.Equals(Food.Position))
            {
                Snake.Grow();
                Score += Food.Points;
                Food = SpawnFood();
                OnScoreChanged();
            }

            // Bonus
            if (Bonus is not null && head.Equals(Bonus.Position))
            {
                Snake.Grow(3);
                Score += Bonus.Points;
                Bonus = null;
                OnScoreChanged();
            }

            Updated?.Invoke(this, EventArgs.Empty);
        }

        private void OnScoreChanged()
        {
            // level-up every 10 points
            var newLevel = 1 + (Score / 10);
            if (newLevel != Level)
            {
                Level = newLevel;
                // speed up: exponential-ish
                var newInterval = Math.Max(40, 200 - (Level - 1) * 22);
                _tickTimer.Interval = newInterval;
            }
        }

        private Food SpawnFood()
        {
            var pos = GetFreePosition();
            return new Food(pos, 1);
        }

        private void TrySpawnBonus()
        {
            // spawn bonus occasionally
            if (Bonus is null && _rnd.NextDouble() < 0.45)
            {
                Bonus = new BonusFood(GetFreePosition(), 5, TimeSpan.FromSeconds(6));
                Updated?.Invoke(this, EventArgs.Empty);

                // auto remove after time
                var t = new System.Timers.Timer(Bonus.Lifespan.TotalMilliseconds)
                {
                    AutoReset = false
                };
                t.Elapsed += (_, _) =>
                {
                    Bonus = null;
                    Updated?.Invoke(this, EventArgs.Empty);
                    t.Dispose();
                };
                t.Start();
            }
        }

        private GridPosition GetFreePosition()
        {
            GridPosition pos;
            do
            {
                pos = new GridPosition(_rnd.Next(0, GridWidth), _rnd.Next(0, GridHeight));
            } while (Snake.Segments.Contains(pos) || (Food?.Position.Equals(pos) ?? false));
            return pos;
        }
    }
}
