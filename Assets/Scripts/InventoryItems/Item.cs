using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour {

    public bool consumable;
    public Sprite menuSprite;
    public GameObject inGamePrefab;

    public enum Seed { PlatformPlant, DewdropPlant }
    public enum Tool { Axe, Shovel }
    public enum Resource { Dirt }

    public bool IsConsumable() {
        return consumable;
    }

    public GameObject GetGamePrefab() {
        return inGamePrefab;
    }

    public Sprite GetMenuSprite() {
        return menuSprite;
    }

    public abstract bool CanUse();
    public abstract void Use();

    public override bool Equals(object other) {
        if (other == null) {
            return false;
        }

        Item otherItem = other as Item;
        if (otherItem == null) {
            return false;
        }

        return this.inGamePrefab == otherItem.inGamePrefab;
    }

    public override int GetHashCode() {
        return base.GetHashCode();
    }

    public override string ToString() {
        return base.ToString();
    }
}
