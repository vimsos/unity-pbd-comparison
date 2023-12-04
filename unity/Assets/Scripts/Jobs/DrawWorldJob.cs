using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Jobs
{
    [BurstCompile]
    public struct DrawWorldJob : IJob
    {
        [ReadOnly] public NativeArray<Pixel> Pixels;
        [WriteOnly] public NativeArray<Color32> Texture;

        public void Execute()
        {
            int length = Pixels.Length;
            for (int i = 0; i < length; i++)
            {
                Texture[i] = Pixels[i].Color;
            }
        }
    }
}