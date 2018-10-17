using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour {

    Item item = null;
    int count = 0;
    bool consumable = false;

    public Text consumableCountText;
    public Image itemImage;
    public Image noItemOverlay;

    private InventorySystem inventory;

    private void Start() {
        inventory = InventorySystem.GetInstance();

        UpdateUI();
    }

    public bool IsEmpty() {
        return item == null;
    }

    public bool IsConsumable() {
        return item != null && consumable;
    }

    public void IncrementCount(int delta) {
        if (consumable) {
            count += delta;
            UpdateUI();
        } else {
            Debug.LogError("Trying to increment count of non-consumable.");
        }
    }

    public Item GetItem() {
        return item;
    }

    public GameObject GetGamePrefab() {
        return item.GetGamePrefab();
    }

    public Sprite GetMenuSprite() {
        return item.GetMenuSprite();
    }

    public void Assign(Item item, int count = 0) {
        if (item != null) {
            // We're replacing something, maybe do something in relation to that?
        }

        this.item = item;
        this.consumable = item.consumable;
        this.count = count;

        UpdateUI();
    }

    public void Use() {
        if (item.CanUse()) {
            item.Use();
        }

        if (consumable) {
            count--;
            if (count <= 0) {
                ClearSlot();
            }
            // Decrement comsumable UI as well
            UpdateUI();
        }
    }

    private void UpdateUI() {
        if (item != null) {
            this.itemImage.gameObject.SetActive(true);
            this.itemImage.sprite = item.GetMenuSprite();
            this.noItemOverlay.gameObject.SetActive(false);
            if (consumable) {
                this.consumableCountText.gameObject.SetActive(true);
                consumableCountText.text = count.ToString();
            } else {
                this.consumableCountText.gameObject.SetActive(false);
            }
        } else {
            this.itemImage.gameObject.SetActive(false);
            this.noItemOverlay.gameObject.SetActive(true);
            this.consumableCountText.gameObject.SetActive(false);
        }
    }

    public void ClearSlot() {
        item = null;
        count = 0;
        consumable = false;
        UpdateUI();
    }










    public InventorySlotData Save() {
        InventorySlotData data = new InventorySlotData();

        if (item) {
            data.item = item.Save();
            data.count = count;
        } else {
            data.item = null;
        }

        return data;
    }

    public void Load(InventorySlotData data) {

        // If this slot had no item in it when saved, clear it when loading.
        if (data.item == null) {
            ClearSlot();
            UpdateUI();
            return;
        }

        // Load the associated item
        Item item = null;
        switch (data.item.type) {
            case Item.ItemType.Seed:
                item = inventory.seedItemMap[data.item.seed];
                break;
            case Item.ItemType.Weapon:
                item = inventory.weaponItemMap[data.item.weapon];
                break;
            case Item.ItemType.Resource:
                item = inventory.resourceItemMap[data.item.resource];
                break;
        }

        this.count = data.count;

        Assign(item, data.count);
    }
}

[Serializable]
public class InventorySlotData {
    public ItemData item;
    public int count = 0;
}
