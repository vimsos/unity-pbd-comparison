using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Common;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Benchmark
{
    public static class Utils
    {
        public const string DATETIME_FORMAT = "dd-MMM-HH-mm-ss";
        public const string DURATION_FORMAT = "mm\\:ss\\.fff";
        public const string SPEEDUP_FORMAT = "0.0000x";

#if ENABLE_MONO
        public const string COMPILER_BACKEND = "mono";
#elif ENABLE_IL2CPP
        public const string COMPILER_BACKEND = "il2cpp";
#endif

        public static string DeviceModel => SystemInfo.deviceModel.Sanitized();
        public static string DeviceProcessor => SystemInfo.processorType.Sanitized();
        public static string ProcessorArch => System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString().Sanitized();

        public static bool CompareRawTexture(NativeArray<Color32> lhs, NativeArray<Color32> rhs)
        {
            if (lhs.Length != rhs.Length)
            { return false; }

            for (int i = 0; i < lhs.Length; i++)
            {
                if (!(lhs[i].Equals(rhs[i])))
                { return false; }
            }

            return true;
        }

        public static IWorld NewWorld(RecordedInput recordedInput, SimulationType type)
        {
            return type switch
            {
                SimulationType.Conventional => new Conventional.World
                (
                    width: recordedInput.Width,
                    height: recordedInput.Height,
                    gravity: new Conventional.Velocity(recordedInput.GravityX, recordedInput.GravityY),
                    tick: 0,
                    initialState: recordedInput.InitialState.Select(piece => (Conventional.Pixel.Kind)piece).ToList()
                ),
                SimulationType.Jobs => new Jobs.World
                (
                    width: recordedInput.Width,
                    height: recordedInput.Height,
                    gravity: new Jobs.Velocity(recordedInput.GravityX, recordedInput.GravityY),
                    tick: 0,
                    initialState: recordedInput.InitialState.Select(piece => (Jobs.Pixel.Kind)piece).ToList()
                ),
                SimulationType.ParallelJobs => new ParallelJobs.World
                (
                    width: recordedInput.Width,
                    height: recordedInput.Height,
                    gravity: new ParallelJobs.Velocity(recordedInput.GravityX, recordedInput.GravityY),
                    tick: 0,
                    initialState: recordedInput.InitialState.Select(piece => (ParallelJobs.Pixel.Kind)piece).ToList()
                ),
                _ => throw new Exception("Unrecognized SimulationType")
            };
        }

        public static (Texture2D, NativeArray<Color32>) SetupTextures(IWorld world, MeshRenderer renderer)
        {
            var texture = new Texture2D(world.Width, world.Height);
            texture.filterMode = FilterMode.Point;
            var rawTexture = texture.GetRawTextureData<Color32>();
            renderer.material.SetTexture("_MainTex", texture);

            return (texture, rawTexture);
        }

        public static RecordedInput GenerateInputPattern(int size, int ticks)
        {
            var types = new byte[] { 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 5 };

            int area = size * size;
            int insertionsPerTick = (int)(area * 0.001);
            var insertions = new List<RecordedInput.Insertion>(insertionsPerTick * ticks);
            var initialState = new List<byte>(new byte[area]);
            System.Random random = new System.Random(area);

            for (uint t = 0; t < ticks; t++)
            {
                for (int i = 0; i < insertionsPerTick; i++)
                {
                    int x = random.Next() % size;
                    int y = random.Next() % size;
                    int index = y * size + x;
                    insertions.Add(new RecordedInput.Insertion
                    {
                        Type = types[(random.Next() % types.Length)],
                        Tick = t,
                        X = x,
                        Y = y,
                        Index = index
                    });
                }
            }

            return new RecordedInput
            {
                Width = size,
                Height = size,
                GravityX = 0,
                GravityY = -1,
                Ticks = ticks,
                Inputs = insertions,
                InitialState = initialState
            };
        }

        public static TickSample TimedSimulationRun(IWorld world, Texture2D texture, NativeArray<Color32> rawTexture, Stopwatch stopwatch)
        {
            stopwatch.Restart();
            world.InsertNewPixels();
            var insert = stopwatch.Elapsed;

            stopwatch.Restart();
            world.ApplyGravity();
            var gravity = stopwatch.Elapsed;

            stopwatch.Restart();
            world.CalculateMovement();
            var movement = stopwatch.Elapsed;

            stopwatch.Restart();
            world.ApplyBehavior();
            var behavior = stopwatch.Elapsed;

            stopwatch.Restart();
            world.ResetStatus();
            var reset = stopwatch.Elapsed;

            stopwatch.Restart();
            world.DrawToTexture(rawTexture);
            texture.Apply();
            var paint = stopwatch.Elapsed;
            stopwatch.Reset();

            world.IncreaseTick();

            return new TickSample(insert, gravity, movement, behavior, reset, paint);
        }

        public static void WarmUpSimulationRun(IWorld world)
        {
            using var rawTexture = new NativeArray<Color32>(world.Width * world.Height, Allocator.TempJob);

            world.InsertNewPixels();
            world.ApplyGravity();
            world.CalculateMovement();
            world.ApplyBehavior();
            world.ResetStatus();
            world.DrawToTexture(rawTexture);
            world.IncreaseTick();
        }

        private static char[] badCharacters = new char[] { '(', ')', '@', '.', };
        public static string Sanitized(this string input)
        {
            return String.Concat(input.Where(c => !Char.IsWhiteSpace(c) && !badCharacters.Contains(c)));
        }

        public static void Prepend(this Text textDisplay, string newLine)
        {
            textDisplay.text = newLine + "\n" + textDisplay.text;
        }
    }
}