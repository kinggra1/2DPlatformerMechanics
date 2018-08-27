using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : MonoBehaviour {

    protected float health;
    protected float maxHealth;

    [Tooltip("Scalar from 0 to 1 indicating what percentage of damage is blocked.")]
    protected float defense = 1f;
}
