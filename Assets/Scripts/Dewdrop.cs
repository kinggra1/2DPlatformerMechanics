using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dewdrop : Collectible {

    override public void Collect() {
        InventorySystem.GetInstance().ChangeWaterLevel(1);
        base.Collect();
    }
}
