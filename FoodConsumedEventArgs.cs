using System;

namespace SnakeGame
{
    public sealed class FoodConsumedEventArgs : EventArgs
    {
        public FoodType Type { get; }

        public FoodConsumedEventArgs(FoodType type)
        {
            Type = type;
        }
    }
}
