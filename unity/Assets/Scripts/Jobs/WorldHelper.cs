using Unity.Burst;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public readonly struct WorldHelper
    {
        public readonly int Width;
        public readonly int Height;

        public int2 PositionSum(int2 a, int2 b)
        {
            return new int2
            (
                x: math.clamp(a.x + b.x, 0, Width - 1),
                y: math.clamp(a.y + b.y, 0, Height - 1)
            );
        }

        public int PositionSumToIndex(int2 a, int2 b)
        {
            return PositionToIndex(PositionSum(a, b));
        }

        public int PositionToIndex(int2 p)
        {
            return math.clamp(p.x, 0, Width - 1) + Width * math.clamp(p.y, 0, Height - 1);
        }

        public int2 IndexToPosition(int i)
        {
            int y = math.clamp(i / Width, 0, Height - 1);
            int x = math.clamp(i - y * Width, 0, Width - 1);
            return new int2(x, y);
        }

        public WorldHelper(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
