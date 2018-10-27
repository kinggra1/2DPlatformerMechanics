using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtPatch : PlantableZone {

    public GameObject wetDirtSprite;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        // Can probably do this more cleanly/on water/grow, but in the current setup that would require a callback?
        // We have the SetActive(true) part handled below, need to figure out SetActive(false)
        // TODO: Consider optimizing
		if (plant != null && plant.IsWatered()) {
            wetDirtSprite.SetActive(true);
        } else {
            wetDirtSprite.SetActive(false);
        }
	}

    public override void Water() {
        base.Water();

        if (plant != null && plant.IsWatered()) {
            wetDirtSprite.SetActive(true);
        }
    }
}
