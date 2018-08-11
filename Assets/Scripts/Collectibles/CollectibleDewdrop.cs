using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleDewdrop : Collectible {

    override public void Collect() {
        InventorySystem.GetInstance().ChangeWaterLevel(1);
        base.Collect();
    }
}
