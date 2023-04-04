using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    public int health;
    public int attack;
    public int range;

    public bool movePossible;
    public bool attackPossible;
    public bool canAttackAfterMove;
    public bool canMoveAfterAttack;

}
