using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    int Health { get; set; }
    void OnDamage(int amount);
    //void KnockBack(float amount);

}

public enum DamageType
{
    PLAYER,
    ENEMIES,
    ALL
}
