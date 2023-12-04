using Common;
using Unity.Jobs;

namespace Jobs
{
    public partial class World : IWorld
    {
        public void InsertNewPixels()
        {
            new InsertPieceJob
            {
                Pixels = pixels,
                Insertions = insertions
            }
            .Schedule()
            .Complete();
        }

        public void ApplyGravity()
        {
            new GravityJob
            {
                Pixels = pixels,
                Gravity = gravity,
                GasVelocity = gasVelocity
            }
            .Schedule()
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
            .Schedule()
            .Complete();
        }

        public void IncreaseTick()
        {
            tick++;
        }
    }
}