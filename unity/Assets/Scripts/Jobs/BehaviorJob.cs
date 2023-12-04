using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using Kind = Jobs.Pixel.Kind;

namespace Jobs
{
    [BurstCompile]
    internal struct BehaviorJob : IJob
    {
        public NativeArray<Pixel> Pixels;
        public WorldHelper World;
        public uint Tick;

        public void Execute()
        {
            for (int y = 0; y < World.Height; y++)
            {
                for (int x = World.Width - 1; x >= 0; x--)
                {
                    HandlePixelAt(x, y);
                }
            }
        }

        void HandlePixelAt(int x, int y)
        {
            var position = new int2(x, y);
            var index = World.PositionToIndex(position);

            if (Pixels[index].IsProcessed()) { return; }

            var nextIndex = Pixels[index].Type switch
            {
                Kind.Sand => HandleSand(index, position),
                Kind.Water => HandleWater(index, position),
                Kind.Smoke => HandleSmoke(index, position),
                Kind.Lava => HandleLava(index, position),
                Kind.Rock => HandleBedrock(index, position),
                _ => index
            };

            Pixels[index] = Pixels[index].SetProcessed();
            Pixels.Swap(index, nextIndex);
        }

        static readonly int2[] SAND_NEIGHBORS = new int2[2] { new int2(1, -1), new int2(-1, -1) };
        int HandleSand(int index, int2 position)
        {
            var nextIndex = index;
            var belowIndex = World.PositionSumToIndex(position, DOWN);
            var belowType = Pixels[belowIndex].Type;

            if (belowType != Kind.Empty)
            {
                for (int ni = 0; ni < SAND_NEIGHBORS.Length; ni++)
                {
                    var neighborIndex = World.PositionSumToIndex(position, SAND_NEIGHBORS[ni]);
                    var neighborType = Pixels[neighborIndex].Type;

                    if (neighborType == Kind.Empty)
                    {
                        nextIndex = neighborIndex;
                        break;
                    }
                }
            }

            return nextIndex;
        }

        static readonly int2[] WATER_NEIGHBORS = new int2[4] { new int2(-1, -1), new int2(1, -1), new int2(-1, 0), new int2(1, 0) };
        int HandleWater(int index, int2 position)
        {
            var nextIndex = index;
            var belowIndex = World.PositionSumToIndex(position, DOWN);
            var aboveIndex = World.PositionSumToIndex(position, UP);
            var aboveType = Pixels[aboveIndex].Type;
            var belowType = Pixels[belowIndex].Type;

            if (aboveType == Kind.Sand)
            {
                nextIndex = aboveIndex;
            }
            else if (belowType != Kind.Empty)
            {
                for (int ni = 0; ni < WATER_NEIGHBORS.Length; ni++)
                {
                    var neighborIndex = World.PositionSumToIndex(position, WATER_NEIGHBORS[ni]);
                    var neighborType = Pixels[neighborIndex].Type;

                    if (neighborType == Kind.Empty || neighborType == Kind.Smoke)
                    {
                        nextIndex = neighborIndex;
                        break;
                    }
                }
            }

            return nextIndex;
        }

        static readonly int2[] SMOKE_NEIGHBORS = new int2[8] { new int2(1, 1), new int2(-1, 1), new int2(-1, 0), new int2(1, 0), new int2(0, 1), new int2(-1, 1), new int2(-1, -1), new int2(0, -1) };
        int HandleSmoke(int index, int2 position)
        {
            if ((position.x * position.y + Tick) % 48 == 0)
            {
                Pixels[index] = new Pixel(Kind.Empty);
                return index;
            }

            var nextIndex = index;

            for (int ni = 0; ni < SMOKE_NEIGHBORS.Length; ni++)
            {
                var neighborIndex = World.PositionSumToIndex(position, SMOKE_NEIGHBORS[ni]);
                var neighborType = Pixels[neighborIndex].Type;

                if (neighborType == Kind.Empty)
                {
                    nextIndex = neighborIndex;
                    break;
                }
            }

            return nextIndex;
        }

        static readonly int2[] LAVA_NEIGHBORS = new int2[4] { new int2(-1, 0), new int2(1, 0), new int2(0, 1), new int2(0, -1) };
        int HandleLava(int index, int2 position)
        {
            var interactingNeighbors = 0;

            for (int ni = 0; ni < LAVA_NEIGHBORS.Length; ni++)
            {
                var neighborIndex = World.PositionSumToIndex(position, LAVA_NEIGHBORS[ni]);
                var neighborType = Pixels[neighborIndex].Type;

                switch (neighborType)
                {
                    case Kind.Water:
                        interactingNeighbors++;
                        Pixels[neighborIndex] = new Pixel(Kind.Smoke);
                        break;
                    case Kind.Sand:
                        interactingNeighbors++;
                        Pixels[neighborIndex] = new Pixel(Kind.Rock);
                        break;
                    case Kind.Empty:
                        if (interactingNeighbors > 0)
                        {
                            Pixels[neighborIndex] = new Pixel(Kind.Smoke);
                        }
                        break;
                }
            }

            if ((position.x * position.y + Tick) % 8 < interactingNeighbors)
            {
                Pixels[index] = new Pixel(Kind.Rock);
                return index;
            }

            var nextIndex = index;
            var belowIndex = World.PositionSumToIndex(position, DOWN);
            var belowType = Pixels[belowIndex].Type;

            if (belowType != Kind.Empty)
            {
                for (int ni = 0; ni < WATER_NEIGHBORS.Length; ni++)
                {
                    var neighborIndex = World.PositionSumToIndex(position, WATER_NEIGHBORS[ni]);
                    var neighborType = Pixels[neighborIndex].Type;

                    if (neighborType == Kind.Empty || neighborType == Kind.Smoke)
                    {
                        nextIndex = neighborIndex;
                        break;
                    }
                }
            }

            return nextIndex;
        }

        int HandleBedrock(int index, int2 position)
        {
            var nextIndex = index;
            var belowIndex = World.PositionSumToIndex(position, DOWN);
            var belowType = Pixels[belowIndex].Type;

            if (belowType == Kind.Lava)
            {
                var decision = (position.x * position.y + Tick) % 8 == 0;
                nextIndex = decision ? belowIndex : index;
            }

            return nextIndex;
        }

        static readonly int2
            LEFT = new int2(-1, 0),
            RIGHT = new int2(1, 0),
            UP = new int2(0, 1),
            DOWN = new int2(0, -1),
            LEFT_UP = new int2(-1, 1),
            LEFT_DOWN = new int2(-1, -1),
            RIGHT_UP = new int2(1, 1),
            RIGHT_DOWN = new int2(1, -1);
    }
}
