using UnityEngine;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public static class DirectionExtensions
{
    public static Vector2Int ToVector2Int(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Vector2Int.up;

            case Direction.Down:
                return Vector2Int.down;

            case Direction.Left:
                return Vector2Int.left;

            case Direction.Right:
                return Vector2Int.right;

            default:
                return Vector2Int.right;
        }
    }

    public static bool IsOpposite(this Direction current, Direction next)
    {
        return current == Direction.Up && next == Direction.Down ||
               current == Direction.Down && next == Direction.Up ||
               current == Direction.Left && next == Direction.Right ||
               current == Direction.Right && next == Direction.Left;
    }
}