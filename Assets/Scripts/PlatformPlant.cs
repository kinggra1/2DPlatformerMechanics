using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformPlant : MonoBehaviour, IGrowable {

    public GameObject[] growthPhases;

    // public float timeBetweenPhases;

    private TimeSystem timeSystem;

    private bool watered = false;

    private float plantedTime;
    private float lastWateredTime = Mathf.Infinity;
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
            int elapsedDays = timeSystem.TimeToDays(timeSystem.GetTime() - lastWateredTime);
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

    void IGrowable.Grow() {

    }
}
