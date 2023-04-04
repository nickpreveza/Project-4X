using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

[System.Serializable]
public class Player
{
    public PlayerType type;
    public int turnOrder;
    public string name;

    public int score;
    public int turnCount;
    public int stars;


    public List<WorldUnit> playerUnits = new List<WorldUnit>();
    public List<WorldHex> playerCities = new List<WorldHex>();
    public List<WorldHex> playerBorders = new List<WorldHex>();

    List<TurnAction> activeTurnActions = new List<TurnAction>();

    List<Turn> turns = new List<Turn>();

    public void StartTurn()
    {
       
        //GameManager.Instance.SetActivePlayer(this);
    }

    public void EndTurn()
    {
        Turn thisTurn = new Turn();
        thisTurn.actions = activeTurnActions;
        turns.Add(thisTurn);

        activeTurnActions.Clear();

       // GameManager.Instance.EndTurn(this);
    }
}



public enum PlayerType
{
    LOCAL,
    ONLINE,
    AI
}
