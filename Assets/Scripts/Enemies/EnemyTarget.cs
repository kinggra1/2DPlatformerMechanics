using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTarget : Enemy, IStrikeable {

    private enum MoveState { IDLE, HIT, GOINGHOME }
    private MoveState moveState = MoveState.IDLE;

    Rigidbody2D rb;
    Vector2 homePosition;
    private Vector2 velocity;

    private float stunTimer = 0f;
    private float stunTime = 1f;

    // Use this for initialization
    void Start () {
        rb = this.GetComponent<Rigidbody2D>();
        homePosition = this.transform.position;
	}

    private void SetMotionState(MoveState newState) {
        switch (newState) {
            case MoveState.IDLE:
                break;

            case MoveState.HIT:
                stunTimer = 0f;
                break;

            case MoveState.GOINGHOME:
                break;
        }

        moveState = newState;
    }

    // Update is called once per frame
    void Update () {
        switch (moveState) {
            case MoveState.IDLE:
                break;

            case MoveState.HIT:
                rb.velocity *= 0.93f;
                stunTimer += Time.deltaTime;
                if (stunTimer > stunTime && rb.velocity.magnitude < 0.01f) {
                    SetMotionState(MoveState.GOINGHOME);
                }
                break;

            case MoveState.GOINGHOME:
                this.transform.position = Vector2.Lerp(this.transform.position, homePosition, Time.deltaTime*5f);
                if (Vector2.Distance(this.transform.position, homePosition) < 0.01f) {
                    SetMotionState(MoveState.IDLE);
                }
                break;
        }
    }

    void IStrikeable.Strike(Vector2 weaponLocation, ItemWeapon weapon) {
        SetMotionState(MoveState.HIT);
        Vector2 knockbackDirection = ((Vector2)this.transform.position - weaponLocation).normalized;
        rb.AddForce(knockbackDirection*1000f);
    }
}
