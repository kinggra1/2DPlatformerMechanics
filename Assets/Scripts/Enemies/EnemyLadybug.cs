using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLadybug : Enemy, IStrikeable {

    private enum MoveState { IDLE, SEEKPLANT, EATPLANT, SEEKPLAYER }
    private MoveState moveState = MoveState.SEEKPLAYER;

    private Rigidbody2D rb;
    private PlayerController player;

    private readonly float STUN_TIME = 1f;
    private float hitStunTimer = 0f;

    private readonly float MAX_FLIGHT_SPEED = 5f; // m/s
    private readonly float SELF_KNOCKBACK_VELOCITY = 20f;
    private readonly float PLAYER_KNOCKBACK_VELOCITY = 20f;
    //private readonly float PLAYER_KNOCKBACK_ANGLE = 30f; // degrees from horizontal

    private float targetXValue;

    // Use this for initialization
    void Start() {
        base.Start();
        rb = this.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
    }

    private void SetMotionState(MoveState newState) {
        switch (newState) {
            case MoveState.IDLE:
                break;

            case MoveState.SEEKPLANT:
                break;

            case MoveState.EATPLANT:
                break;

            case MoveState.SEEKPLAYER:
                break;
        }

        moveState = newState;
    }

    // Update is called once per frame
    void Update() {
        switch (moveState) {
            case MoveState.IDLE:
                break;

            case MoveState.SEEKPLANT:
                break;

            case MoveState.EATPLANT:
                break;

            case MoveState.SEEKPLAYER:
                Vector2 direction = (player.transform.position - transform.position).normalized;
                if (rb.velocity.magnitude < MAX_FLIGHT_SPEED) {
                    rb.AddForce(direction * 10f);
                } else {
                    rb.velocity *= 0.9f;
                }
                break;

                /*
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
                */
        }
    }

    void IStrikeable.Strike(Vector3 weaponLocation, ItemWeapon weapon) {
        // Debug.Log("Smap");
        base.TakeDamage(1f);
        //SetMotionState(MoveState.HIT);

        // Effects for getting hit here
        // Project vector from weapon to us onto the X axis and normalize to decide direction (left or right)
        Vector2 knockback = (this.transform.position - weaponLocation).normalized;
        rb.AddForce(knockback * 1000f);
        // rb.velocity = knockback;
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
