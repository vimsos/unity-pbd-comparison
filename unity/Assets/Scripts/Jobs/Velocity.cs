using Unity.Burst;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public readonly struct Velocity
    {
        public readonly sbyte X;
        public readonly sbyte Y;

        public Velocity(int x, int y)
        {
            X = (sbyte)x;
            Y = (sbyte)y;
        }

        public static implicit operator int2(Velocity v) => new int2(v.X, v.Y);

        public static implicit operator Velocity(int2 v) => new Velocity(v.x, v.y);

        public static Velocity operator +(Velocity lhs, Velocity rhs) => new Velocity(lhs.X + rhs.X, lhs.Y + rhs.Y);

        public static Velocity operator -(Velocity v) => new Velocity(-v.X, -v.Y);
    }
}
