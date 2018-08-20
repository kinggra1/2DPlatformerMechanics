using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A harvestable refers to a single component on a plant that can be "harvested"
 * A plant may have multiple harvestable elements (e.g. 3 fruit in different
 * locations). 
 * When the player is in a plantablezone and "harvests" the plant, all harvestable
 * locations will have Harvest() called.
 * 
 * The Growable  script will manage creating and removing instances of Harvestable.
 * 
 * Example usages:
 * 1. Final phase of a fruit growing plant grows 3 fruit with Harvestable scripts on them.
 *      - Harvesting the Growable  will call Harvest() on all fruit and revert the Growable  back one phase.
 *      - Fruit objects may or may not be destroyed depending on whether or not they have [destroyWhenHarvested] set.
 * 2. The final phase of a resource plant has a Harvestable on its parent object
 *      - Harvesting the Growable  will call Harvest() which destroys the whole plant while spawning resources.
 */
public class Harvestable : MonoBehaviour {

    [Tooltip("GameObject spanwed when Harvest() is called. In most cases this object should have a Collectible script on it.")]
    public GameObject harvestedObject;

    [Tooltip("The minimum number of [harvestedObject] that will be returned when Harvest() is called.")]
    public int minHarvestYield;
    [Tooltip("The maximum number of [harvestedObject] that will be returned when Harvest() is called.")]
    public int maxHarvestYield;

    [Tooltip("Whether the GameObject this script is attached to will be destroyed when Harvest() is called.")]
    public bool destroyWhenHarvested = false;

    public void Harvest() {
        int harvestYield = Mathf.RoundToInt(Random.Range(minHarvestYield, maxHarvestYield));
        for (int i = 0; i < harvestYield; i++) {
            GameObject obj = Instantiate(harvestedObject);
            obj.transform.position = this.transform.position;

            // Could apply random forces to a RigidBody2D here for dynamic drop visuals. 
            // Particularly if multiple things are spanwed.
        }

        if (destroyWhenHarvested) {
            Destroy(this.gameObject);
        }
    }
	
}
