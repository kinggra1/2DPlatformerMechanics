using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySample : Enemy, IStrikeable {

    private enum MoveState { IDLE, HIT, OTHERSTATE1, OTHERSTATE2 } // As many states as you need
    private MoveState moveState = MoveState.IDLE;

    Rigidbody2D rb;

    private readonly float STUN_TIME = 1f;

    // Use this for initialization
    new void Start() {
        base.Start();
        rb = this.GetComponent<Rigidbody2D>();
    }

    private void SetMotionState(MoveState newState) {

        if (newState == moveState) {
            return;
        }

        stateTimer = 0f;

        switch (newState) {
            case MoveState.IDLE:
                break;

            // This refers to being hit
            case MoveState.HIT:
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

        stateTimer += Time.deltaTime;

        switch (moveState) {
            case MoveState.IDLE:
                break;

            case MoveState.HIT:

                stateTimer += Time.deltaTime;
                if (stateTimer > STUN_TIME) {
                    SetMotionState(MoveState.IDLE);
                }
                break;

            case MoveState.OTHERSTATE1:
                break;

            case MoveState.OTHERSTATE2:
                break;
        }
    }

    void IStrikeable.Strike(Vector2 weaponLocation, ItemWeapon weapon) {
        // Debug.Log("Smap");
        base.TakeDamage(0f);
        SetMotionState(MoveState.HIT);

        // Effects for getting hit here
        // E.g. simple Physics2D knockback based on the position of the weapon
        // Vector2 knockbackDirection = (this.transform.position - weaponLocation).normalized;
        // rb.AddForce(knockbackDirection * 1000f);
    }

    // Handle how we hit the player
    private void OnTriggerStay2D(Collider2D collider) {
        PlayerController player = collider.gameObject.GetComponentInParent<PlayerController>();
        if (player && !player.IsInvulnerable()) {
            Vector2 knockback = (player.transform.position - this.transform.position).normalized;

            player.GetHit(knockback * 20f);
        }
    }
}
