using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* A smaller version of WaterSprite that only has a single target, flies 
 * directly to it quickly, attacks it, and is destroyed
 */
public class WaterSpriteOffspring : MonoBehaviour {
    private GameObject target = null;

    private Vector3 targetVector;
    private SpriteRenderer sprite;
    private GameObject body;

    Vector2 velocity = Vector2.zero;

    // Use this for initialization
    void Start () {
        sprite = this.GetComponentInChildren<SpriteRenderer>();
        body = sprite.gameObject;
    }
    
    // Update is called once per frame
    void Update () {

        // We should be heading toward a target if there is one
        if (target != null) {
            targetVector = target.transform.position - this.transform.position;
            targetVector *= 10f;
        }
        else {
            // TODO: Consider code to find a new enemy of existing target gets destroyed
            // but for now
            Destroy(this.gameObject);
            return;
        }

        if (Vector2.Distance(this.transform.position, target.transform.position) < 0.5f) {
            InteractWithTarget(); // this could set target to be null if we ran out of targets
        }

        velocity = targetVector;


        // the closer we are to the target, the more we dampen velocity change
        // Dividing by velocity.magnitude means we'll slow down if we're going faster than
        // [distance-to-player]/second, with stronger decreases the closer we get to the player
        velocity *= Mathf.Clamp(targetVector.magnitude/velocity.magnitude, 0.8f, 1.0f);
        this.transform.position += new Vector3(velocity.x, velocity.y) * Time.deltaTime;

        // point us in the direction of velocity
        sprite.gameObject.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);

        // and stretch based on current velocity
        float scaleFactor = velocity.magnitude / 8f;
        // float waterLevel = 0f;
        float waterLevelScalar = 0.25f; // 25% radius of original
        sprite.gameObject.transform.localScale = new Vector3(Mathf.Clamp(scaleFactor, 1f, 2f), 1f-Mathf.Clamp(scaleFactor, 0f, 0.25f), 1f) * waterLevelScalar;
    }

    public void SetTarget(GameObject newTarget) {
        this.target = newTarget;
    }

    private void InteractWithTarget() {

        if (target != null) {

            // If current target is a collectible, call Collect() on it (free for you, cheap for them (tm) )
            Collectible collectible = target.GetComponent<Collectible>();
            if (collectible) {
                collectible.CollectIfPossible();
            }

            // If current target is a PlantableZone, water it
            PlantableZone plantableZone = target.GetComponent<PlantableZone>();
            if (plantableZone != null) {
                plantableZone.Water();
            }

            // If current target is a Strikable, strike it up
            IStrikeable strikable = target.GetComponent<IStrikeable>();
            if (strikable != null) {
                strikable.Strike(this.transform.position, null);
            }
        }

        Destroy(this.gameObject);
    }
}
