using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Jobs
{
    [BurstCompile]
    internal struct ResetStatusJob : IJob
    {
        public NativeArray<Pixel> Pixels;

        public void Execute()
        {
            int length = Pixels.Length;
            for (int i = 0; i < length; i++)
            {
                Pixels[i] = Pixels[i].ResetStatus();
            }
        }
    }
}