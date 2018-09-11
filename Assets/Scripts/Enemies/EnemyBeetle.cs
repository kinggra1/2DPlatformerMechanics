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
    private readonly float MAX_ROAM_DISTANCE = 6000f; // basically roam forever
    private readonly float SELF_KNOCKBACK_VELOCITY = 20f;
    private readonly float PLAYER_KNOCKBACK_VELOCITY = 20f;
    private readonly float PLAYER_KNOCKBACK_ANGLE = 30f; // degrees from horizontal

    private float targetXValue;

    // Use this for initialization
    new void Start() {
        base.Start();
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

        Vector3 facingDirection = AI.DirectionToVector2(direction);

        // Check to see if we're going to hit any non-trigger colliers in front of us
        RaycastHit2D[] hits = Physics2D.LinecastAll(transform.position, transform.position + facingDirection, AI.NonPlayerOrEnemyLayermask);
        foreach(RaycastHit2D hit in hits) {
            // Ignore triggers
            if (!hit.collider.isTrigger) {
                // We've hit something that isn't a Player or an Enemy or a trigger
                return true;
            }
        }
        Debug.DrawLine(transform.position, transform.position + facingDirection);

        /*
        // Assume that we're about to fall off of a cliff in front of ourselves and do a 
        // check to prove that false.
        // This block is basically the difference between green shell and red shell koopas in SMB.
        hits = Physics2D.LinecastAll(transform.position + facingDirection, transform.position + facingDirection + Vector3.down, AI.NonPlayerOrEnemyLayermask);
        bool nearCliff = true;
        foreach (RaycastHit2D hit in hits) {
            // Ignore triggers
            if (!hit.collider.isTrigger) {
                // Okay, we've found solid ground, we're good here
                nearCliff = false;
                break;
            }
        }
        Debug.DrawLine(transform.position + facingDirection, transform.position + facingDirection + Vector3.down, Color.red);
        if (nearCliff) {
            return true;
        }
        */


        // Otherwise check to see if we've exceeded our max roaming distance
        float distanceToTarget = Mathf.Abs(transform.position.x - targetXValue);
        return (direction == AI.Direction.LEFT && transform.position.x < targetXValue
            || direction == AI.Direction.RIGHT && transform.position.x > targetXValue)
            || distanceToTarget > MAX_ROAM_DISTANCE;
    }

    private void ChangeDirection() {
        base.SetDirection(AI.OppositeDirection(direction));
        targetXValue = transform.position.x + MAX_ROAM_DISTANCE * AI.DirectionScalarX(direction);
    }

    void IStrikeable.Strike(Vector3 weaponLocation, ItemWeapon weapon) {
        base.TakeDamage(1f);
        SetMotionState(MoveState.HIT);

        // Effects for getting hit here
        // Project vector from weapon to us onto the X axis and normalize to decide direction (left or right)
        float knockbackX = Vector3.Project(this.transform.position - weaponLocation, Vector2.right).normalized.x;
        rb.velocity = new Vector2(knockbackX * SELF_KNOCKBACK_VELOCITY, rb.velocity.y);
    }

    // Handle how we hit the player
    private void OnTriggerStay2D(Collider2D collider) {
        PlayerController player = collider.gameObject.GetComponentInParent<PlayerController>();
        if (player && !player.IsInvulnerable()) {
            float knockbackX = Vector3.Project(player.transform.position - this.transform.position, Vector2.right).normalized.x;
            knockbackX = knockbackX * Mathf.Cos(PLAYER_KNOCKBACK_ANGLE * Mathf.Deg2Rad);
            // The amount of "up" our knockback has. We're small and low so we knock the player up a little in a predictable way.
            float knockbackY = Mathf.Sin(PLAYER_KNOCKBACK_ANGLE * Mathf.Deg2Rad);

            player.GetHit(new Vector2(knockbackX, knockbackY)*7f);
        }
    }
}