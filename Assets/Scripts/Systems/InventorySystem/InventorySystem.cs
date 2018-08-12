using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour {

    private static InventorySystem instance = null;

    public GameObject itemSlotParent;
    public GameObject cursorImage;

    public Dictionary<Item.Seed, Item> seedItemMap = new Dictionary<Item.Seed, Item>();
    public Dictionary<Item.Tool, Item> toolItemMap = new Dictionary<Item.Tool, Item>();
    public Dictionary<Item.Resource, Item> resourceItemMap = new Dictionary<Item.Resource, Item>();

    private PlayerController player;
    private WaterSpriteController waterSprite;
    private TimeSystem timeSystem;

    private List<InventorySlot> itemSlots = new List<InventorySlot>();

    private int selectedItemIndex = 0;

    private RectTransform cursorRectTransform;
    private RectTransform itemRectTransform;

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
        waterSprite = player.GetWaterSprite();
        timeSystem = TimeSystem.GetInstance();

        // Set up our list of available InventorySlots
        itemSlots = new List<InventorySlot>(GetComponentsInChildren<InventorySlot>());

        LoadItemsFromResources();

        itemSlots[0].Assign(toolItemMap[Item.Tool.Axe], false);
        itemSlots[1].Assign(toolItemMap[Item.Tool.Shovel], false);

        itemSlots[2].Assign(seedItemMap[Item.Seed.PlatformPlant], true, 3);
        itemSlots[3].Assign(seedItemMap[Item.Seed.DewdropPlant], true, 3);

        cursorRectTransform = cursorImage.GetComponent<RectTransform>();
    }

    public void LoadItemsFromResources() {
        GameObject platformPlantSeedItem = (GameObject)Resources.Load("PlantPrefabs/ItemSeedPlatformPlant", typeof(GameObject));
        GameObject dewdropPlantSeedItem = (GameObject)Resources.Load("PlantPrefabs/ItemSeedDewdropPlant", typeof(GameObject));
        seedItemMap.Add(Item.Seed.PlatformPlant, platformPlantSeedItem.GetComponent<Item>());
        seedItemMap.Add(Item.Seed.DewdropPlant, dewdropPlantSeedItem.GetComponent<Item>());

        GameObject axeItem = (GameObject)Resources.Load("ToolPrefabs/ItemToolAxe", typeof(GameObject));
        GameObject itemShovel = (GameObject)Resources.Load("ToolPrefabs/ItemToolShovel", typeof(GameObject));
        toolItemMap.Add(Item.Tool.Axe, axeItem.GetComponent<Item>());
        toolItemMap.Add(Item.Tool.Shovel, itemShovel.GetComponent<Item>());

        GameObject dirtItem = (GameObject)Resources.Load("ResourcePrefabs/ItemResourceDirt", typeof(GameObject));
        resourceItemMap.Add(Item.Resource.Dirt, dirtItem.GetComponent<Item>());
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

            // Check to see if we have a variety of things and if we can use them
            IGrowable plantableSeed = currentItem.GetGamePrefab().GetComponent<IGrowable>();
            DirtPatch dirtPatch = currentItem.GetGamePrefab().GetComponent<DirtPatch>();

            IPlantableZone plantableZone = player.GetAvailablePlantableZone();

            // Planting a seed
            if (plantableSeed != null) {
                if (plantableZone != null && !plantableZone.IsPlanted()) {
                    plantableZone.PlantSeed(currentItem.GetGamePrefab());
                    currentItem.Use();
                }
            }
            // Placing dirt on the ground
            else if (dirtPatch != null) {
                if (plantableZone == null && player.OnPlantableGround()) {
                    Instantiate(dirtPatch).transform.position = player.transform.position + Vector3.down * 0.5f;
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

    internal void UseTool(Item.Tool toolType) {

        IPlantableZone currentPlantableZone = player.GetAvailablePlantableZone();

        switch (toolType) {

            case Item.Tool.Axe:
                if (currentPlantableZone != null) {
                    if (currentPlantableZone.IsPlanted()) {
                        currentPlantableZone.Chop();
                    }
                }
                else {
                    // Swiping at empty space with axe
                }
                break;

            case Item.Tool.Shovel:
                if (currentPlantableZone != null) {
                    // can only dig up empty dirt patch
                    if (!currentPlantableZone.IsPlanted()) {
                        Destroy((currentPlantableZone as MonoBehaviour).gameObject);

                        // reworked the InvetorySystem to track a dictionary of items from enums, this is so much nicer now
                        PickupItem(resourceItemMap[Item.Resource.Dirt]);

                        // seriously look at how gross this used to be
                        // PickupItem(((GameObject)Resources.Load("PlantPrefabs/ItemDirtPatch", typeof(GameObject))).GetComponent<Item>());
                    }
                }
                else {
                    // Swiping at empty space with shovel
                }
                break;

                // handle other tools here
        }
    }
}
