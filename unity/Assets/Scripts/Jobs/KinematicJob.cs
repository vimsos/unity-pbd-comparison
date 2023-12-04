using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Kind = Jobs.Pixel.Kind;

namespace Jobs
{
    [BurstCompile]
    internal struct KinematicJob : IJob
    {
        public NativeArray<Pixel> Pixels;
        public WorldHelper World;

        public void Execute()
        {
            for (int i = 0; i < Pixels.Length; i++)
            {
                if (Pixels[i].Type == Kind.Empty || Pixels[i].Type == Kind.Rock || Pixels[i].IsMoved()) { continue; }

                var position = World.IndexToPosition(i);
                var nextIndex = i;
                var nextPosition = position;
                var targetPosition = World.PositionSum(position, Pixels[i].Velocity);
                var step = new int2
                (
                    x: math.clamp(Pixels[i].Velocity.X, -1, 1),
                    y: math.clamp(Pixels[i].Velocity.Y, -1, 1)
                );
                int totalSteps = math.max(math.abs(Pixels[i].Velocity.X), math.abs(Pixels[i].Velocity.Y));

                for (int s = 0; s < totalSteps; s++)
                {
                    int2 candidate1 = new int2(nextPosition.x + step.x, nextPosition.y);
                    int2 candidate2 = new int2(nextPosition.x + step.x, nextPosition.y + step.y);
                    int2 candidate3 = new int2(nextPosition.x, nextPosition.y + step.y);

                    int2 candidate = GetClosest(candidate1, candidate2, candidate3, targetPosition);
                    int candidateIndex = World.PositionToIndex(candidate);

                    if (Pixels[candidateIndex].Type == Kind.Empty)
                    {
                        nextPosition = candidate;
                        nextIndex = candidateIndex;
                        continue;
                    }
                    else { break; }
                }

                Velocity actualVelocity = nextPosition - position;
                Pixels[i] = Pixels[i].SetVelocity(actualVelocity).SetMoved();
                Pixels.Swap(i, nextIndex);
            }

            int2 GetClosest(int2 p1, int2 p2, int2 p3, int2 target)
            {
                var d1 = (target.x - p1.x) * (target.x - p1.x) + (target.y - p1.y) * (target.y - p1.y);
                var d2 = (target.x - p2.x) * (target.x - p2.x) + (target.y - p2.y) * (target.y - p2.y);
                var d3 = (target.x - p3.x) * (target.x - p3.x) + (target.y - p3.y) * (target.y - p3.y);

                int2 result = p1;
                int squareDistance = d1;

                if (d2 < squareDistance)
                {
                    result = p2;
                    squareDistance = d2;
                }
                if (d3 < squareDistance)
                {
                    result = p3;
                }

                return result;
            }
        }
    }
}