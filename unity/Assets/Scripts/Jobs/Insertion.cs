using Unity.Burst;
using Kind = Jobs.Pixel.Kind;

namespace Jobs
{
    [BurstCompile]
    public readonly struct Insertion
    {
        public readonly Kind Type;
        public readonly int Index;

        public Insertion(Kind type, int index)
        {
            Type = type;
            Index = index;
        }
    }
}