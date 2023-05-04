using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    public int c;
    public int r;

    //stats
    public string unitName = "defaultUnit";
    public int cost;
    public int health = 10;
    public int attack = 5;
    public int defense = 1;
    public int range = 1;
    public int actionPoints = 1;
    public int attackCharges = 1;

    //gameplay affected
    public int currentHealth;
    public int currentAttack;
    public int currentDefense;

    //turn resets
    public int currentTurnActionPoints = 1; //related to button actions and movement 
    public int currentTurnAttackCharges = 1; //related only to attacks
    public int enemiesInRange = 0;
    public bool hasNoValidHexesInRange;
    //Abilties
    public bool canAttackAfterMove;
    public bool canMoveAfterAttack;
    public bool setAPToZeroAfterWalk;

    public bool canMove;
    public bool canAttack;

    public bool isInteractable;
    public bool hasMoved;
    public bool hasAttacked;

    public int associatedPlayerIndex = -1;

    public int movePoints = 1;
    public int movePointsRemaining = 1;

    public void SetupData()
    {
        currentHealth = health;
        currentAttack = attack;
        currentDefense = defense;
    }
    public void ValidateRemainigUnitActions(bool parentHexHasUnitActions)
    {
        if (currentTurnActionPoints > 0)
        {
            if (hasNoValidHexesInRange)
            {
                canMove = false;
            }
            else
            {
                canMove = true;
            }

            if (parentHexHasUnitActions)
            {
                canMove = true;
            }
           
        }
        else
        {
            canMove = false;
        }

        if (currentTurnAttackCharges > 0)
        {
            if (enemiesInRange > 0)
            {
                canAttack = true;
            }
            else
            {
                canAttack = false;
            }
            
        }
        else
        {
            canAttack = false;
        }
       
        if (!canAttackAfterMove && hasMoved)
        {
            currentTurnAttackCharges = 0;
            canAttack = false;
        }

        if (!canMoveAfterAttack && hasAttacked)
        {
            canMove = false;
        }


        if (!canAttack && !canMove && currentTurnActionPoints <= 0)
        {
            isInteractable = false;

        }
        else
        {
            isInteractable = true;
        }


    }
}
