using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

[System.Serializable]
public class Player
{
    public PlayerType type;
    public int index;
    public string name;

    public int score;
    public int turnCount;
    public int stars;

    public Color playerColor;

    public List<WorldUnit> playerUnitsThatCanBeSpawned = new List<WorldUnit>();

    public List<WorldUnit> playerUnits = new List<WorldUnit>();
    public List<WorldHex> playerCities = new List<WorldHex>();
    public List<WorldHex> playerBorders = new List<WorldHex>();

    List<TurnAction> activeTurnActions = new List<TurnAction>();

    List<Turn> turns = new List<Turn>();

    public void StartTurn()
    {
        List<TurnAction> activeTurnActions = new List<TurnAction>();
        turnCount++;

        //TODO: Calculate Stars
        //TODO: Recalculate Score


        if (type == PlayerType.AI)
        {
            UnitManager.Instance.PlayRandomTurnForAIUnits(this);
        }
    }

  

    public void EndTurn()
    {
        Turn thisTurn = new Turn();
        thisTurn.actions = activeTurnActions;
        turns.Add(thisTurn);

        activeTurnActions.Clear();

       // GameManager.Instance.EndTurn(this);
    }

    public void AddUnit(WorldUnit newUnit)
    {
        playerUnits.Add(newUnit);
    }

    public void AddCity(WorldHex cityHex)
    {
        //TODO: Some security checks to make sure this is the correct tile;
        playerCities.Add(cityHex);
        cityHex.OccupyCityByPlayer(this);
    }
}



public enum PlayerType
{
    LOCAL,
    ONLINE,
    AI
}
