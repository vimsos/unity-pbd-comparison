using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Kind = ParallelJobs.Pixel.Kind;

namespace ParallelJobs
{
    [BurstCompile]
    internal struct GravityJob : IJobParallelFor
    {
        public NativeArray<Pixel> Pixels;
        public Velocity Gravity;
        public Velocity GasVelocity;

        public void Execute(int i)
        {
            switch (Pixels[i].Type)
            {
                case Kind.Smoke:
                    Pixels[i] = Pixels[i].SetVelocity(GasVelocity);
                    break;
                default:
                    Pixels[i] = Pixels[i].Accelerate(Gravity);
                    break;
            }
        }
    }
}