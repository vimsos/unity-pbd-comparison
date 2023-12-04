using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Kind = Jobs.Pixel.Kind;

namespace Jobs
{
    [BurstCompile]
    internal struct GravityJob : IJob
    {
        public NativeArray<Pixel> Pixels;
        public Velocity Gravity;
        public Velocity GasVelocity;

        public void Execute()
        {
            int length = Pixels.Length;
            for (int i = 0; i < length; i++)
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
}