using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBigEnemy : Enemy, IStrikeable {

    private enum MoveState { IDLE, HIT, TELEGRAPH_CHARGE, CHARGE, ATTACK_RECOVERY } // As many states as you need
    private MoveState moveState = MoveState.IDLE;

    private PlayerController player;
    Rigidbody2D rb;

    private readonly float TELEGRAPH_CHARGE_TIME = 1f;
    private readonly float CHARGE_TIME = 0.5f;
    private readonly float ATTACK_RECOVERY_TIME = 2f;

    private readonly float PLAYER_DETECTION_DISTANCE = 15f;

    private readonly float CHARGE_SPEED = 15f;

    // Use this for initialization
    new void Start() {
        base.Start();
        rb = this.GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
    }

    private void SetMotionState(MoveState newState) {

        if (newState == moveState) {
            return;
        }

        stateTimer = 0f;

        // In absence of animations, we'll just use sprite coloring to telegraph state/intention of the box
        switch (newState) {
            case MoveState.IDLE:
                SetSpriteColor(Color.white);
                break;

            // This refers to being hit
            case MoveState.HIT:
                break;

            case MoveState.TELEGRAPH_CHARGE:
                // Start facing player
                Vector3 vecToPlayer = player.transform.position - this.transform.position;
                SetDirection(AI.VectorToHorizontalDirection(vecToPlayer));
                SetSpriteColor(Color.yellow);
                break;

            case MoveState.CHARGE:
                SetSpriteColor(Color.red);
                break;

            case MoveState.ATTACK_RECOVERY:
                SetSpriteColor(Color.blue);
                break;
        }

        moveState = newState;
    }

    // Update is called once per frame
    void FixedUpdate() {

        stateTimer += Time.deltaTime;

        switch (moveState) {
            case MoveState.IDLE:
                if (Vector2.Distance(player.transform.position, this.transform.position) < PLAYER_DETECTION_DISTANCE) {
                    SetMotionState(MoveState.TELEGRAPH_CHARGE);
                    break;
                }
                break;

            case MoveState.TELEGRAPH_CHARGE:
                if (stateTimer > TELEGRAPH_CHARGE_TIME) {
                    SetMotionState(MoveState.CHARGE);
                    break;
                }
                break;

            case MoveState.CHARGE:
                if (stateTimer > CHARGE_TIME) {
                    SetMotionState(MoveState.ATTACK_RECOVERY);
                    break;
                }

                float xSpeed = CHARGE_SPEED * (direction == AI.Direction.LEFT ? -1 : 1);
                rb.velocity = new Vector2(xSpeed, rb.velocity.y);
                break;

            case MoveState.ATTACK_RECOVERY:
                if (stateTimer > ATTACK_RECOVERY_TIME) {
                    SetMotionState(MoveState.IDLE);
                    break;
                }
                break;
        }
    }

    void IStrikeable.Strike(Vector2 weaponLocation, ItemWeapon weapon) {
        // Debug.Log("Smap");
        base.TakeDamage(1f);

        // Hitting big boi does not affect his state.
    }

    // Handle how we hit the player
    private void OnTriggerStay2D(Collider2D collider) {
        PlayerController player = collider.gameObject.GetComponentInParent<PlayerController>();
        if (player && !player.IsInvulnerable()) {
            Vector2 knockback = AI.VectorToPlayer(this.gameObject).normalized;

            player.GetHit(knockback * 20f);
        }
    }
}
