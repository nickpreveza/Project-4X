using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public Sprite weaponSprite;
    public float bulletSpawnOffset;
    public AttackPattern attackPattern;

    public Stat Damage;
    public Stat CriticalDamage;
    public Stat CriticalChance;
    public Stat FireRate;

    public int ammoPerShot;
    public float burstRate;
    public int burstAmmo;

    public float shotgunSpreadAngle;

    public float projectileLifetime;
    public float projectileSpeed;
}

[System.Serializable]
public class Stat
{
    public float CurrentLevelValue()
    {
        if (statLevel > value.Length - 1)
        {
            return -1;
        }
        else
        {
            return value[statLevel];
        }
    }

    public float NextLevelValue()
    {
        if (statLevel + 1> value.Length - 1)
        {
            return -1;
        }
        else
        {
            return value[statLevel + 1];
        }
    }
    public float LevelValue(int statLevel)
    {

        if (statLevel > value.Length - 1)
        {
            return -1;
        }
        else
        {
            return value[statLevel];
        }
    }

    public float LevelPrice(int statLevel)
    {
        if (statLevel > valueLevelPrice.Length - 1)
        {
            return -1;
        }
        else
        {
            return valueLevelPrice[statLevel];
        }
    }

    public float NextLevelPrice()
    {
        if (statLevel + 1 > valueLevelPrice.Length - 1)
        {
            return -1;
        }
        else
        {
            return valueLevelPrice[statLevel + 1];
        }
    }

    public string statName;
    public int statLevel; 
    public float[] value;
    public int[] valueLevelPrice;
}

public enum AttackPattern
{
    PISTOL,
    SHOTGUN
}
