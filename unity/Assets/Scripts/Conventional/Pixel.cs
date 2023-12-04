using UnityEngine;

namespace Conventional
{
    public readonly struct Pixel
    {
        public enum Status : byte
        {
            None = 0b00000000,
            Moved = 0b00000001,
            Processed = 0b00000010
        }

        public enum Kind : byte
        {
            Empty = 0b00000000,
            Rock = 0b00000001,
            Sand = 0b00000010,
            Water = 0b00000011,
            Lava = 0b00000100,
            Smoke = 0b00000101
        }

        public readonly Kind Type;
        public readonly Status State;
        public readonly Velocity Velocity;
        public readonly Color32 Color;

        public bool IsMoved()
        {
            return (State & Status.Moved) != 0;
        }

        public bool IsProcessed()
        {
            return (State & Status.Processed) != 0;
        }

        public Pixel SetVelocity(Velocity velocity)
        {
            return new Pixel
            (
                type: Type,
                state: State,
                velocity: velocity,
                color: Color
            );
        }

        public Pixel Accelerate(Velocity delta)
        {
            return new Pixel
            (
                type: Type,
                state: State,
                velocity: Velocity + delta,
                color: Color
            );
        }

        public Pixel SetMoved()
        {
            return new Pixel
            (
                type: Type,
                state: State | Status.Moved,
                velocity: Velocity,
                color: Color
            );
        }

        public Pixel SetProcessed()
        {
            return new Pixel
            (
                type: Type,
                state: State | Status.Processed,
                velocity: Velocity,
                color: Color
            );
        }

        public Pixel ResetStatus()
        {
            return new Pixel
            (
                type: Type,
                state: Status.None,
                velocity: Velocity,
                color: Color
            );
        }

        public static Color32 ColorFromType(Kind type)
        {
            return type switch
            {
                Kind.Empty => new Color32(0, 0, 0, 255),
                Kind.Rock => new Color32(127, 127, 127, 255),
                Kind.Sand => new Color32(255, 255, 255, 255),
                Kind.Water => new Color32(164, 219, 232, 255),
                Kind.Smoke => new Color32(70, 70, 70, 255),
                Kind.Lava => new Color32(204, 0, 0, 255),
                _ => new Color32(199, 21, 133, 255)
            };
        }

        public Pixel(Kind type)
        {
            Type = type;
            State = Status.None;
            Velocity = new Velocity(0, 0);
            Color = ColorFromType(type);
        }

        Pixel(Kind type, Status state, Velocity velocity, Color32 color)
        {
            Type = type;
            State = state;
            Velocity = velocity;
            Color = color;
        }
    }
}
