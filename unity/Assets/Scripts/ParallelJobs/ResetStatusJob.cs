using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace ParallelJobs
{
    [BurstCompile]
    internal struct ResetStatusJob : IJobParallelFor
    {
        public NativeArray<Pixel> Pixels;

        public void Execute(int i)
        {
            Pixels[i] = Pixels[i].ResetStatus();
        }
    }
}