using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Spawn a GameObject at this location after a certain amount of in-game time.
 * 
 * Can be used to spawn something once and then be destroyed using [oneOff]
 * Can spawn infinite things at a constant rate [multispawner]
 * Can spawn a single object and then wait until it is destroyed (e.g. collected) before resetting
 */
public class TimedSpawner : MonoBehaviour {

    // TODO: Make some kind of custom inspector for TimeInstant so that all times can use
    // that class

    [Tooltip("The GameObject that will be spawned.")]
    public GameObject prefab;
    [Tooltip("How many in game days until the GameObject spanws.")]
    public float spawnDelayDays = 1; // TODO: give this some random jitter

    [Tooltip("If true, this spawner will not wait for the spawned object to be destroyed to spawn another.")]
    public bool multispawner = false;
    [Tooltip("Only spawn a single instance then destroy the object this is attached to. Takes precedence over [multispawner]")]
    public bool oneOff = false;

    private GameObject lastSpawnedObject = null;

    private TimeSystem timeSystem;
    private TimeInstant timerStart = new TimeInstant();

    private bool spawned = false;

	// Use this for initialization
	void Start () {
        timeSystem = TimeSystem.GetInstance();
        timerStart = timeSystem.GetTime();
	}
	
	// Update is called once per frame
	void Update () {
        if (lastSpawnedObject == null || multispawner) {

            // Dewdrop was just collected, reset timing
            if (spawned) {
                timerStart = timeSystem.GetTime();
                spawned = false;
            }

            int elapsedDays = (timeSystem.GetTime() - timerStart).GetDays();
            if (elapsedDays >= spawnDelayDays) {
                SpawnObject();
                if (oneOff) {
                    Destroy(this.gameObject);
                }
            }
        }
	}

    private void SpawnObject() {
        lastSpawnedObject = Instantiate(prefab, this.transform) as GameObject;
        lastSpawnedObject.transform.localPosition = Vector3.zero;

        if (multispawner) {
            // reset timing if we're a multispawner
            timerStart = timeSystem.GetTime();
        } else {
            spawned = true;
        }
    }
}
