using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemWeapon : Item {

    public Weapon weaponType = Weapon.Sword;
    public Tool toolType = Tool.None;

    public float usageCooldown = 0.4f;

    private PlayerController player;

    override public bool CanUse() {
        return InventorySystem.GetInstance().CanUseTool();
    }

    public override ItemType GetItemType() {
        return ItemType.Weapon;
    }

    public Weapon GetWeaponType() {
        return weaponType;
    }

    public Tool GetToolType() {
        return toolType;
    }

    override public void Use() {
        InventorySystem.GetInstance().SetToolUsageCooldown(usageCooldown);
        player = PlayerController.GetInstance();
        player.UseWeapon(weaponType);
        player.UseTool(toolType);
    }

    override public void UseSpecial() {
        InventorySystem.GetInstance().SetToolUsageCooldown(usageCooldown);
        player = PlayerController.GetInstance();
        player.UseSpecial(Weapon.Whip); // TODO: Change this once done testing whip special functionality.
    }



    public override ItemData Save() {
        ItemData data = new ItemData();

        data.type = GetItemType();
        data.weapon = weaponType;
        data.tool = toolType;

        return data;
    }
}
