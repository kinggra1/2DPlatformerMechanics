using UnityEngine;
using System;
using System.Collections;

public static class AI {

    private static PlayerController player;

    // maintain singleton reference to player for calculating distance to player
    private static PlayerController GetPlayer() {
        if (player == null) {
            player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
        }

        return player;
    }

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

    public static Direction VectorToHorizontalDirection(Vector2 vector) {
        if (Mathf.Approximately(vector.x, 0f)) {
            return Direction.NONE;
        }

        return vector.x > 0 ? Direction.RIGHT : Direction.LEFT;
    }

    public static Direction VectorToVerticalDirection(Vector2 vector) {
        if (Mathf.Approximately(vector.y, 0f)) {
            return Direction.NONE;
        }

        return vector.y > 0 ? Direction.UP : Direction.DOWN;
    }

    public static Direction VectorToDirection(Vector2 vector) {
        // This is mostly a horizontal vector
        if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y)) {
            return VectorToHorizontalDirection(vector);
        } else {
            return VectorToVerticalDirection(vector);
        }
    }

    public static Direction FloatToHorizontalDirection(float value) {
        if (Mathf.Approximately(value, 0f)) {
            return Direction.NONE;
        }

        return value > 0 ? Direction.RIGHT : Direction.LEFT;
    }

    public static Direction FloatToVerticalDirection(float value) {
        if (Mathf.Approximately(value, 0f)) {
            return Direction.NONE;
        }

        return value > 0 ? Direction.UP : Direction.DOWN;
    }








    /*
     * *****************************************************************************
     * Raycasting Tools
     * *****************************************************************************
     */

    public static int PlayerOrEnemyLayermask = (
        (1 << LayerMask.NameToLayer("Player"))
        | (1 << LayerMask.NameToLayer("Enemy"))
    );
    public static int NonPlayerOrEnemyLayermask = ~PlayerOrEnemyLayermask;

    public static int PlayerLayermask = (
        (1 << LayerMask.NameToLayer("Player"))
    );

    public static int NonPlayerLayermask = ~PlayerLayermask;

    public static int EnemyLayermask = (
        (1 << LayerMask.NameToLayer("Enemy"))
    );

    public static int PlantLayermask = (
        (1 << LayerMask.NameToLayer("Plant"))
    );

    public static int WaterLayermask = (
        (1 << LayerMask.NameToLayer("Water"))
    );

    public static int StandableRaycastLayers = (
    (1 << LayerMask.NameToLayer("Ground"))
            | (1 << LayerMask.NameToLayer("Platform"))
            // | (1 << LayerMask.NameToLayer("NameOfALayerWeCanStandOn")
            // ...
            );



    public static float DistanceToPlayer(GameObject go) {
        return Vector2.Distance(go.transform.position, GetPlayer().transform.position);
    }

    public static Vector2 VectorToPlayer(GameObject go) {
        return ((Vector2)(GetPlayer().transform.position - go.transform.position)).normalized;
    }


    public static Collider2D FindPlantInRange(Vector2 pos, float distance) {
        return Physics2D.OverlapCircle(pos, distance, PlantLayermask);
    }

    // Results will not be sorted
    public static void NearestPlants(Vector2 pos, float distance, Collider2D[] results) {
        Physics2D.OverlapCircleNonAlloc(pos, distance, results, PlantLayermask);
    }
}
