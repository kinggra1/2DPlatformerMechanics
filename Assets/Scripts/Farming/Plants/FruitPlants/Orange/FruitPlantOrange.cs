using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitPlantOrange : FruitPlant {
    public override void Chop() {
        // If this is the largest we can get (i.e. ornages are ripe)
        if (phaseIndex == growthPhases.Length - 1) {
            // Harvest all harvestables (base.Chop())
            base.Chop();

            // Then revert to the phase where there is no fruit yet
            ChangePhaseIndex(-1);
        }
    }
}
