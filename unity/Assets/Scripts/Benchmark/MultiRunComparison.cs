using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using Common;

using static Benchmark.Utils;

namespace Benchmark
{
    public class MultiRunComparison : MonoBehaviour
    {
        [Header("Simulation Objects")]
        [SerializeField] MeshRenderer referenceSimulationRenderer = null;
        [SerializeField] MeshRenderer benchmarkSimulationRenderer = null;

        [Header("References")]
        [SerializeField] Text log = null;

        [Header("Settings")]
        [SerializeField] int numberOfRuns = 10;
        [SerializeField] SimulationType[] simulationTypes = { SimulationType.Conventional, SimulationType.Jobs, SimulationType.ParallelJobs };
        [SerializeField] int[] simulationSizes = { 64, 128, 256, 512 };
        [SerializeField] int simulationLength = 900;

        NativeArray<Color32> rawReferenceTexture;
        string deviceModel;
        string deviceProcessor;
        DateTime now;

        IEnumerator Start()
        {
            log.Prepend($"DeviceModel: {DeviceModel}");
            log.Prepend($"ProcessorName: {DeviceProcessor}");
            log.Prepend($"HighResolutionTimer: {Stopwatch.IsHighResolution}");

            now = DateTime.Now;

            foreach (var size in simulationSizes)
            {
                log.Prepend($"Generating {size}x{size} reference simulation result");
                var recordedInput = GenerateInputPattern(size, simulationLength);
                yield return StartCoroutine(SetupReference(recordedInput));

                foreach (var type in simulationTypes)
                {
                    log.Prepend($"Warming up {type} {recordedInput.Width}x{recordedInput.Height}");

                    using var warmUp = NewWorld(recordedInput, type);
                    for (int i = 0; i < recordedInput.Ticks / 4; i++)
                    {
                        WarmUpSimulationRun(warmUp);
                    }

                    log.Prepend($"Running SimulationType.{type} {recordedInput.Width}x{recordedInput.Height} Length: {recordedInput.Ticks} NumberOfRuns: {numberOfRuns}");

                    GC.Collect();

                    yield return StartCoroutine(Benchmark(recordedInput, type));

                    GC.Collect();
                }
            }

            log.Prepend("Finished running all scenarios");
        }

        IEnumerator SetupReference(RecordedInput recordedInput)
        {
            using var world = NewWorld(recordedInput, SimulationType.Jobs);
            var (texture, rawTexture) = SetupTextures(world, referenceSimulationRenderer);
            rawReferenceTexture = rawTexture;
            var stopwatch = new Stopwatch();
            var totalTicks = recordedInput.Ticks;

            for (uint tick = 0; tick < totalTicks; tick++)
            {
                world.Insert(recordedInput.InsertionsAtTick(tick));

                _ = TimedSimulationRun(world, texture, rawTexture, stopwatch);

                yield return null;
            }

            yield break;
        }

        IEnumerator Benchmark(RecordedInput recordedInput, SimulationType type)
        {
            IWorld world = NewWorld(recordedInput, type);
            var (texture, rawTexture) = SetupTextures(world, benchmarkSimulationRenderer);

            var totalTicks = recordedInput.Ticks;
            var runSamples = new RunSample[numberOfRuns];
            var tickSamples = new TickSample[numberOfRuns][];
            for (int run = 0; run < numberOfRuns; run++)
            {
                tickSamples[run] = new TickSample[totalTicks];
            }

            Stopwatch stopwatch = new Stopwatch();

            for (int run = 0; run < numberOfRuns; run++)
            {
                world = world ?? NewWorld(recordedInput, type);

                for (int tick = 0; tick < totalTicks; tick++)
                {
                    world.Insert(recordedInput.InsertionsAtTick(world.Tick));

                    tickSamples[run][tick] = TimedSimulationRun(world, texture, rawTexture, stopwatch);
                }

                var matchesReference = CompareRawTexture(rawReferenceTexture, rawTexture);
                runSamples[run] = new RunSample(tickSamples[run]);

                log.Prepend($"{type} run #{run} TotalRuntime: {runSamples[run].TotalRuntime.ToString(DURATION_FORMAT)} MatchesReference: {matchesReference}");

                world.Dispose();
                world = null;
                yield return null;
            }

            BenchmarkResult result = new BenchmarkResult
            (
                simulationType: type,
                dateTime: now,
                recordedInput: recordedInput,
                numberOfRuns: numberOfRuns,
                runs: runSamples
            );

#if !UNITY_EDITOR
            result.ExportCsv();
#endif
        }
    }
}
