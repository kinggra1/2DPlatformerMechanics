using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSeed : MonoBehaviour, Item {

    public Sprite menuSprite;
    public GameObject plantablePrefab;

    GameObject Item.GetGamePrefab() {
        return plantablePrefab;
    }

    Sprite Item.GetMenuSprite() {
        return menuSprite;
    }

    bool Item.CanUse() {
        return false;
    }

    // Seeds can't be used. I guess. Idk. (InventorySlot handles consumable count)
    // TODO: Come up with a better design for mix of consumables/tools
    void Item.Use() {
        throw new System.NotImplementedException("Can't use seeds.");
    }
}
