using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Represents a location where Growable objects can be planted.
 * 
 * TODO: Consider refactoring from an interface into a class that handles most
 * of the pass-through calls to the Growable . Others can override for special 
 * logic, but I imagine most of the time calling "Water()" will just call water
 * on the associated Growable  object.
 */
public abstract class PlantableZone : MonoBehaviour {

    public enum Type { DirtPatch };

    public Item.Seed seedType; // used for saving
    protected Growable plant = null;
    public Type zoneType;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public virtual bool IsPlanted() {
        return plant != null;
    }

    public virtual void Fertalize() {
        throw new System.NotImplementedException();
    }

    public virtual void PlantSeed(ItemSeed seed) {
        seedType = seed.seedType;
        GameObject plantObject = seed.GetGamePrefab();

        plant = Instantiate(plantObject, this.transform).GetComponent<Growable>();
        // Make sure we have a Growable being planted
        if (plant == null) {
            Debug.LogError("Error trying to plant seed.");
        }
    }

    public virtual bool CanBeWatered() {
        return plant != null && plant.CanBeWatered();
    }

    public virtual void Water() {
        if (plant != null) {
            plant.Water();
        } else {
            Debug.LogError("Trying to water a PlantableZone with no plant!");
        }

    }

    public virtual void Chop() {
        plant.Chop();

        // One chop chump
        //ResetPatch();
    }

    protected void ResetPatch() {
        if (plant) {
            Destroy(plant.gameObject);
        }
        plant = null;
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








    public virtual PlantableZoneData Save() {
        PlantableZoneData data = new PlantableZoneData();

        data.location = new SerializedVector(this.transform.position);
        data.isPlanted = IsPlanted();
        data.seedType = seedType;
        data.zoneType = zoneType;

        if (plant != null) {
            data.plantData = plant.Save();
        }

        return data;
    }

    public virtual void Load(PlantableZoneData data) {
        ResetPatch();

        this.transform.position = data.location.AsVector3();
        if (data.isPlanted) {
            PlantSeed(InventorySystem.GetInstance().seedItemMap[data.seedType]);
            plant.Load(data.plantData);
        }
    }
}

[Serializable]
public class SerializedVector {
    private float x;
    private float y;
    private float z;

    public SerializedVector(Vector3 vector) {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public SerializedVector(Vector2 vector) {
        x = vector.x;
        y = vector.y;
    }

    public Vector3 AsVector3() {
        return new Vector3(x, y, z);
    }

    public Vector2 AsVector2() {
        return new Vector2(x, y);
    }
}

[Serializable]
public class PlantableZoneData {
    public SerializedVector location;
    public bool isPlanted;
    public Item.Seed seedType;
    public PlantableZone.Type zoneType; // Used by parent loading to create the correct type of PlantableZone
    public GrowableData plantData = null;

    // This will be set by the scene Save function to store proper order of plants on plants
    public int parentGrowableIndex;
}
