using Common;
using Unity.Jobs;

namespace ParallelJobs
{
    public partial class World : IWorld
    {
        const int BATCH_SIZE = 128;

        public void InsertNewPixels() => new InsertPieceJob
        {
            Pixels = pixels,
            Insertions = insertions
        }
        .Schedule()
        .Complete();


        public void ApplyGravity()
        {
            new GravityJob
            {
                Pixels = pixels,
                Gravity = gravity,
                GasVelocity = gasVelocity
            }
            .Schedule(total, BATCH_SIZE)
            .Complete();
        }

        public void CalculateMovement()
        {
            new KinematicJob
            {
                Pixels = pixels,
                World = helper
            }
            .Schedule()
            .Complete();
        }

        public void ApplyBehavior()
        {
            new BehaviorJob
            {
                Pixels = pixels,
                World = helper,
                Tick = tick
            }
            .Schedule()
            .Complete();
        }

        public void ResetStatus()
        {
            new ResetStatusJob
            {
                Pixels = pixels
            }
            .Schedule(total, BATCH_SIZE)
            .Complete();
        }

        public void IncreaseTick()
        {
            tick++;
        }
    }
}