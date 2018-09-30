using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLadybug : Enemy, IStrikeable {

    private enum MoveState { ROAMING, SEEKPLANT, EATPLANT, SEEKPLAYER, HIT }
    private MoveState moveState = MoveState.SEEKPLAYER;

    private Rigidbody2D rb;
    private PlayerController player;

    private readonly float STUN_TIME = 0.8f;
    private readonly float EAT_TIME = 5f;

    private readonly float MAX_FLIGHT_SPEED = 5f; // m/s
    private readonly float SELF_KNOCKBACK_VELOCITY = 20f;
    private readonly float PLAYER_KNOCKBACK_VELOCITY = 20f;
    private readonly float PLAYER_DETECTION_DISTANCE = 15f;
    private readonly float PLANT_DETECTION_DISTANCE = 35f;

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

        switch (newState) {
            case MoveState.ROAMING:
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

        stateTimer += Time.deltaTime;

        switch (moveState) {
            case MoveState.ROAMING:
                if (AI.DistanceToPlayer(this.gameObject) < PLAYER_DETECTION_DISTANCE) {
                    SetMotionState(MoveState.SEEKPLAYER);
                    break;
                }

                targetPlantZone = null;
                Collider2D plant = AI.FindPlantInRange(transform.position, PLANT_DETECTION_DISTANCE);
                Collider2D[] plants = new Collider2D[20];
                if (plant) {
                    targetPlantZone = plant.GetComponentInParent<IPlantableZone>();
                    if (targetPlantZone != null && targetPlantZone.IsPlanted()) {
                        targetPlantPosition = plant.transform.position;
                        SetMotionState(MoveState.SEEKPLANT);
                        break;
                    }
                }

                rb.velocity = Vector2.zero;
                break;

            case MoveState.SEEKPLANT:

                if (!targetPlantZone.IsPlanted()) {
                    SetMotionState(MoveState.ROAMING);
                    break;
                }

                if (Vector2.Distance(targetPlantPosition, transform.position) < 1f) {
                    rb.velocity = Vector2.zero;
                    SetMotionState(MoveState.EATPLANT);
                    break;
                }

                Vector2 direction = (targetPlantPosition - transform.position).normalized;
                if (rb.velocity.magnitude < MAX_FLIGHT_SPEED) {
                    rb.AddForce(direction * 10f);
                }
                else {
                    rb.velocity *= 0.9f;
                }
                break;

            case MoveState.EATPLANT:
                if (!targetPlantZone.IsPlanted()) {
                    SetMotionState(MoveState.ROAMING);
                    break;
                }
                else {
                    if (stateTimer > EAT_TIME) {
                        targetPlantZone.Chop();
                        SetMotionState(MoveState.ROAMING);
                        break;
                    }
                    // targetPlant.GetEaten();
                }
                break;

            case MoveState.SEEKPLAYER:
                if (AI.DistanceToPlayer(this.gameObject) > PLAYER_DETECTION_DISTANCE) {
                    SetMotionState(MoveState.ROAMING);
                    break;
                }

                direction = (player.transform.position - transform.position).normalized;
                if (rb.velocity.magnitude < MAX_FLIGHT_SPEED) {
                    rb.AddForce(direction * 10f);
                } else {
                    rb.velocity *= 0.9f;
                }

                break;


            case MoveState.HIT:
                rb.velocity *= 0.93f;
                if (stateTimer > STUN_TIME) {
                    SetMotionState(MoveState.SEEKPLAYER);
                    break;
                }
                break;

            /*
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
        SetMotionState(MoveState.HIT);

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
