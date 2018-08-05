﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleSeed : Collectible {

    public GameObject associatedItemObject;
    private Item associatedItem;

    private void Start() {
        associatedItem = associatedItemObject.GetComponent<Item>();
        if (associatedItem == null) {
            Debug.LogError("No Item associated with this CollectibleSeed.");
        }

        // Create the in game image for this icon based on the menu Sprite of the associated item.
        GameObject itemImage = new GameObject("image");
        itemImage.transform.parent = this.transform;
        itemImage.transform.localPosition = Vector3.zero;
        itemImage.transform.localScale = Vector3.one * 0.3f;
        itemImage.AddComponent<SpriteRenderer>().sprite = associatedItem.GetMenuSprite();
    }

    override public void Collect() {
        InventorySystem.GetInstance().PickupItem(associatedItem);
        base.Collect();
    }
}