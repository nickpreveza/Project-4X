using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    public int c;
    public int r;

    public string unitName = "defaultUnit";
    public int cost;
    public int health = 10;
    public int attack = 5;

    public int range = 1;

    //Abilties
    public bool canAttackAfterMove;
    public bool canMoveAfterAttack;

    public bool isInteractable;
    public bool hasMoved;
    public bool hasAttacked;

    public int associatedPlayerIndex = -1;

    public int movePoints = 1;
    public int movePointsRemaining = 1;

    public void ActionReset()
    {
        hasMoved = false;
        hasAttacked = false;
        isInteractable = true;
    }
}
