using System;

namespace SnakeGame
{
    public sealed class BonusFood : Food
    {
        public TimeSpan Lifespan { get; }
        public BonusFood(GridPosition position, int points, TimeSpan lifespan)
            : base(position, points)
        {
            Lifespan = lifespan;
        }
    }
}
