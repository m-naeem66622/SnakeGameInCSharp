using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    public enum FoodType
    {
        Normal,
        Fast,
        Slow,
        Bonus
    }

    public class Food
    {
        public GridPosition Position { get; }
        public int Points { get; }
        public FoodType Type { get; }

        public Food(GridPosition position, int points, FoodType type = FoodType.Normal)
        {
            Position = position;
            Points = points;
            Type = type;
        }
    }
}
