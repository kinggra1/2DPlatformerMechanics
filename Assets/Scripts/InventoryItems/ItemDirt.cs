using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDirt : Item {

    override public bool CanUse() {
        return false;
    }

    public override ItemType GetItemType() {
        return ItemType.Resource;
    }

    // Seeds can't be used. I guess. Idk. (InventorySlot handles consumable count)
    // TODO: Come up with a better design for mix of consumables/tools
    override public void Use() {
        throw new System.NotImplementedException("Can't use dirt.");
    }

    public override ItemData Save() {
        ItemData data = new ItemData();

        data.type = GetItemType();
        data.resource = Item.Resource.Dirt;

        return data;
    }
}

