using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Common;

using static Benchmark.Utils;

namespace Benchmark
{
    public class SideBySide : MonoBehaviour
    {
        [Header("Simulation Objects")]
        [SerializeField] MeshRenderer aSimulationRenderer = null;
        [SerializeField] MeshRenderer bSimulationRenderer = null;

        [Header("Configuration")]
        [SerializeField] Dropdown inputDropdown = null;
        [SerializeField] Dropdown sizeDropdown = null;
        [SerializeField] Dropdown lengthDropdown = null;
        [SerializeField] Dropdown aDropdown = null;
        [SerializeField] Dropdown bDropdown = null;
        [SerializeField] Button startButton = null;

        [Header("Simulation A")]
        [SerializeField] Text aInsertTimeDisplay = null;
        [SerializeField] Text aGravityTimeDisplay = null;
        [SerializeField] Text aMovementTimeDisplay = null;
        [SerializeField] Text aBehaviorTimeDisplay = null;
        [SerializeField] Text aResetTimeDisplay = null;
        [SerializeField] Text aPaintTimeDisplay = null;
        [SerializeField] Text aTotalTimeDisplay = null;
        [SerializeField] Text realTimeTimeDisplay = null;

        [Header("Simulation B")]
        [SerializeField] Text bInsertTimeDisplay = null;
        [SerializeField] Text bGravityTimeDisplay = null;
        [SerializeField] Text bMovementTimeDisplay = null;
        [SerializeField] Text bBehaviorTimeDisplay = null;
        [SerializeField] Text bResetTimeDisplay = null;
        [SerializeField] Text bPaintTimeDisplay = null;
        [SerializeField] Text bTotalTimeDisplay = null;
        [SerializeField] Text speedUpDisplay = null;

        [Header("Settings")]
        [SerializeField] string inputFileName = "RecordedInput";
        [SerializeField] RecordedInput recordedInput = null;

        SimulationType ATypeSelection => aDropdown.value switch
        {
            0 => SimulationType.Conventional,
            1 => SimulationType.Jobs,
            2 => SimulationType.ParallelJobs,
            _ => throw new Exception("Unrecognized SimulationType")
        };

        SimulationType BTypeSelection => bDropdown.value switch
        {
            0 => SimulationType.Conventional,
            1 => SimulationType.Jobs,
            2 => SimulationType.ParallelJobs,
            _ => throw new Exception("Unrecognized SimulationType")
        };

        int SizeSelection => sizeDropdown.value switch
        {
            0 => 64,
            1 => 128,
            2 => 256,
            3 => 512,
            _ => throw new Exception("Unrecognized Simulation Size")
        };

        int LenghtSelection => lengthDropdown.value switch
        {
            0 => 300,
            1 => 600,
            2 => 900,
            3 => 1200,
            4 => 1500,
            _ => throw new Exception("Unrecognized Simulation Length")
        };

        RecordedInput InputSelection => inputDropdown.value switch
        {
            0 => recordedInput,
            1 => GenerateInputPattern(SizeSelection, LenghtSelection),
            _ => throw new Exception("Unrecognized Input")
        };

        void Start()
        {
            string inputJson = Resources.Load<TextAsset>(inputFileName).text;
            recordedInput = JsonUtility.FromJson<RecordedInput>(inputJson);

            inputDropdown.onValueChanged.AddListener((selected) =>
            {
                sizeDropdown.interactable = selected == 1;
                lengthDropdown.interactable = selected == 1;
            });

            startButton.onClick.AddListener(() =>
            {
                startButton.interactable = false;
                aDropdown.interactable = false;
                bDropdown.interactable = false;
                sizeDropdown.interactable = false;
                lengthDropdown.interactable = false;
                inputDropdown.interactable = false;

                StartCoroutine(Compare(InputSelection, ATypeSelection, BTypeSelection));
            });
        }

