using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDirt : Item {

    override public bool CanUse() {
        return false;
    }

    // Seeds can't be used. I guess. Idk. (InventorySlot handles consumable count)
    // TODO: Come up with a better design for mix of consumables/tools
    override public void Use() {
        throw new System.NotImplementedException("Can't use seeds.");
    }
}

