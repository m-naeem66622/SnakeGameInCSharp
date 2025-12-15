using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeGame
{
    public class Food
    {
        public GridPosition Position { get; }
        public int Points { get; }

        public Food(GridPosition position, int points)
        {
            Position = position;
            Points = points;
        }
    }
}
