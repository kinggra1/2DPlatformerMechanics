using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DewdropPlant : MonoBehaviour, IGrowable {
    
    public GameObject associatedSeed;
    public GameObject[] growthPhases;

    private TimeSystem timeSystem;

    private bool watered = false;

    private TimeInstant plantedTime = new TimeInstant();
    private TimeInstant lastWateredTime = new TimeInstant();
    private int phaseIndex = 0;

    private int growTimeDays = 2;

    // Use this for initialization
    void Start() {
        timeSystem = TimeSystem.GetInstance();
        plantedTime = timeSystem.GetTime();

        foreach (GameObject phase in growthPhases) {
            phase.SetActive(false);
        }
        growthPhases[phaseIndex].SetActive(true);
    }

    // Update is called once per frame
    void Update() {

        if (watered) {
            int elapsedDays = (timeSystem.GetTime() - lastWateredTime).GetDays();
            if (elapsedDays >= growTimeDays) {
                growthPhases[phaseIndex].SetActive(false);
                phaseIndex++;
                GameObject nextPhase = growthPhases[phaseIndex];
                nextPhase.SetActive(true);

                IGrowablePhase growablePhase = nextPhase.GetComponent<IGrowablePhase>();
                if (growablePhase != null) {
                    growablePhase.AnimatePhaseGrowth();
                }

                watered = false;
            }
        }
    }

    bool IGrowable.CanBeWatered() {
        return !watered && phaseIndex < growthPhases.Length - 1;
    }

    void IGrowable.Water() {
        watered = true;
        lastWateredTime = timeSystem.GetTime();
    }

    bool IGrowable.IsWatered() {
        return watered;
    }

    void IGrowable.Grow() {

    }

    void IGrowable.Chop() {
        /*
        for (int i = 0; i < numDroppedSeeds; i++) {
            GameObject spawnedSeed = Instantiate(associatedSeed);
            spawnedSeed.transform.position = this.transform.position;
            Rigidbody2D rb = spawnedSeed.GetComponent<Rigidbody2D>();
            rb.AddForce(new Vector2(Random.Range(-50, 50), Random.Range(100f, 200f)));
        }
        Destroy(this.gameObject);
        */

        // This is a REALLY good reason to have Growable be a base class that
        // does things like this for all derived classes.
        foreach (Harvestable harvestable in GetComponentsInChildren<Harvestable>()) {
            harvestable.Harvest();
        }
    }
}
