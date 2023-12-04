using System.Collections.Generic;
using Common;
using Unity.Collections;
using UnityEngine;
using Kind = Conventional.Pixel.Kind;

namespace Conventional
{
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
        readonly List<Insertion> insertions;
        readonly Pixel[,] pixels;

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
            pixels = new Pixel[width, height];
            insertions = new List<Insertion>(INSERTION_LIST_CAPACITY);

            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[x, y] = new Pixel(initialState[i]);
                    i++;
                }
            }
        }

        public void DrawToTexture(NativeArray<Color32> rawTexture)
        {
            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rawTexture[i++] = pixels[x, y].Color;
                }
            }
        }

        void Swap(Vector2Int a, Vector2Int b)
        {
            (pixels[a.x, a.y], pixels[b.x, b.y]) = (pixels[b.x, b.y], pixels[a.x, a.y]);
        }

        Vector2Int PositionSum(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int
            (
                x: Mathf.Clamp(a.x + b.x, 0, width - 1),
                y: Mathf.Clamp(a.y + b.y, 0, height - 1)
            );
        }

        public void Insert(IEnumerable<RecordedInput.Insertion> newInsertions)
        {
            foreach (var i in newInsertions)
            {
                insertions.Add(new Insertion((Kind)i.Type, i.X, i.Y));
            }
        }

        public void Dispose() { }
    }
}