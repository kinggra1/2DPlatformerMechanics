﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeapon : Item {

    public Weapon weaponType = Weapon.Sword;
    public Tool toolType = Tool.None;

    public float usageCooldown = 0.4f;

    override public bool CanUse() {
        return InventorySystem.GetInstance().CanUseTool();
    }

    public override ItemType GetItemType() {
        return ItemType.Weapon;
    }

    override public void Use() {
        InventorySystem.GetInstance().SetToolUsageCooldown(usageCooldown);

        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<PlayerController>();
        player.UseWeapon(weaponType);

        if (toolType != Tool.None) {
            InventorySystem inventory = InventorySystem.GetInstance();
            inventory.UseTool(toolType);
        }
    }




    public override ItemData Save() {
        ItemData data = new ItemData();

        data.type = GetItemType();
        data.weapon = weaponType;
        data.tool = toolType;

        return data;
    }
}
