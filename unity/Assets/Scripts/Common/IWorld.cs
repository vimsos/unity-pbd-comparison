using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Common
{
    public interface IWorld : IDisposable
    {
        int Width { get; }
        int Height { get; }
        uint Tick { get; }

        void Insert(IEnumerable<RecordedInput.Insertion> newInsertions);
        void InsertNewPixels();
        void ApplyGravity();
        void CalculateMovement();
        void ApplyBehavior();
        void ResetStatus();
        void DrawToTexture(NativeArray<Color32> rawTexture);
        void IncreaseTick();
    }
}