using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour {

    protected float health;
    protected float maxHealth;
    protected AI.Direction direction = AI.Direction.RIGHT;

    [Tooltip("Scalar from 0 to 1 indicating what percentage of damage is blocked.")]
    public float defense = 1f;
}
