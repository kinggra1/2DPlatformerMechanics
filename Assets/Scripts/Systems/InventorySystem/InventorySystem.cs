﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour {

    private static InventorySystem instance = null;

    public GameObject itemSlotParent;
    public GameObject cursorImage;

    public Dictionary<Item.Seed, ItemSeed> seedItemMap = new Dictionary<Item.Seed, ItemSeed>();
    public Dictionary<Item.Weapon, ItemWeapon> weaponItemMap = new Dictionary<Item.Weapon, ItemWeapon>();
    public Dictionary<Item.Resource, Item> resourceItemMap = new Dictionary<Item.Resource, Item>();

    public Dictionary<PlantableZone.Type, GameObject> plantableZonePrefabMap = new Dictionary<PlantableZone.Type, GameObject>();

    private PlayerController player;
    private WaterSpriteController waterSprite;
    private TimeSystem timeSystem;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();

    private int selectedItemIndex = 0;

    private RectTransform cursorRectTransform;
    private RectTransform itemRectTransform;

    private float toolUsageCooldownTimer = 0f;

    // RESOURCES
    private int waterLevel = 1;
    private int maxWaterLevel = 10;

    // Use this for initialization
    void Awake() {

        if (instance == null) {
            // Keep this object around between scenes.
            DontDestroyOnLoad(this.transform.parent.gameObject);
            instance = this;
        }
        else if (instance != this) {
            Destroy(this.transform.parent.gameObject);
        }
    }

    void Start() {
        player = PlayerController.GetInstance();
        waterSprite = player.GetWaterSprite();
        timeSystem = TimeSystem.GetInstance();

        // Set up our list of available InventorySlots
        inventorySlots = new List<InventorySlot>(GetComponentsInChildren<InventorySlot>());

        LoadItemsFromResources();

        inventorySlots[0].Assign(weaponItemMap[Item.Weapon.Axe]);
        inventorySlots[1].Assign(weaponItemMap[Item.Weapon.Shovel]);
        inventorySlots[2].Assign(weaponItemMap[Item.Weapon.Sword]);

        inventorySlots[3].Assign(seedItemMap[Item.Seed.PlatformPlant], 5);
        inventorySlots[4].Assign(seedItemMap[Item.Seed.DewdropPlant], 1);
        inventorySlots[5].Assign(seedItemMap[Item.Seed.FruitPlantOrange], 3);

        cursorRectTransform = cursorImage.GetComponent<RectTransform>();

        UpdateUI();
    }

    // TODO: Make this assignment happen through a ScriptableObject or something instead of Resources loading
    public void LoadItemsFromResources() {
        GameObject platformPlantSeedItem = (GameObject)Resources.Load("PlantPrefabs/Inventory/ItemSeedPlatformPlant", typeof(GameObject));
        GameObject dewdropPlantSeedItem = (GameObject)Resources.Load("PlantPrefabs/Inventory/ItemSeedDewdropPlant", typeof(GameObject));
        GameObject fruitOrangePlantSeedItem = (GameObject)Resources.Load("PlantPrefabs/Inventory/ItemSeedFruitPlantOrange", typeof(GameObject));
        seedItemMap.Add(Item.Seed.PlatformPlant, platformPlantSeedItem.GetComponent<ItemSeed>());
        seedItemMap.Add(Item.Seed.DewdropPlant, dewdropPlantSeedItem.GetComponent<ItemSeed>());
        seedItemMap.Add(Item.Seed.FruitPlantOrange, fruitOrangePlantSeedItem.GetComponent<ItemSeed>());

        GameObject axeItem = (GameObject)Resources.Load("ToolPrefabs/Inventory/ItemToolAxe", typeof(GameObject));
        GameObject shovelItem = (GameObject)Resources.Load("ToolPrefabs/Inventory/ItemToolShovel", typeof(GameObject));
        GameObject swordItem = (GameObject)Resources.Load("WeaponPrefabs/Inventory/ItemWeaponSword", typeof(GameObject));
        weaponItemMap.Add(Item.Weapon.Axe, axeItem.GetComponent<ItemWeapon>());
        weaponItemMap.Add(Item.Weapon.Shovel, shovelItem.GetComponent<ItemWeapon>());
        weaponItemMap.Add(Item.Weapon.Sword, swordItem.GetComponent<ItemWeapon>());

        GameObject dirtItem = (GameObject)Resources.Load("ResourcePrefabs/ItemResourceDirt", typeof(GameObject));
        resourceItemMap.Add(Item.Resource.Dirt, dirtItem.GetComponent<Item>());

        GameObject dirtPatch = (GameObject)Resources.Load("PlantableZonePrefabs/DirtPatch", typeof(GameObject));
        plantableZonePrefabMap.Add(PlantableZone.Type.DirtPatch, dirtPatch);
    }

    public bool WaterLevelFull() {
        return waterLevel >= maxWaterLevel;
    }

    public void FillWaterLevel() {
        waterLevel = maxWaterLevel;
    }

    public int GetWaterLevel() {
        return waterLevel;
    }

    public float GetWaterLevelPercentage() {
        return (float)waterLevel / maxWaterLevel;
    }

    public void ChangeWaterLevel(int volumeChange) {
        // Can be called with positive or negative values, but will always be [0, maxWaterLevel]
        waterLevel = Mathf.Clamp(waterLevel + volumeChange, 0, maxWaterLevel);
    }

    public bool CanPickupItem(Item item)
    {
        // Check to see if this item can be stacked on another consumable
        if (item.IsConsumable()) {
            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.IsEmpty() && slot.IsConsumable() && slot.GetItem().Equals(item)) {
                    // TODO: If we add resource stacking limits for consumables, this is where to check those
                    return true;
                }
            }
        }

        // Check to see if there are any empty slots
        foreach (InventorySlot slot in inventorySlots) {
            if (slot.IsEmpty()) {
                return true;
            }
        }

        return false;
    }

    // Usages should probably always be guarded with checks to CanPickupItem
    public void PickupItem(Item item) {
        // Check to see if this item already exists somewhere in our inventory if it's a consumable
        if (item.IsConsumable()) {
            foreach (InventorySlot slot in inventorySlots) {
                if (!slot.IsEmpty() && slot.IsConsumable() && slot.GetItem().Equals(item)) {
                    // TODO: If we add resource stacking limits for consumables, this is where to check those
                    slot.IncrementCount(1);
                    return;
                }
            }
        }

        // If all of the above fails, put it in the first free inventory slot
        foreach (InventorySlot slot in inventorySlots) {
            if (slot.IsEmpty()) {
                slot.Assign(item, 1);
                return;
            }
        }

        // Uhoh, there are no remaining open inventory slots, and this will involve rewriting
        // a number of systems, particularly sending the WaterSprite out ot go pick up Collectables 
        Debug.LogError("We shouldn't have tried picking this up. Oops. Fix your code.");
    }

    public static InventorySystem GetInstance() {
        if (instance == null) {
            Debug.LogError("Scene needs a Canvas + Inventory");
            //instance = new GameObject().AddComponent<InventorySystem>();
            //instance.name = "InventorySystem";
        }
        return instance;
    }

    private void Update() {

        if (toolUsageCooldownTimer > 0f) {
            toolUsageCooldownTimer -= Time.deltaTime;
        }

        bool useItemPressed = Input.GetButton("Fire1");
        bool useItemSpecialPressed = Input.GetButton("SpecialAttack");
        bool waterButtonPressed = Input.GetButton("Fire2");

        int indexChange = 0;
        indexChange = Mathf.RoundToInt(10f*Input.GetAxis("Mouse ScrollWheel"));
        indexChange += Input.GetButtonDown("NavLeft")? -1 : 0;
        indexChange += Input.GetButtonDown("NavRight") ? 1 : 0;
        
        if (indexChange != 0) {
            SetSelectedItemIndex(selectedItemIndex += indexChange);
        }

        if (useItemPressed) {
            InventorySlot currentItem = inventorySlots[selectedItemIndex];

            // Right now all we can do is use items, so nothing left to do.
            if (currentItem.IsEmpty()) {
                return;
            }

            // Check to see if we have a variety of things and if we can use them
            Growable  plantableSeed = currentItem.GetGamePrefab().GetComponent<Growable>();
            DirtPatch dirtPatch = currentItem.GetGamePrefab().GetComponent<DirtPatch>();

            PlantableZone plantableZone = player.GetAvailablePlantableZone();

            // Planting a seed
            if (plantableSeed != null) {
                ItemSeed seed = (ItemSeed)currentItem.GetItem();
                if (plantableZone != null && !plantableZone.IsPlanted()) {
                    plantableZone.PlantSeed(seed);
                    currentItem.Use();
                }
            }
            // Placing dirt on the ground
            else if (dirtPatch != null) {
                if (plantableZone == null && player.OnPlantableGround()) {

                    GameObject dirt = Instantiate(dirtPatch.gameObject);

                    // Assign the dirt patch to be parented to whatever the player is standing on
                    // This allows us to recursively destroy plants after we plant on top of them 
                    // (e.g. dirt pile on a leaf platform. Destroy bottom plant, it destroys the rest)
                    Transform parent = player.GetObjectBelow().transform;
                    dirt.transform.parent = parent;

                    // Place this dirt roughly on the ground
                    dirt.transform.position = player.transform.position + Vector3.down * 0.5f;
                    currentItem.Use();
                }
            } else {
                currentItem.Use();
            }
        }

        if (useItemSpecialPressed) {
            InventorySlot currentItem = inventorySlots[selectedItemIndex];

            // Make sure we have something equipped.
            if (currentItem.IsEmpty()) {
                return;
            }

            currentItem.UseSpecial();
        }

        if (waterButtonPressed) {
            PlantableZone plantableZone = player.GetAvailablePlantableZone();
            if (plantableZone != null && plantableZone.CanBeWatered()) {
                GameObject target = (plantableZone as MonoBehaviour).gameObject;
                if (waterLevel > 0) {

                    if (!waterSprite.PlanningToVisit(target)) {
                        // Everything that we have that implements interfaces is also a MonoBehavior, so we can
                        // use this as a """safe""" cast in order to find the game object
                        // The water sprite reaching the PlantableZone will handle the watering itself.
                        waterSprite.AddImmediateToTargetList((plantableZone as MonoBehaviour).gameObject);

                        // TODO: Consider implications of this call. It means we can't possibly overwater, but it
                        // also changes the watersprite visual before it actually reaches the PlantableZone
                        ChangeWaterLevel(-1);
                    } else {
                        Debug.LogError("D:");
                    }

                } else {
                    // lol you've got no water, nerd
                }
            }
        }


        // Quick and dirty keypress check for inventory slots
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            SetSelectedItemIndex(0);
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            SetSelectedItemIndex(1);
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            SetSelectedItemIndex(2);
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            SetSelectedItemIndex(3);
        } else if (Input.GetKeyDown(KeyCode.Alpha5)) {
            SetSelectedItemIndex(4);
        } else if (Input.GetKeyDown(KeyCode.Alpha6)) {
            SetSelectedItemIndex(5);
        } else if (Input.GetKeyDown(KeyCode.Alpha7)) {
            SetSelectedItemIndex(6);
        } else if (Input.GetKeyDown(KeyCode.Alpha8)) {
            SetSelectedItemIndex(7);
        } else if (Input.GetKeyDown(KeyCode.Alpha9)) {
            SetSelectedItemIndex(8);
        } else if (Input.GetKeyDown(KeyCode.Alpha0)) {
            SetSelectedItemIndex(9);
        } else if (Input.GetKeyDown(KeyCode.Minus)) {
            SetSelectedItemIndex(10);
        } else if (Input.GetKeyDown(KeyCode.Equals)) {
            SetSelectedItemIndex(11);
        }


    }

    private void SetSelectedItemIndex(int newIndex) {
        if (newIndex < 0) {
            newIndex += inventorySlots.Count;
        }
        if (newIndex >= inventorySlots.Count) {
            newIndex -= inventorySlots.Count;
        }
        selectedItemIndex = newIndex;
        UpdateUI();
    }

    private void UpdateUI() {
        itemRectTransform = inventorySlots[selectedItemIndex].gameObject.GetComponent<RectTransform>();
        cursorRectTransform.anchoredPosition = new Vector2(itemRectTransform.localPosition.x, 0f);

        Item weaponItem = inventorySlots[selectedItemIndex].GetItem();
        if (weaponItem && weaponItem.GetComponent<ItemWeapon>() != null) {
            player.SetWeapon(weaponItem.GetComponent<ItemWeapon>());
        } else {
            player.SetWeapon(null);
        }
    }

    public bool CanUseTool() {
        return toolUsageCooldownTimer <= 0f;
    }

    public void SetToolUsageCooldown(float time) {
        if (toolUsageCooldownTimer > time) {
            Debug.LogError("How did this happen?");
            return;
        }

        toolUsageCooldownTimer = time;
    }

    internal void UseTool(Item.Tool toolType) {

        PlantableZone currentPlantableZone = player.GetAvailablePlantableZone();

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











    public InventoryData Save() {
        InventoryData data = new InventoryData();

        foreach (InventorySlot slot in inventorySlots) {
            data.inventorySlots.Add(slot.Save());
        }
        data.selectedItemIndex = selectedItemIndex;
        data.waterLevel = waterLevel;

        return data;
    }

    public void Load(InventoryData data) { 

        for (int i = 0; i < data.inventorySlots.Count; i++) {
            inventorySlots[i].Load(data.inventorySlots[i]);
        }
        SetSelectedItemIndex(data.selectedItemIndex);
        waterLevel = data.waterLevel;
    }
}

[Serializable]
public class InventoryData {
    public List<InventorySlotData> inventorySlots = new List<InventorySlotData>();
    public int selectedItemIndex;
    public int waterLevel;
}
