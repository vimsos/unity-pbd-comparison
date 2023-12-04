using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using Common;
using UnityEngine;
using static Benchmark.Utils;

namespace Benchmark
{
    public enum SimulationType { Conventional = 0, Jobs = 1, ParallelJobs = 2 }

    public class BenchmarkResult
    {
        public readonly SimulationType Type;
        public readonly DateTime DateTime;
        public readonly int NumberOfRuns;
        public readonly int TicksPerRun;
        public readonly int Size;
        public readonly RunSample[] Runs;

        static readonly NumberFormatInfo numberFormat = new NumberFormatInfo
        {
            NumberDecimalSeparator = ".",
        };

        public void ExportCsv()
        {
            var header = $"run,tick,insert,gravity,movement,behavior,reset,paint,total";

            var content = new StringBuilder
            (
                capacity: header.Length * NumberOfRuns * TicksPerRun
            );
            content.AppendLine(header);

            for (int run = 0; run < NumberOfRuns; run++)
            {
                for (int tick = 0; tick < TicksPerRun; tick++)
                {
                    var sample = Runs[run].TickSamples[tick];
                    var text = $"{run},{tick},{sample.Insert.TotalMilliseconds.ToString(numberFormat)},{sample.Gravity.TotalMilliseconds.ToString(numberFormat)},{sample.Movement.TotalMilliseconds.ToString(numberFormat)},{sample.Behavior.TotalMilliseconds.ToString(numberFormat)},{sample.Reset.TotalMilliseconds.ToString(numberFormat)},{sample.Paint.TotalMilliseconds.ToString(numberFormat)},{sample.Total.TotalMilliseconds.ToString(numberFormat)}";
                    content.AppendLine(text);
                }
            }

            var folderName = $"{COMPILER_BACKEND}_{DateTime.ToString(DATETIME_FORMAT)}";
            var folderPath = Path.Combine(Application.dataPath, folderName);
            var fileName = $"{COMPILER_BACKEND}_{Type}_{DeviceModel}_{DeviceProcessor}_{ProcessorArch}_{NumberOfRuns}_{Size}_{DateTime.ToString(DATETIME_FORMAT)}.csv";
            var filePath = Path.Combine(folderPath, fileName);
            Debug.Log(filePath);

            Directory.CreateDirectory(folderPath);
            File.WriteAllText(filePath, content.ToString());
        }

        public BenchmarkResult(SimulationType simulationType, DateTime dateTime, int numberOfRuns, RecordedInput recordedInput, RunSample[] runs)
        {
            Type = simulationType;
            DateTime = dateTime;
            NumberOfRuns = numberOfRuns;
            TicksPerRun = recordedInput.Ticks;
            Size = recordedInput.Width;
            Runs = runs;

            foreach (var runSample in Runs.Select(run => run.TickSamples))
            {
                if (runSample.Length != TicksPerRun)
                {
                    throw new Exception($"RunSamples has incorrect size, expected {TicksPerRun}, got {runSample.Length}.");
                }

                foreach (var tickSample in runSample)
                {
                    if (tickSample.Total.Ticks == 0)
                    {
                        throw new Exception("TickSample with zero total time.");
                    }
                }
            }
        }
    }

    public class RunSample
    {
        public readonly TickSample[] TickSamples;

        public TimeSpan TotalRuntime => new TimeSpan(TickSamples.Sum(simulationTick => simulationTick.Total.Ticks));

        public RunSample(TickSample[] tickSamples)
        {
            TickSamples = tickSamples;
        }
    }

    public readonly struct TickSample
    {
        public readonly TimeSpan Insert;
        public readonly TimeSpan Gravity;
        public readonly TimeSpan Movement;
        public readonly TimeSpan Behavior;
        public readonly TimeSpan Reset;
        public readonly TimeSpan Paint;

        public TimeSpan Total => Insert + Gravity + Movement + Behavior + Reset + Paint;

        public TickSample(TimeSpan insert, TimeSpan gravity, TimeSpan movement, TimeSpan behavior, TimeSpan reset, TimeSpan paint)
        {
            Insert = insert;
            Gravity = gravity;
            Movement = movement;
            Behavior = behavior;
            Reset = reset;
            Paint = paint;
        }
    }
}