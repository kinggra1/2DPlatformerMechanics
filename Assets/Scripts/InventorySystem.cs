using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour {

    private static InventorySystem instance = null;

    public GameObject itemSlotParent;
    public GameObject cursorImage;

    private PlayerController player;
    private WaterSpriteController waterSprite;
    private TimeSystem timeSystem;

    // TODO: Make this more complex/interesting. Have a map to InventoryItems of some kind.
    // Tools/resources/etc. Somethings have counts. Some things have links to prefabs. 
    // Definitely need some planning here.
    // Right now it's just a prefab and hard-coded to a platform seed.
    private List<InventorySlot> itemSlots = new List<InventorySlot>();
    private int selectedItemIndex = 0;

    private RectTransform cursorRectTransform;
    private RectTransform itemRectTransform;

    // RESOURCES
    private int waterLevel = 1;
    private int maxWaterLevel = 10;

    // Use this for initialization
    void Start() {
        if (instance != null) {
            return;
        }
        instance = this;

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        waterSprite = player.GetWaterSprite();
        timeSystem = TimeSystem.GetInstance();

        // Set up our list of available InventorySlots
        itemSlots = new List<InventorySlot>(GetComponentsInChildren<InventorySlot>());

        GameObject platformPlantSeedItem = (GameObject)Resources.Load("PlantPrefabs/ItemSeedPlatformPlant", typeof(GameObject));
        GameObject axeItem = (GameObject)Resources.Load("ToolPrefabs/ItemToolAxe", typeof(GameObject));
        itemSlots[selectedItemIndex].Assign(platformPlantSeedItem.GetComponent<Item>(), true, 3);

        itemSlots[1].Assign(axeItem.GetComponent<Item>(), false);

        cursorRectTransform = cursorImage.GetComponent<RectTransform>();
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

    public void PickupItem(Item item) {
        // Check to see if this item already exists somewhere in our inventory if it's a consumable
        if (item.IsConsumable()) {
            foreach (InventorySlot slot in itemSlots) {
                if (!slot.IsEmpty() && slot.IsConsumable() && slot.GetItem().Equals(item)) {
                    slot.IncrementCount(1);
                    return;
                }
            }
        }

        // If all of the above fails, put it in the first free inventory slot
        foreach (InventorySlot slot in itemSlots) {
            if (slot.IsEmpty()) {
                slot.Assign(item, item.IsConsumable(), 1);
                return;
            }
        }
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

        int indexChange = Mathf.RoundToInt(10f*Input.GetAxis("Mouse ScrollWheel"));
        
        if (indexChange != 0) {
            selectedItemIndex -= indexChange;
            if (selectedItemIndex < 0) {
                selectedItemIndex += itemSlots.Count;
            }
            if (selectedItemIndex >= itemSlots.Count) {
                selectedItemIndex -= itemSlots.Count;
            }
            itemRectTransform = itemSlots[selectedItemIndex].gameObject.GetComponent<RectTransform>();
            cursorRectTransform.anchoredPosition = new Vector2(itemRectTransform.localPosition.x, 0f);
        }

        if (clickPressed) {
            InventorySlot currentItem = itemSlots[selectedItemIndex];

            // Right now all we can do in Update is use items, so nothing left to do.
            if (currentItem.IsEmpty()) {
                return;
            }

            // Check to see if we have something that can be planted and a place to plant it
            IGrowable plantableSeed = currentItem.GetGamePrefab().GetComponent<IGrowable>();
            IPlantableZone plantableZone = player.GetAvailablePlantableZone();
            if (plantableSeed != null) {
                if (plantableZone != null && !plantableZone.IsPlanted()) {
                    plantableZone.PlantSeed(currentItem.GetGamePrefab());
                    currentItem.Use();
                }
            } else {
                currentItem.Use();
            }
        }

        if (waterButtonPressed) {
            IPlantableZone plantableZone = player.GetAvailablePlantableZone();
            if (plantableZone != null && plantableZone.CanBeWatered()) {
                if (waterLevel > 0) {
                    
                    // Everything that we have that implements interfaces is also a MonoBehavior, so we can
                    // use this as a """safe""" cast in order to find the game object
                    // The water sprite reaching the PlantableZone will handle the watering itself.
                    waterSprite.AddImmediateToTargetList((plantableZone as MonoBehaviour).gameObject);

                    // TODO: Consider implications of this call. It means we can't possibly overwater, but it
                    // also changes the watersprite visual before it actually reaches the PlantableZone
                    ChangeWaterLevel(-1);

                } else {
                    // lol you've got no water, nerd
                }
            }
        }
    }
}
