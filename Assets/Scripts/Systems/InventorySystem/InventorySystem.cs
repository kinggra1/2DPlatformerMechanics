using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour {

    private static InventorySystem instance = null;

    public GameObject itemSlotParent;
    public GameObject cursorImage;

    public Dictionary<Item.Seed, Item> seedItemMap = new Dictionary<Item.Seed, Item>();
    public Dictionary<Item.Weapon, Item> weaponItemMap = new Dictionary<Item.Weapon, Item>();
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

        itemSlots[0].Assign(weaponItemMap[Item.Weapon.Axe]);
        itemSlots[1].Assign(weaponItemMap[Item.Weapon.Shovel]);
        itemSlots[2].Assign(weaponItemMap[Item.Weapon.Sword]);

        itemSlots[3].Assign(seedItemMap[Item.Seed.PlatformPlant], 5);
        itemSlots[4].Assign(seedItemMap[Item.Seed.DewdropPlant], 1);
        itemSlots[5].Assign(seedItemMap[Item.Seed.FruitPlantOrange], 3);

        cursorRectTransform = cursorImage.GetComponent<RectTransform>();

        UpdateUI();
    }

    public void LoadItemsFromResources() {
        GameObject platformPlantSeedItem = (GameObject)Resources.Load("PlantPrefabs/Inventory/ItemSeedPlatformPlant", typeof(GameObject));
        GameObject dewdropPlantSeedItem = (GameObject)Resources.Load("PlantPrefabs/Inventory/ItemSeedDewdropPlant", typeof(GameObject));
        GameObject fruitOrangePlantSeedItem = (GameObject)Resources.Load("PlantPrefabs/Inventory/ItemSeedFruitPlantOrange", typeof(GameObject));
        seedItemMap.Add(Item.Seed.PlatformPlant, platformPlantSeedItem.GetComponent<Item>());
        seedItemMap.Add(Item.Seed.DewdropPlant, dewdropPlantSeedItem.GetComponent<Item>());
        seedItemMap.Add(Item.Seed.FruitPlantOrange, fruitOrangePlantSeedItem.GetComponent<Item>());

        GameObject axeItem = (GameObject)Resources.Load("ToolPrefabs/Inventory/ItemToolAxe", typeof(GameObject));
        GameObject shovelItem = (GameObject)Resources.Load("ToolPrefabs/Inventory/ItemToolShovel", typeof(GameObject));
        GameObject swordItem = (GameObject)Resources.Load("WeaponPrefabs/Inventory/ItemWeaponSword", typeof(GameObject));
        weaponItemMap.Add(Item.Weapon.Axe, axeItem.GetComponent<Item>());
        weaponItemMap.Add(Item.Weapon.Shovel, shovelItem.GetComponent<Item>());
        weaponItemMap.Add(Item.Weapon.Sword, swordItem.GetComponent<Item>());

        GameObject dirtItem = (GameObject)Resources.Load("ResourcePrefabs/ItemResourceDirt", typeof(GameObject));
        resourceItemMap.Add(Item.Resource.Dirt, dirtItem.GetComponent<Item>());
    }

    public bool WaterLevelFull() {
        return waterLevel >= maxWaterLevel;
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

    public bool CanPickupItem(Item item)
    {
        // Check to see if this item can be stacked on another consumable
        if (item.IsConsumable()) {
            foreach (InventorySlot slot in itemSlots) {
                if (!slot.IsEmpty() && slot.IsConsumable() && slot.GetItem().Equals(item)) {
                    // TODO: If we add resource stacking limits for consumables, this is where to check those
                    return true;
                }
            }
        }

        // Check to see if there are any empty slots
        foreach (InventorySlot slot in itemSlots) {
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
            foreach (InventorySlot slot in itemSlots) {
                if (!slot.IsEmpty() && slot.IsConsumable() && slot.GetItem().Equals(item)) {
                    // TODO: If we add resource stacking limits for consumables, this is where to check those
                    slot.IncrementCount(1);
                    return;
                }
            }
        }

        // If all of the above fails, put it in the first free inventory slot
        foreach (InventorySlot slot in itemSlots) {
            if (slot.IsEmpty()) {
                slot.Assign(item, 1);
                return;
            }
        }

        // Uhoh, there are no remaining open inventory slots, and this will involve rewriting
        // a number of systems, particularly sending the WaterSprite out ot go pick up Collectables 
        Debug.LogError("We shouldn't have tried picking this up. Oops. Fix your code.");
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

        bool useItemPressed = Input.GetMouseButtonDown(0);
        bool waterButtonPressed = Input.GetMouseButtonDown(1);

        int indexChange = Mathf.RoundToInt(10f*Input.GetAxis("Mouse ScrollWheel"));
        
        if (indexChange != 0) {
            SetSelectedItemIndex(selectedItemIndex += indexChange);
        }

        if (useItemPressed) {
            InventorySlot currentItem = itemSlots[selectedItemIndex];

            // Right now all we can do is use items, so nothing left to do.
            if (currentItem.IsEmpty()) {
                return;
            }

            // Check to see if we have a variety of things and if we can use them
            Growable  plantableSeed = currentItem.GetGamePrefab().GetComponent<Growable >();
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
            newIndex += itemSlots.Count;
        }
        if (newIndex >= itemSlots.Count) {
            newIndex -= itemSlots.Count;
        }
        selectedItemIndex = newIndex;
        UpdateUI();
    }

    private void UpdateUI() {
        itemRectTransform = itemSlots[selectedItemIndex].gameObject.GetComponent<RectTransform>();
        cursorRectTransform.anchoredPosition = new Vector2(itemRectTransform.localPosition.x, 0f);

        Item weaponItem = itemSlots[selectedItemIndex].GetItem();
        if (weaponItem && weaponItem.GetComponent<ItemWeapon>() != null) {
            player.SetWeaponObject(weaponItem.inGamePrefab);
        } else {
            player.SetWeaponObject(null);
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
