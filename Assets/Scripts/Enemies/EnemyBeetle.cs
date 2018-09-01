using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBeetle : Enemy, IStrikeable {

    private enum MoveState { IDLE, HIT, ROAMING }
    private MoveState moveState = MoveState.ROAMING;

    Rigidbody2D rb;

    private readonly float STUN_TIME = 1f;
    private float hitStunTimer = 0f;

    private readonly float ROAM_SPEED = 2f; // m/s
    private readonly float MAX_ROAM_DISTANCE = 6f;
    private readonly float SELF_KNOCKBACK_VELOCITY = 20f;


    private float targetXValue;

    // Use this for initialization
    void Start() {
        rb = this.GetComponent<Rigidbody2D>();

        targetXValue = transform.position.x + MAX_ROAM_DISTANCE * AI.DirectionScalarX(direction);
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
                // float newX = Mathf.MoveTowards(transform.position.x, startingX + roamXTargetOffset, ROAM_SPEED * Time.deltaTime);
                // rb.MovePosition(new Vector2(newX, transform.position.y));

                // Change directions if we've traveled target distance
                if (NeedNewTarget()) {
                    ChangeDirection();
                }

                float xSpeed = ROAM_SPEED * (direction == AI.Direction.LEFT ? -1 : 1);
                rb.velocity = new Vector2(xSpeed, rb.velocity.y);
                break;
        }
    }

    private bool NeedNewTarget() {
        float distanceToTarget = Mathf.Abs(transform.position.x - targetXValue);
        return (direction == AI.Direction.LEFT && transform.position.x < targetXValue
            || direction == AI.Direction.RIGHT && transform.position.x > targetXValue)
            || distanceToTarget > MAX_ROAM_DISTANCE;
    }

    private void ChangeDirection() {
        direction = AI.OppositeDirection(direction);
        targetXValue = transform.position.x + MAX_ROAM_DISTANCE * AI.DirectionScalarX(direction);
    }

    void IStrikeable.Strike(Vector3 weaponLocation, ItemWeapon weapon) {
        // Debug.Log("Smap");
        SetMotionState(MoveState.HIT);

        // Effects for getting hit here
        // Project vector from weapon to us onto the X axis and normalize to decide direction (left or right)
        Vector2 knockbackDirection = Vector3.Project(this.transform.position - weaponLocation, Vector2.right).normalized;
        //rb.AddForce(knockbackDirection * 1000f);
        rb.velocity = new Vector2(knockbackDirection.x * SELF_KNOCKBACK_VELOCITY, rb.velocity.y);
        //Debug.Log(Time.deltaTime + " " + knockbackDirection * 1000f);
    }
}