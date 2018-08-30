using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBeetle : Enemy, IStrikeable {

    private enum MoveState { IDLE, HIT, ROAMING } // As many states as you need
    private MoveState moveState = MoveState.IDLE;

    Rigidbody2D rb;

    private readonly float STUN_TIME = 1f;
    private float hitStunTimer = 0f;

    private readonly float ROAM_SPEED = 1f; // m/s
    private readonly float MAX_ROAM_DISTANCE = 3f;
    
    private float startingX;
    private float roamXTargetOffset;

    // Use this for initialization
    void Start() {
        rb = this.GetComponent<Rigidbody2D>();

        roamXTargetOffset = MAX_ROAM_DISTANCE;
        startingX = transform.position.x;
    }

    private void SetMotionState(MoveState newState) {
        switch (newState) {
            case MoveState.IDLE:
                break;

            // This refers to being hit
            case MoveState.HIT:
                hitStunTimer = 0f;
                break;

            case MoveState.ROAMING:
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
                rb.velocity *= 0.93f;
                hitStunTimer += Time.deltaTime;
                if (hitStunTimer > STUN_TIME) {
                    SetMotionState(MoveState.ROAMING);
                }
                break;

            case MoveState.ROAMING:
                float newX = Mathf.MoveTowards(transform.position.x, startingX + roamXTargetOffset, ROAM_SPEED * Time.deltaTime);
                rb.MovePosition(new Vector2(newX, transform.position.y));

                // Change directions if we've reached our target
                Debug.Log(newX + " " + startingX + roamXTargetOffset);
                if (Mathf.Approximately(newX, startingX + roamXTargetOffset)) {
                    roamXTargetOffset = -roamXTargetOffset;
                }
                break;
        }
    }

    void IStrikeable.Strike(Vector3 weaponLocation, ItemWeapon weapon) {
        // Debug.Log("Smap");
        SetMotionState(MoveState.HIT);

        // Effects for getting hit here
        // E.g. simple Physics2D knockback based on the position of the weapon
        Vector2 knockbackDirection = (this.transform.position - weaponLocation).normalized;
        rb.AddForce(knockbackDirection * 1000f);
    }
}