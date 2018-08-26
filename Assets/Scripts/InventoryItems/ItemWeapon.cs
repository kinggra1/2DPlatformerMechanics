using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeapon : Item {

    public Weapon weaponType = Weapon.Sword;
    public Tool toolType = Tool.None;

    override public bool CanUse() {
        return true;
    }

    override public void Use() {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        player.UseWeapon(weaponType);

        if (toolType != Tool.None) {
            InventorySystem inventory = InventorySystem.GetInstance();
            inventory.UseTool(toolType);
        }
    }
}
