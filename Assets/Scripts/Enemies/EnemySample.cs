using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySample : Enemy, IStrikeable {

    private enum MoveState { IDLE, HIT, OTHERSTATE1, OTHERSTATE2 } // As many states as you need
    private MoveState moveState = MoveState.IDLE;

    Rigidbody2D rb;

    private readonly float STUN_TIME = 1f;
    private float hitStunTimer = 0f;

    // Use this for initialization
    void Start() {
        rb = this.GetComponent<Rigidbody2D>();
    }

    private void SetMotionState(MoveState newState) {
        switch (newState) {
            case MoveState.IDLE:
                break;

            // This refers to being hit
            case MoveState.HIT:
                hitStunTimer = 0f;
                break;

            case MoveState.OTHERSTATE1:
                break;

            case MoveState.OTHERSTATE2:
                break;
        }

        moveState = newState;
    }

    // Update is called once per frame
    void Update() {
        switch (moveState) {
            case MoveState.IDLE:
                break;

            case MoveState.HIT:

                hitStunTimer += Time.deltaTime;
                if (hitStunTimer > STUN_TIME) {
                    SetMotionState(MoveState.IDLE);
                }
                break;

            case MoveState.OTHERSTATE1:
                break;

            case MoveState.OTHERSTATE2:
                break;
        }
    }

    void IStrikeable.Strike(Vector3 weaponLocation, ItemWeapon weapon) {
        // Debug.Log("Smap");
        SetMotionState(MoveState.HIT);

        // Effects for getting hit here
        // E.g. simple Physics2D knockback based on the position of the weapon
        // Vector2 knockbackDirection = (this.transform.position - weaponLocation).normalized;
        // rb.AddForce(knockbackDirection * 1000f);
    }
}
