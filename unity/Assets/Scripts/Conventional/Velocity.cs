using UnityEngine;

namespace Conventional
{
    public readonly struct Velocity
    {
        public readonly sbyte X;
        public readonly sbyte Y;

        public Velocity(int x, int y)
        {
            X = (sbyte)x;
            Y = (sbyte)y;
        }

        public static implicit operator Vector2Int(Velocity v) => new Vector2Int(v.X, v.Y);

        public static implicit operator Velocity(Vector2Int v) => new Velocity(v.x, v.y);

        public static Velocity operator +(Velocity lhs, Velocity rhs) => new Velocity(lhs.X + rhs.X, lhs.Y + rhs.Y);

        public static Velocity operator -(Velocity v) => new Velocity(-v.X, -v.Y);
    }
}
