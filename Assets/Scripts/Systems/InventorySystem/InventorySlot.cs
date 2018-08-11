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

    private void Awake() {
        //consumableCount = GetComponentInChildren<Text>();
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

    public void Assign(Item item, bool consumable = false, int count = 0) {
        if (item != null) {
            // We're replacing something, maybe do something in relation to that?
        }

        this.item = item;
        this.consumable = consumable;
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
}
