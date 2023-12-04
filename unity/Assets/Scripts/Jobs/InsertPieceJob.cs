using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

namespace Jobs
{
    [BurstCompile]
    public struct InsertPieceJob : IJob
    {
        [WriteOnly] public NativeArray<Pixel> Pixels;
        public NativeList<Insertion> Insertions;

        public void Execute()
        {
            for (int i = 0; i < Insertions.Length; i++)
            {
                Pixels[Insertions[i].Index] = new Pixel(Insertions[i].Type);
            }
            Insertions.Clear();
        }
    }
}