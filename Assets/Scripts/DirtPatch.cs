using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtPatch : MonoBehaviour, IPlantableZone {

    // TODO: Refactor plant so that this is a "Growable" and the phases are something else
    private IGrowable plant = null;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    bool IPlantableZone.CanPlantSeed() {
        return plant == null;
    }

    void IPlantableZone.Fertalize() {
        throw new System.NotImplementedException();
    }

    void IPlantableZone.PlantSeed(GameObject seed) {
        plant = Instantiate(seed, this.transform).GetComponent<IGrowable>();
    }

    bool IPlantableZone.CanBeWatered() {
        return plant != null && plant.CanBeWatered();
    }

    void IPlantableZone.Water() {
        plant.Water();
    }

    void OnTriggerEnter2D(Collider2D collider) {
        PlayerController player = collider.GetComponentInParent<PlayerController>();
        if (player) {
            //Debug.Log("In plantable zone");
            player.SetAvailablePlantableZone(this);
        }
    }

    void OnTriggerExit2D(Collider2D collider) {
        PlayerController player = collider.GetComponentInParent<PlayerController>();
        if (player && (DirtPatch)player.GetAvailablePlantableZone() == this) {
            //Debug.Log("No plantable zone");
            player.SetAvailablePlantableZone(null);
        }
    }
}
