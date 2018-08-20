using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Gorwables are things that can be watered, can grow, and can be "chopped"
 */
public interface IGrowable {
    
    void Grow();
    bool CanBeWatered();

    void Water();
    bool IsWatered();

    void Chop();
}
