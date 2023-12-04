using UnityEngine;
using Kind = Conventional.Pixel.Kind;

namespace Conventional
{
    public partial class World
    {
        public void InsertNewPixels()
        {
            for (int i = 0; i < insertions.Count; i++)
            {
                pixels[insertions[i].X, insertions[i].Y] = new Pixel(insertions[i].Type);

            }
            insertions.Clear();
        }

        public void ApplyGravity()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    switch (pixels[x, y].Type)
                    {
                        case Kind.Smoke:
                            pixels[x, y] = pixels[x, y].SetVelocity(gasVelocity);
                            break;
                        default:
                            pixels[x, y] = pixels[x, y].Accelerate(gravity);
                            break;
                    }
                }
            }
        }

        public void CalculateMovement()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pixels[x, y].Type == Kind.Empty || pixels[x, y].Type == Kind.Rock || pixels[x, y].IsMoved()) { continue; }

                    var position = new Vector2Int(x, y);
                    var nextPosition = position;
                    var targetPosition = PositionSum(position, pixels[x, y].Velocity);
                    var step = new Vector2Int
                    (
                        x: Mathf.Clamp(pixels[x, y].Velocity.X, -1, 1),
                        y: Mathf.Clamp(pixels[x, y].Velocity.Y, -1, 1)
                    );
                    var totalSteps = Mathf.Max(Mathf.Abs(pixels[x, y].Velocity.X), Mathf.Abs(pixels[x, y].Velocity.Y));

                    for (int s = 0; s < totalSteps; s++)
                    {
                        Vector2Int candidate1 = new Vector2Int(nextPosition.x + step.x, nextPosition.y);
                        Vector2Int candidate2 = new Vector2Int(nextPosition.x + step.x, nextPosition.y + step.y);
                        Vector2Int candidate3 = new Vector2Int(nextPosition.x, nextPosition.y + step.y);

                        Vector2Int candidate = GetClosest(candidate1, candidate2, candidate3, targetPosition);

                        if (pixels[candidate.x, candidate.y].Type == Kind.Empty)
                        {
                            nextPosition = candidate;
                            continue;
                        }
                        else { break; }
                    }

                    Velocity actualVelocity = nextPosition - position;
                    pixels[x, y] = pixels[x, y].SetVelocity(actualVelocity).SetMoved();
                    Swap(position, nextPosition);
                }

                Vector2Int GetClosest(Vector2Int p1, Vector2Int p2, Vector2Int p3, Vector2Int target)
                {
                    var d1 = (target.x - p1.x) * (target.x - p1.x) + (target.y - p1.y) * (target.y - p1.y);
                    var d2 = (target.x - p2.x) * (target.x - p2.x) + (target.y - p2.y) * (target.y - p2.y);
                    var d3 = (target.x - p3.x) * (target.x - p3.x) + (target.y - p3.y) * (target.y - p3.y);

                    var result = p1;
                    var squareDistance = d1;

                    if (d2 < squareDistance)
                    {
                        result = p2;
                        squareDistance = d2;
                    }
                    if (d3 < squareDistance)
                    {
                        result = p3;
                    }

                    return result;
                }
            }
        }

        public void ApplyBehavior()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = width - 1; x >= 0; x--)
                {
                    HandlePieceAt(x, y);
                }
            }
        }

        void HandlePieceAt(int x, int y)
        {
            if (pixels[x, y].IsProcessed()) { return; }

            var position = new Vector2Int(x, y);

            var nextPosition = pixels[x, y].Type switch
            {
                Kind.Sand => HandleSand(position),
                Kind.Water => HandleWater(position),
                Kind.Smoke => HandleSmoke(position),
                Kind.Lava => HandleLava(position),
                Kind.Rock => HandleBedrock(position),
                _ => position
            };

            pixels[x, y] = pixels[x, y].SetProcessed();
            Swap(position, nextPosition);
        }

        static readonly Vector2Int[] SAND_NEIGHBORS = new Vector2Int[2] { new Vector2Int(1, -1), new Vector2Int(-1, -1) };
        Vector2Int HandleSand(Vector2Int position)
        {
            var nextPosition = position;
            var belowPosition = PositionSum(position, DOWN);
            var belowType = pixels[belowPosition.x, belowPosition.y].Type;

            if (belowType != Kind.Empty)
            {
                for (int ni = 0; ni < SAND_NEIGHBORS.Length; ni++)
                {
                    var neighborPosition = PositionSum(position, SAND_NEIGHBORS[ni]);
                    var neighborType = pixels[neighborPosition.x, neighborPosition.y].Type;

                    if (neighborType == Kind.Empty)
                    {
                        nextPosition = neighborPosition;
                        break;
                    }
                }
            }

            return nextPosition;
        }

        static readonly Vector2Int[] WATER_NEIGHBORS = new Vector2Int[4] { new Vector2Int(-1, -1), new Vector2Int(1, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0) };
        Vector2Int HandleWater(Vector2Int position)
        {
            var nextPosition = position;
            var belowPosition = PositionSum(position, DOWN);
            var abovePosition = PositionSum(position, UP);
            var aboveType = pixels[abovePosition.x, abovePosition.y].Type;
            var belowType = pixels[belowPosition.x, belowPosition.y].Type;

            if (aboveType == Kind.Sand)
            {
                nextPosition = abovePosition;
            }
            else if (belowType != Kind.Empty)
            {
                for (int ni = 0; ni < WATER_NEIGHBORS.Length; ni++)
                {
                    var neighborPosition = PositionSum(position, WATER_NEIGHBORS[ni]);
                    var neighborType = pixels[neighborPosition.x, neighborPosition.y].Type;

                    if (neighborType == Kind.Empty || neighborType == Kind.Smoke)
                    {
                        nextPosition = neighborPosition;
                        break;
                    }
                }
            }

            return nextPosition;
        }

        static readonly Vector2Int[] SMOKE_NEIGHBORS = new Vector2Int[8] { new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(0, -1) };
        Vector2Int HandleSmoke(Vector2Int position)
        {
            if ((position.x * position.y + tick) % 48 == 0)
            {
                pixels[position.x, position.y] = new Pixel(Kind.Empty);
                return position;
            }

            var nextPosition = position;

            for (int ni = 0; ni < SMOKE_NEIGHBORS.Length; ni++)
            {
                var neighborPosition = PositionSum(position, SMOKE_NEIGHBORS[ni]);
                var neighborType = pixels[neighborPosition.x, neighborPosition.y].Type;

                if (neighborType == Kind.Empty)
                {
                    nextPosition = neighborPosition;
                    break;
                }
            }

            return nextPosition;
        }

        static readonly Vector2Int[] LAVA_NEIGHBORS = new Vector2Int[4] { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        Vector2Int HandleLava(Vector2Int position)
        {
            var interactingNeighbors = 0;

            for (int ni = 0; ni < 4; ni++)
            {
                var neighborPosition = PositionSum(position, LAVA_NEIGHBORS[ni]);
                var neighborType = pixels[neighborPosition.x, neighborPosition.y].Type;

                switch (neighborType)
                {
                    case Kind.Water:
                        interactingNeighbors++;
                        pixels[neighborPosition.x, neighborPosition.y] = new Pixel(Kind.Smoke);
                        break;
                    case Kind.Sand:
                        interactingNeighbors++;
                        pixels[neighborPosition.x, neighborPosition.y] = new Pixel(Kind.Rock);
                        break;
                    case Kind.Empty:
                        if (interactingNeighbors > 0)
                        {
                            pixels[neighborPosition.x, neighborPosition.y] = new Pixel(Kind.Smoke);
                        }
                        break;
                }
            }

            if ((position.x * position.y + tick) % 8 < interactingNeighbors)
            {
                pixels[position.x, position.y] = new Pixel(Kind.Rock);
                return position;
            }

            var nextPosition = position;
            var belowPosition = PositionSum(position, DOWN);
            var belowType = pixels[belowPosition.x, belowPosition.y].Type;

            if (belowType != Kind.Empty)
            {
                for (int ni = 0; ni < WATER_NEIGHBORS.Length; ni++)
                {
                    var neighborPosition = PositionSum(position, WATER_NEIGHBORS[ni]);
                    var neighborType = pixels[neighborPosition.x, neighborPosition.y].Type;

                    if (neighborType == Kind.Empty || neighborType == Kind.Smoke)
                    {
                        nextPosition = neighborPosition;
                        break;
                    }
                }
            }

            return nextPosition;
        }

        Vector2Int HandleBedrock(Vector2Int position)
        {
            var nextPosition = position;
            var belowPosition = PositionSum(position, DOWN);
            var belowType = pixels[belowPosition.x, belowPosition.y].Type;

            if (belowType == Kind.Lava)
            {
                var decision = (position.x * position.y + tick) % 8 == 0;
                nextPosition = decision ? belowPosition : position;
            }

            return nextPosition;
        }

        public void ResetStatus()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    pixels[x, y] = pixels[x, y].ResetStatus();
                }
            }
        }

        public void IncreaseTick()
        {
            tick++;
        }

        public static readonly Vector2Int
            LEFT = new Vector2Int(-1, 0),
            RIGHT = new Vector2Int(1, 0),
            UP = new Vector2Int(0, 1),
            DOWN = new Vector2Int(0, -1),
            LEFT_UP = new Vector2Int(-1, 1),
            LEFT_DOWN = new Vector2Int(-1, -1),
            RIGHT_UP = new Vector2Int(1, 1),
            RIGHT_DOWN = new Vector2Int(1, -1);
    }
}