using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Common;
using Kind = Jobs.Pixel.Kind;

namespace Jobs
{
    [BurstCompile]
    public partial class World : IWorld
    {
        public int Width => width;
        public int Height => height;
        public int Total => total;
        public Velocity Gravity => gravity;
        public uint Tick => tick;

        readonly int width;
        readonly int height;
        readonly int total;
        readonly Velocity gravity;
        readonly Velocity gasVelocity;
        readonly NativeList<Insertion> insertions;
        readonly NativeArray<Pixel> pixels;
        readonly WorldHelper helper;

        uint tick;

        public World(int width, int height, Velocity gravity, uint tick, List<Kind> initialState)
        {
            const int INSERTION_LIST_CAPACITY = 128;

            this.width = width;
            this.height = height;
            this.gravity = gravity;
            this.tick = tick;

            gasVelocity = -gravity + new Velocity(0, 1);
            total = this.width * this.height;
            pixels = new NativeArray<Pixel>(total, Allocator.Persistent);
            insertions = new NativeList<Insertion>(INSERTION_LIST_CAPACITY, Allocator.Persistent);
            helper = new WorldHelper(width, height);

            for (int i = 0; i < total; i++)
            {
                pixels[i] = new Pixel(initialState[i]);
            }
        }

        public void DrawToTexture(NativeArray<Color32> rawTexture)
        {
            var draw = new DrawWorldJob
            {
                Pixels = pixels,
                Texture = rawTexture
            };

            var drawHandle = draw.Schedule();
            drawHandle.Complete();
        }

        public void Dispose()
        {
            pixels.Dispose();
            insertions.Dispose();
        }

        public void Insert(IEnumerable<RecordedInput.Insertion> newInsertions)
        {
            foreach (var i in newInsertions)
            {
                insertions.Add(new Insertion((Kind)i.Type, i.Index));
            }
        }
    }
}