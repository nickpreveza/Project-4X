using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitData
{
    public int healthPoints;
    public int attackPoints;
    public int defensePoints;
    public int movementPoints;
    

    public UnitData(UnitObject unit)
    {
        healthPoints = unit.healthPoints;
        attackPoints = unit.attackPoints;
        defensePoints = unit.defensePoints;
        movementPoints = unit.defensePoints;
    }
}
