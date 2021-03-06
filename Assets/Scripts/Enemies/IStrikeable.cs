﻿using UnityEngine;

/* 
 * Anything that can be struck by a weapon. Implements its own reaction to being struck by whatever weapon
 */
internal interface IStrikeable {
    void Strike(Vector2 weaponLocation, ItemWeapon weapon);
}