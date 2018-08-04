using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Item {

    bool CanUse();
    void Use();
    GameObject GetGamePrefab();
    Sprite GetMenuSprite();
}
