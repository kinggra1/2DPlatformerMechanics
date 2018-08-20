using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Growable s are things that can be watered, can grow, and can be "chopped"
 */
public abstract class Growable  : MonoBehaviour {

    // public GameObject associatedSeed;
    public GameObject[] growthPhases;

    // public float timeBetweenPhases;

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

                IGrowablePhase GrowablePhase = nextPhase.GetComponent<IGrowablePhase>();
                if (GrowablePhase != null) {
                    GrowablePhase.AnimatePhaseGrowth();
                }

                watered = false;
            }
        }
    }

    public virtual bool CanBeWatered() {
        return !watered && phaseIndex < growthPhases.Length - 1;
    }

    public virtual void Water() {
        watered = true;
        lastWateredTime = timeSystem.GetTime();
    }

    public virtual bool IsWatered() {
        return watered;
    }


    public virtual void Grow() {
        throw new System.NotImplementedException("<-- What he said.");
    }

    public virtual void Chop() {
        foreach (Harvestable harvestable in GetComponentsInChildren<Harvestable>()) {
            harvestable.Harvest();
        }
    }
}
