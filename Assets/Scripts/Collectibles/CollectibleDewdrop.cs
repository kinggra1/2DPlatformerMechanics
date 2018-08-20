using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleDewdrop : Collectible {

    private InventorySystem inventorySystem;

    private void Start() {
        inventorySystem = InventorySystem.GetInstance();
    }

    public override bool CanCollect() {
        return !inventorySystem.WaterLevelFull();
    }

    protected override void Collect() {
        InventorySystem.GetInstance().ChangeWaterLevel(1);
        base.Collect();
    }
}
