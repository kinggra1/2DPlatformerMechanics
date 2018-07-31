using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlantableZone {

    bool CanPlantSeed();
    void PlantSeed(GameObject seed);
    void Fertalize();
    bool CanBeWatered();
    void Water();
}
