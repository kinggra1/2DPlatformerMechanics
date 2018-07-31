using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour {

    private static InventorySystem instance = null;

    private PlayerController player;
    private TimeSystem timeSystem;

    // TODO: Make this more complex/interesting. Have a map to InventoryItems of some kind.
    // Tools/resources/etc. Somethings have counts. Some things have links to prefabs. 
    // Definitely need some planning here.
    // Right now it's just a prefab and hard-coded to a platform seed.
    private List<GameObject> itemSlots = new List<GameObject>();
    private int selectedItemIndex = 0;

    // RESOURCES
    private int waterLevel = 1;
    private int maxWaterLevel = 10;

    // Use this for initialization
    void Awake() {
        if (instance != null) {
            return;
        }
        instance = this;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        timeSystem = TimeSystem.GetInstance();


        GameObject platformPlantPrefab = (GameObject)Resources.Load("PlantPrefabs/PlatformPlant", typeof(GameObject));
        itemSlots.Add(platformPlantPrefab);
    }

    public int GetWaterLevel() {
        return waterLevel;
    }

    public float GetWaterLevelPercentage() {
        return (float)waterLevel / maxWaterLevel;
    }

    public void ChangeWaterLevel(int volume) {
        // Can be called with positive or negative values, but will always be [0, maxWaterLevel]
        waterLevel = Mathf.Clamp(waterLevel + volume, 0, maxWaterLevel);
    }

    // now if we forget to put a InventorySystem in the scene, we can still
    // call this and one will be dynamically created
    public static InventorySystem GetInstance() {
        if (instance == null) {
            instance = new GameObject().AddComponent<InventorySystem>();
            instance.name = "InventorySystem";
        }
        return instance;
    }

    private void Update() {
        bool clickPressed = Input.GetMouseButtonDown(0);
        bool waterButtonPressed = Input.GetKeyDown(KeyCode.E);

        if (clickPressed) {
            // Do some check to see if we currently have something equipt here, blah, blah, blah
            // if it's a seed... (need to do this check)
            // check we have a place to plant it
            IPlantableZone plantableZone = player.GetAvailablePlantableZone();
            if (plantableZone != null && plantableZone.CanPlantSeed()) {
                plantableZone.PlantSeed(itemSlots[selectedItemIndex]);
            }
        }

        if (waterButtonPressed) {
            IPlantableZone plantableZone = player.GetAvailablePlantableZone();
            if (plantableZone != null && plantableZone.CanBeWatered()) {
                if (waterLevel > 0) {
                    plantableZone.Water();
                    waterLevel--;
                } else {
                    // lol you've got no water, nerd
                }
            }
        }
    }
}
