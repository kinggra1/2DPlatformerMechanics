using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Item : MonoBehaviour {

    public bool consumable;
    public Sprite menuSprite;
    public GameObject inGamePrefab;

    public enum ItemType { Seed, Weapon, Resource }

    public enum Seed { PlatformPlant, DewdropPlant, FruitPlantOrange }
    public enum Tool { None, Axe, Shovel }
    public enum Weapon { None, Axe, Shovel, Sword, Whip }
    public enum Resource { Dirt }
    public enum ElementCrystal { None, Fire, Electric }

    public bool IsConsumable() {
        return consumable;
    }

    public GameObject GetGamePrefab() {
        return inGamePrefab;
    }

    public Sprite GetMenuSprite() {
        return menuSprite;
    }

    public abstract ItemType GetItemType();
    public abstract bool CanUse();
    public abstract void Use();

    // Items do not have special usage unless otherwise specified.
     public virtual void UseSpecial() {
        // TODO: Play "no special use" sound or something to communicate.
        return;
    }

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






    public abstract ItemData Save();
}

[Serializable]
public class ItemData {
    public Item.ItemType type;
    public Item.Seed seed;
    public Item.Weapon weapon;
    public Item.Tool tool;
    public Item.Resource resource;
    public Item.ElementCrystal crystal;
}
