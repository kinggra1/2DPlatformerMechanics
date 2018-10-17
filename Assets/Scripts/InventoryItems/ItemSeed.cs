using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSeed : Item {

    public Item.Seed seedType;

    override public bool CanUse() {
        return false;
    }

    public override ItemType GetItemType() {
        return ItemType.Seed;
    }

    // Seeds can't be used. I guess. Idk. (InventorySlot handles consumable count)
    // TODO: Come up with a better design for mix of consumables/tools
    override public void Use() {
        throw new System.NotImplementedException("Can't use seeds.");
    }

    public override ItemData Save() {
        ItemData data = new ItemData();

        data.type = GetItemType();
        data.seed = seedType;

        return data;
    }
}
