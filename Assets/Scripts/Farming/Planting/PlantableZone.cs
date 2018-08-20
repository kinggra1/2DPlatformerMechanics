using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Represents a location where Growable  objects can be planted.
 * 
 * TODO: Consider refactoring from an interface into a class that handles most
 * of the pass-through calls to the Growable . Others can override for special 
 * logic, but I imagine most of the time calling "Water()" will just call water
 * on the associated Growable  object.
 */
public interface IPlantableZone {

    bool IsPlanted();
    void PlantSeed(GameObject seed);

    void Fertalize();


    /*
     * At the moment, everything below here mostly operates as a pass-through call
     * to tell the underlying Growable  to be watered/chopped. PlantableZone is really
     * just a wrapper for a Growable  object anyway. 
     */
    bool CanBeWatered();
    void Water();

    void Chop();
}
