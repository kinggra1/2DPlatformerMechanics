﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformPlant : MonoBehaviour {

    public GameObject[] growthPhases;

    public float timeBetweenPhases;

    TimeSystem timeSystem;

    private float plantedTime;
    private int phaseIndex = 0;

	// Use this for initialization
	void Start () {
        timeSystem = TimeSystem.GetInstance();
        plantedTime = timeSystem.GetTime();

        foreach (GameObject phase in growthPhases) {
            phase.SetActive(false);
        }
        growthPhases[phaseIndex].SetActive(true);
	}
	
	// Update is called once per frame
	void Update () {
        int day = timeSystem.TimeToDays(plantedTime - timeSystem.DawnOfTime());

        if (day != phaseIndex && day < growthPhases.Length) {
            growthPhases[phaseIndex].SetActive(false);
            growthPhases[++phaseIndex].SetActive(true);
        }
	}
}
