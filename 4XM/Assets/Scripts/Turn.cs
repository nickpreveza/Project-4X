using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn
{
    public List<TurnAction> actions;
}
public class TurnAction 
{
    WorldUnit unit;
    WorldHex startHex;
    WorldHex endHex;
}
