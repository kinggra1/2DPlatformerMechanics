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

    protected TimeInstant plantedTime = new TimeInstant();
    protected TimeInstant lastWateredTime = new TimeInstant();
    protected int phaseIndex = 0;

    protected int growTimeDays = 2;

    // Use this for initialization
    void Awake() {
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
                ChangePhaseIndex(1);
                watered = false;
            }
        }
    }

    protected void ChangePhaseIndex(int delta) {
        growthPhases[phaseIndex].SetActive(false);
        phaseIndex += delta;
        phaseIndex = (int)Mathf.Clamp(phaseIndex, 0, growthPhases.Length - 1);
        growthPhases[phaseIndex].SetActive(true);

        // grow to the next phase if we're advancing one phase
        if (delta == 1) {
            IGrowablePhase GrowablePhase = growthPhases[phaseIndex].GetComponent<IGrowablePhase>();
            if (GrowablePhase != null) {
                GrowablePhase.AnimatePhaseGrowth();
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
