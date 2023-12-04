using System;
using System.Linq;
using System.Collections.Generic;

namespace Common
{
    [Serializable]
    public class RecordedInput
    {
        public int Width;
        public int Height;
        public int GravityX;
        public int GravityY;
        public int Ticks;
        public List<Insertion> Inputs = new List<Insertion>();
        public List<byte> InitialState = new List<byte>();

        public IEnumerable<Insertion> InsertionsAtTick(uint tick)
        {
            return Inputs.Where(i => i.Tick == tick);
        }

        [Serializable]
        public class Insertion
        {
            public byte Type;
            public uint Tick;
            public int Index;
            public int X;
            public int Y;
        }
    }
}

