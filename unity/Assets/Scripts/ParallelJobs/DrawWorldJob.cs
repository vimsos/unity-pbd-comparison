using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace ParallelJobs
{
    [BurstCompile]
    public struct DrawWorldJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Pixel> Pixels;
        [WriteOnly] public NativeArray<Color32> Texture;

        public void Execute(int i)
        {
            Texture[i] = Pixels[i].Color;
        }
    }
}