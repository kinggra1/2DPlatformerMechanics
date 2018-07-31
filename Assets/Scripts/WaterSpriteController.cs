using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpriteController : MonoBehaviour {

    private float xSwayRadius = 0.1f;
    private float ySwayRadius = 0.1f;
    private Vector3 floatingOffset = new Vector3(1.2f, 1f, 0f);

    // List of things we should fly to, in order
    private List<GameObject> targetList = new List<GameObject>();
    private int nextTargetIndex = 0;
    private GameObject target = null;

    private InventorySystem inventory;
    private PlayerController player;
    private Rigidbody2D playerRigidbody;
    private Vector3 targetPosition;
    private Vector3 localOffset = Vector3.zero;
    private SpriteRenderer sprite;
    private GameObject body;
    private ParticleSystem drippingParticles;

    Vector2 velocity = Vector2.zero;

    private float randomSwayFactor;

	// Use this for initialization
	void Start () {
        inventory = InventorySystem.GetInstance();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        playerRigidbody = player.GetComponent<Rigidbody2D>();

        sprite = this.GetComponentInChildren<SpriteRenderer>();
        body = sprite.gameObject;

        drippingParticles = this.GetComponentInChildren<ParticleSystem>();

        // randomSwayFactor = Random.Range(0.1f, 0.8f);
	}
	
	// Update is called once per frame
	void Update () {

        // Update whether which target (if any) we should be going to
        if (target == null) {
            if (targetList.Count > 0) {
                NextTarget();
            }
        } else if (Vector3.Distance(this.transform.position, target.transform.position) < 0.5f) {
            NextTarget(); // this could set target to be null if we ran out of targets
        }

        // We should be heading toward a target if there is one
        if (target != null) {
            targetPosition = target.transform.position - this.transform.position;
            targetPosition *= 5f;
        } else { // target defaults to player
            Vector3 targetOffset = floatingOffset;
            // Mirror the floating offset depending on the direction the player is facing
            if (player.PlayerFacing() == PlayerController.Direction.RIGHT) {
                targetOffset.x *= -1;
            }

            // "Random" bobbing around
            if (true) {
                localOffset.x = Mathf.Cos(Time.time) * xSwayRadius;
                localOffset.y = Mathf.Cos(Time.time * 2.2f) * ySwayRadius;
            }
            targetPosition = player.transform.position + (targetOffset + localOffset) - this.transform.position;
        }

        velocity = targetPosition;



        // the closer we are to the player, the more we dampen velocity change
        // Dividing by velocity.magnitude means we'll slow down if we're going faster than
        // [distance-to-player]/second, with stronger decreases the closer we get to the player
        velocity *= Mathf.Clamp(targetPosition.magnitude/velocity.magnitude, 0.8f, 1.0f);
        this.transform.position += new Vector3(velocity.x, velocity.y) * Time.deltaTime;

        // point us in the direction of velocity
        sprite.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);

        // and stretch based on current velocity
        float scaleFactor = velocity.magnitude / 8f;
        float waterLevel = inventory.GetWaterLevelPercentage();
        float waterLevelScalar = waterLevel * 0.5f + 0.5f; // make sure we're no less than 50% size
        ParticleSystem.SizeOverLifetimeModule dropSize = drippingParticles.sizeOverLifetime;
        dropSize.sizeMultiplier = waterLevelScalar;
        sprite.gameObject.transform.localScale = new Vector3(Mathf.Clamp(scaleFactor, 1f, 2f), 1f-Mathf.Clamp(scaleFactor, 0f, 0.25f), 1f) * waterLevelScalar;

        if (Mathf.Approximately(waterLevel, 0f)) {
            
        } else {

        }
        //drippingParticles.transform.rotation = Quaternion.Inverse(transform.rotation);
    }

    private void NextTarget() {

        if (target != null) {

            // If this is a collectible, call Collect() on it (free for you, cheap for them (tm) )
            Collectible collectible = target.GetComponent<Collectible>();
            if (collectible) {
                collectible.Collect();
            }
        }

        // We've run out of things to fly to, clear the list
        // This also covers the case where the list is empty
        if (nextTargetIndex >= targetList.Count) {
            nextTargetIndex = 0;
            targetList.Clear();
            target = null;
        } else {
            target = targetList[nextTargetIndex++];
        }
    }

    public void AddToTargetList(GameObject target) {
        targetList.Add(target);
    }
}
