using UnityEngine;
using System.Collections;

public static class AI {

    /*
     * *****************************************************************************
     * Direction Tools
     * *****************************************************************************
     */
    public enum Direction { NONE, UP, DOWN, LEFT, RIGHT };

    public static float DirectionScalarX(Direction direction) {
        switch (direction) {
            case Direction.NONE:
            case Direction.UP:
            case Direction.DOWN:
                return 0f;
            case Direction.LEFT:
                return -1f;
            case Direction.RIGHT:
                return 1f;
        }
       return 0f;
    }

    public static float DirectionScalarY(Direction direction) {
        switch (direction) {
            case Direction.NONE:
            case Direction.LEFT:
            case Direction.RIGHT:
                return 0f;
            case Direction.DOWN:
                return -1f;
            case Direction.UP:
                return 1f;
        }
        return 0f;
    }

    public static Vector2 DirectionToVector2(Direction direction) {
        switch (direction) {
            case Direction.NONE:
                return Vector2.zero;
            case Direction.UP:
                return Vector2.up;
            case Direction.DOWN:
                return Vector2.down;
            case Direction.LEFT:
                return Vector2.left;
            case Direction.RIGHT:
                return Vector2.right;
        }
        return Vector2.zero;
    }

    public static Direction OppositeDirection(Direction direction) {
        switch (direction) {
            case Direction.NONE:
                return Direction.NONE;
            case Direction.UP:
                return Direction.DOWN;
            case Direction.DOWN:
                return Direction.UP;
            case Direction.LEFT:
                return Direction.RIGHT;
            case Direction.RIGHT:
                return Direction.LEFT;
        }
        return Direction.NONE;
    }
}
