using Kind = Conventional.Pixel.Kind;

namespace Conventional
{
    public readonly struct Insertion
    {
        public readonly Kind Type;
        public readonly int X;
        public readonly int Y;

        public Insertion(Kind type, int x, int y)
        {
            Type = type;
            X = x;
            Y = y;
        }
    }
}