        IEnumerator Compare(RecordedInput input, SimulationType aType, SimulationType bType)
        {
            var totalTicks = input.Ticks;

            using var aSimulation = NewWorld(input, aType);
            using var bSimulation = NewWorld(input, bType);

            var (aTexture, aRawTexture) = SetupTextures(aSimulation, aSimulationRenderer);
            var (bTexture, bRawTexture) = SetupTextures(aSimulation, bSimulationRenderer);

            Stopwatch stopwatch = new Stopwatch();

            TimeSpan aInsertTime, aGravityTime, aMovementTime, aBehaviorTime, aResetTime, aPaintTime, aTotalTime, bInsertTime, bGravityTime, bMovementTime, bBehaviorTime, bResetTime, bPaintTime, bTotalTime;
            aInsertTime = aGravityTime = aMovementTime = aBehaviorTime = aResetTime = aPaintTime = aTotalTime = bInsertTime = bGravityTime = bMovementTime = bBehaviorTime = bResetTime = bPaintTime = bTotalTime = new TimeSpan();

            aInsertTime = aGravityTime = aMovementTime = aBehaviorTime = aResetTime = aPaintTime = aTotalTime = bInsertTime = bGravityTime = bMovementTime = bBehaviorTime = bResetTime = bPaintTime = bTotalTime = new TimeSpan(0);

            for (int t = 0; t < totalTicks; t++)
            {
                aSimulation.Insert(input.InsertionsAtTick(aSimulation.Tick));
                bSimulation.Insert(input.InsertionsAtTick(bSimulation.Tick));

                var aResult = TimedSimulationRun(aSimulation, aTexture, aRawTexture, stopwatch);
                var bResult = TimedSimulationRun(bSimulation, bTexture, bRawTexture, stopwatch);

                aInsertTime += aResult.Insert;
                aGravityTime += aResult.Gravity;
                aMovementTime += aResult.Movement;
                aBehaviorTime += aResult.Behavior;
                aResetTime += aResult.Reset;
                aPaintTime += aResult.Paint;
                aTotalTime += aResult.Total;

                bInsertTime += bResult.Insert;
                bGravityTime += bResult.Gravity;
                bMovementTime += bResult.Movement;
                bBehaviorTime += bResult.Behavior;
                bResetTime += bResult.Reset;
                bPaintTime += bResult.Paint;
                bTotalTime += bResult.Total;

                aInsertTimeDisplay.text = aInsertTime.ToString(DURATION_FORMAT);
                aGravityTimeDisplay.text = aGravityTime.ToString(DURATION_FORMAT);
                aMovementTimeDisplay.text = aMovementTime.ToString(DURATION_FORMAT);
                aBehaviorTimeDisplay.text = aBehaviorTime.ToString(DURATION_FORMAT);
                aResetTimeDisplay.text = aResetTime.ToString(DURATION_FORMAT);
                aPaintTimeDisplay.text = aPaintTime.ToString(DURATION_FORMAT);
                aTotalTimeDisplay.text = aTotalTime.ToString(DURATION_FORMAT);

                bInsertTimeDisplay.text = bInsertTime.ToString(DURATION_FORMAT);
                bGravityTimeDisplay.text = bGravityTime.ToString(DURATION_FORMAT);
                bMovementTimeDisplay.text = bMovementTime.ToString(DURATION_FORMAT);
                bBehaviorTimeDisplay.text = bBehaviorTime.ToString(DURATION_FORMAT);
                bResetTimeDisplay.text = bResetTime.ToString(DURATION_FORMAT);
                bPaintTimeDisplay.text = bPaintTime.ToString(DURATION_FORMAT);
                bTotalTimeDisplay.text = bTotalTime.ToString(DURATION_FORMAT);

                realTimeTimeDisplay.text = new TimeSpan(0, 0, 0, 0, (int)(t * 1000f / 30f)).ToString(DURATION_FORMAT);
                double speedUpFactor = (double)aTotalTime.Ticks / (double)bTotalTime.Ticks;
                speedUpDisplay.text = speedUpFactor.ToString(SPEEDUP_FORMAT);

                yield return new WaitForEndOfFrame();
            }

            startButton.interactable = true;
            aDropdown.interactable = true;
            bDropdown.interactable = true;
            sizeDropdown.interactable = inputDropdown.value == 1;
            lengthDropdown.interactable = inputDropdown.value == 1;
            inputDropdown.interactable = true;

            yield break;
        }
    }
}
