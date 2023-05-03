using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;
using System.Linq;

public class WorldUnit : MonoBehaviour
{

    public Unit data;
    Wiggler wiggler;

    public WorldHex parentHex;

    Vector3 oldPosition;
    Vector3 newPosition;
    Vector3 currentVelocity;
    float smoothTime = 0.5f;
    bool shouldMove;

    Queue<WorldHex> hexPath;

    void Start()
    {
        wiggler = GetComponent<Wiggler>();

        oldPosition = newPosition = this.transform.position;
        SI_EventManager.Instance.onTurnEnded += OnTurnEnded;
    }

    public void SpawnSetup(WorldHex newHex, int playerIndex)
    {
        data.associatedPlayerIndex = playerIndex;

        data.c = newHex.hexData.C;
        data.r = newHex.hexData.R;
        parentHex = newHex;
        parentHex.UnitIn(this);

        SetInteractable(true, true);
    }

    public void SetInteractable(bool resetWalk, bool resetAttack)
    {
        data.isInteractable = true;

        if (resetWalk)
        {
            data.hasMoved = false;
        }

        if (resetAttack)
        {
            data.hasAttacked = false;
        }
        //TODO: move these to a different function to have better control over visuals
        this.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitActive;
        this.GetComponent<MeshRenderer>().material.color = GameManager.Instance.GetPlayerColor(data.associatedPlayerIndex);
    }

    public void AutomoveRandomly()
    {
        List<WorldHex> hexesInRadius = MapManager.Instance.GetHexesListWithinRadius(parentHex.hexData, data.range);

        if (hexesInRadius.Contains(parentHex))
        {
            hexesInRadius.Remove(parentHex);
        }

        WorldHex selecedHex = hexesInRadius[Random.Range(0, hexesInRadius.Count)];
        Move(selecedHex, true);
    }

    public void SetUninteractable()
    {
        data.hasMoved = true;
        data.hasAttacked = true;
        data.isInteractable = false;

        //TODO: Set to a transparent shader
        this.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitUsed;
        this.GetComponent<MeshRenderer>().material.color = GameManager.Instance.GetPlayerColor(data.associatedPlayerIndex);
    }
    public void OnTurnEnded(int playerIndex)
    {
        //if this is the end of this players turn, reset the checks. 
        if (data.associatedPlayerIndex == playerIndex)
        {
            data.ActionReset();
        }
    }

    public void OnTurnStarted(int playerIndex)
    {
        if (data.associatedPlayerIndex == playerIndex)
        {
            SetInteractable(true, true);
        }
    }
    public void SetHexPath(WorldHex[] newHexPath)
    {
        hexPath = new Queue<WorldHex>(newHexPath);
    }

    public void DoQueuedTurn()
    {
        if (hexPath == null || hexPath.Count == 0)
        {
            return;
        }

        WorldHex newHex = hexPath.Dequeue();

        Move(newHex);
    }

    public int MovementCostOfHex(WorldHex hex)
    {
        //TODO: Override movement cost based on unit abilities or stats 
        return hex.hexData.moveCost;
    }

    public float AggregateTurnsToEnterHex(WorldHex hex, float turnsToData)
    {
        //return the number of turn required to reach that hex. 
        //if a cost is greater, autocomplete turns off and user has to manually decide. 
        float baseTurnsToEnterHex = MovementCostOfHex(hex) / data.movePoints;
        float turnsRemaining = data.movePointsRemaining / data.movePoints;
        return 0f;
    }

    public void Move(WorldHex newHex, bool followCamera = false)
    {
        data.c = newHex.hexData.C;
        data.r = newHex.hexData.R;

        oldPosition = parentHex.hexData.PositionFromCamera();

        parentHex.UnitOut(this);

        parentHex = newHex;
        parentHex.UnitIn(this);
       
        transform.SetParent(parentHex.unitParent);

        data.hasMoved = true;

        newPosition = parentHex.hexData.PositionFromCamera();

        if (Vector3.Distance(oldPosition, newPosition) > 2)
        {
            //Skip animation
            this.transform.position = newPosition;
            Debug.Log("Animation skipped");
        }
        else
        {
            //Do animated move
            shouldMove = true;
        }
       
        SetUninteractable();

        if (followCamera)
        {
            SI_CameraController.Instance.PanToHex(newHex);
        }

        GameManager.Instance.sessionPlayers[data.associatedPlayerIndex].lastMovedUnit = this;
        //wiggler?.AnimatedMove(newPosition);

        //check if attack possibled
    }

    private void Update()
    {
        if (shouldMove)
        {
            this.transform.position = Vector3.SmoothDamp(this.transform.position, newPosition, ref currentVelocity, smoothTime);
            
            if (Vector3.Distance(this.transform.position, newPosition) < 0.1)
            {
                shouldMove = false;
                this.transform.position = newPosition;
                currentVelocity = Vector3.zero;
            }
        }
    }

    public void Select()
    {
        wiggler?.Wiggle();
        if (GameManager.Instance.activePlayer.index == data.associatedPlayerIndex)
        {
            if (data.isInteractable)
            {
                UnitManager.Instance.SelectUnit(this);
            }
            else
            {
                Debug.Log("This Unit is not interactable");
            }
          
        }
        else
        {
            //TODO: Show information about the unit in the UI
            Debug.Log("This unit belongs to a different player");
        }
       
    }

    public void Deselect()
    {

    }
}
