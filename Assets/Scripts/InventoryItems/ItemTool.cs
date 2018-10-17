using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTool : Item {

    public Tool toolType = Tool.Axe;

    override public bool CanUse() {
        return true;
    }

    public override ItemType GetItemType() {
        return ItemType.Weapon;
    }

    override public void Use() {
        InventorySystem inventory = InventorySystem.GetInstance();
        if (inventory != null) {
            inventory.UseTool(toolType);
        } else {
            Debug.LogError("Unable to find Inventory.");
        }
        
    }

    public override ItemData Save() {
        ItemData data = new ItemData();

        data.type = GetItemType();

        return data;
    }
}
