using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSpriteController : MonoBehaviour {

    public GameObject waterSpriteChildPrefab;

    private Item.ElementCrystal crystal = Item.ElementCrystal.None;

    private float xSwayRadius = 0.2f;
    private float ySwayRadius = 0.2f;
    private Vector3 floatingOffset = new Vector3(1.2f, 1f, 0f);

    // List of things we should fly to, in order
    private List<GameObject> targetList = new List<GameObject>();
    private int nextTargetIndex = 0;
    private GameObject target = null;

    private InventorySystem inventory;
    private PlayerController player;
    private Rigidbody2D playerRigidbody;
    private Vector3 targetVector;
    private Vector3 localOffset = Vector3.zero;
    private SpriteRenderer sprite;
    private GameObject body;
    private ParticleSystem drippingParticles;

    Vector2 velocity = Vector2.zero;

    private float randomSwayFactor;

    private bool specialButtonPressed;

	// Use this for initialization
	void Start () {
        inventory = InventorySystem.GetInstance();

        player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
        playerRigidbody = player.GetRigidbody();

        sprite = this.GetComponentInChildren<SpriteRenderer>();
        body = sprite.gameObject;

        drippingParticles = this.GetComponentInChildren<ParticleSystem>();

        // randomSwayFactor = Random.Range(0.1f, 0.8f);
	}
	
	// Update is called once per frame
	void FixedUpdate() {

        // TODO: Update this to work with Input Manager
        specialButtonPressed = Input.GetKeyDown(KeyCode.E);

        if (specialButtonPressed) {
            TargetNearbyEnemies();
        }

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
            targetVector = target.transform.position - this.transform.position;
            targetVector *= 5f;
        } else { // target defaults to player
            Vector3 targetOffset = floatingOffset;
            // Mirror the floating offset depending on the direction the player is facing
            if (player.PlayerFacing() == AI.Direction.RIGHT) {
                targetOffset.x *= -1;
            }

            // "Random" bobbing around
            if (true) {
                localOffset.x = Mathf.Cos(Time.time) * xSwayRadius;
                localOffset.y = Mathf.Cos(Time.time * 2.2f) * ySwayRadius;
            }
            targetVector = player.transform.position + (targetOffset + localOffset) - this.transform.position;
        }

        if (target == null) {
            // We're heading towards the player, let's do some fancy spirally movement here

            // set targetVector to point fro player to waterSprite for polar calculations
            targetVector = -targetVector;
            bool abovePlayer = targetVector.y > 0f;
            bool rightOfPlayer = targetVector.x > 0f;

            bool rotateClockwise = abovePlayer && rightOfPlayer || !abovePlayer && !rightOfPlayer;
            Vector3 endVector;
            if (abovePlayer) {
                endVector = Vector3.up;
            } else {
                endVector = Vector3.up;
            }

            // Find the angle between our vector from the player and the end vector
            float travelAngle = Vector3.Angle(targetVector, endVector);

            travelAngle = Mathf.Deg2Rad * travelAngle * 0.05f;
            if (rightOfPlayer) {
                travelAngle = -travelAngle;
            }

            // Polar rotation 10% in the correct direction
            float newX = targetVector.x * Mathf.Cos(travelAngle) - targetVector.y * Mathf.Sin(travelAngle);
            float newY = targetVector.x * Mathf.Sin(travelAngle) + targetVector.y * Mathf.Cos(travelAngle);

            Vector3 targetOrbitPosition = this.transform.position - targetVector + new Vector3(newX, newY, 0f) * 0.6f;
            Debug.DrawLine(transform.position, transform.position - targetVector, Color.red);
            Debug.DrawLine(transform.position, targetOrbitPosition);
            velocity = (targetOrbitPosition - transform.position).normalized * targetVector.magnitude * 2f;
            Debug.DrawLine(transform.position, transform.position + new Vector3(velocity.x, velocity.y, 0f), Color.blue);

        } else {
            velocity = targetVector;
        }


        // the closer we are to the player, the more we dampen velocity change
        // Dividing by velocity.magnitude means we'll slow down if we're going faster than
        // [distance-to-player]/second, with stronger decreases the closer we get to the player
        velocity *= Mathf.Clamp(targetVector.magnitude/velocity.magnitude, 0.8f, 1.0f);
        this.transform.position += new Vector3(velocity.x, velocity.y) * Time.fixedDeltaTime;

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
            
        } 
    }

    private int ClosestComponentCompare(Component x, Component y) {
        float xDist = Vector3.Distance(x.transform.position, this.transform.position);
        float yDist = Vector3.Distance(y.transform.position, this.transform.position);

        if (xDist < yDist) {
            return -1;
        } else if (xDist > yDist) {
            return 1;
        } else {
            return 0;
        }
    }

    private void TargetNearbyEnemies() {

        // Find all things on "Enemy" layer 10m around us
        Collider2D[] enemiesToAttack = Physics2D.OverlapCircleAll(player.transform.position, 10f, AI.EnemyLayermask);
        System.Array.Sort<Component>(enemiesToAttack, new System.Comparison<Component>(ClosestComponentCompare));
        foreach(Collider2D col in enemiesToAttack) {
            // We can only hit things that have IStrikeable attached
            if (col.gameObject.GetComponent<IStrikeable>() != null) {
                // AddImmediateToTargetList(col.gameObject);
                // Send a child off to attack this
                if (inventory.GetWaterLevel() > 0) {
                    GameObject offspring = Instantiate(waterSpriteChildPrefab);
                    offspring.transform.position = this.transform.position;
                    WaterSpriteOffspring waterSpriteOffspring = offspring.GetComponent<WaterSpriteOffspring>();
                    waterSpriteOffspring.SetTarget(col.gameObject);

                    inventory.ChangeWaterLevel(-1);
                }
            }
        }
    }

    private void NextTarget() {

        if (target != null) {

            // If current target is a collectible, call Collect() on it (free for you, cheap for them (tm) )
            Collectible collectible = target.GetComponent<Collectible>();
            if (collectible) {
                collectible.CollectIfPossible();
            }

            // If current target is a PlantableZone, water it
            IPlantableZone plantableZone = target.GetComponent<IPlantableZone>();
            if (plantableZone != null) {
                plantableZone.Water();
            }

            // If current target is a Strikable, strike it up
            IStrikeable strikable = target.GetComponent<IStrikeable>();
            if (strikable != null) {
                strikable.Strike(this.transform.position, null);
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

    // Make sure that the next target we go for is this
    public void AddImmediateToTargetList(GameObject target) {
        targetList.Insert(nextTargetIndex, target);
    }

    public bool PlanningToVisit(GameObject target) {
        foreach (GameObject t in targetList) {
            if (t.Equals(target)) {
                return true;
            }
        }
        return false;
    }
}
