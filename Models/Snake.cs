using System.Collections.Generic;
using System.Linq;

namespace SnakeGame
{
    public enum Direction { Up, Down, Left, Right }

    public sealed class Snake
    {
        public List<GridPosition> Segments { get; private set; } = new();
        private Direction _direction;
        private Direction _desiredDirection;
        private int _pendingGrowth;

        public GridPosition Head => Segments.First();

        private Snake(List<GridPosition> segments, Direction dir)
        {
            Segments = segments;
            _direction = dir;
            _desiredDirection = dir;
        }

        public static Snake CreateCentered(int x, int y)
        {
            var segs = new List<GridPosition> { new GridPosition(x, y), new GridPosition(x - 1, y), new GridPosition(x - 2, y) };
            return new Snake(segs, Direction.Right);
        }

        public void ChangeDirection(Direction d)
        {
            // prevent reversing relative to current motion
            if ((_direction == Direction.Left && d == Direction.Right) ||
                (_direction == Direction.Right && d == Direction.Left) ||
                (_direction == Direction.Up && d == Direction.Down) ||
                (_direction == Direction.Down && d == Direction.Up))
            {
                return;
            }

            if (_desiredDirection == d)
            {
                return;
            }

            _desiredDirection = d;
        }

        public void Move()
        {
            _direction = _desiredDirection;
            var head = Head;
            GridPosition next = _direction switch
            {
                Direction.Up => new GridPosition(head.X, head.Y - 1),
                Direction.Down => new GridPosition(head.X, head.Y + 1),
                Direction.Left => new GridPosition(head.X - 1, head.Y),
                Direction.Right => new GridPosition(head.X + 1, head.Y),
                _ => head
            };

            Segments.Insert(0, next);

            if (_pendingGrowth > 0)
            {
                _pendingGrowth--;
            }
            else
            {
                Segments.RemoveAt(Segments.Count - 1);
            }
        }

        public void Grow(int amount = 1) => _pendingGrowth += amount;

        public bool CollidesWithSelf()
        {
            var head = Head;
            return Segments.Skip(1).Any(p => p.Equals(head));
        }
    }
}
