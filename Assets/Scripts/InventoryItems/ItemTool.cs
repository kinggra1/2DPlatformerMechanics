using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTool : Item {

    public Tool toolType = Tool.Axe;

    override public bool CanUse() {
        return true;
    }


    override public void Use() {
        InventorySystem inventory = InventorySystem.GetInstance();
        if (inventory != null) {
            inventory.UseTool(toolType);
        } else {
            Debug.LogError("Unable to find Inventory.");
        }
        
    }
}
