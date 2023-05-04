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
    public bool IsInteractable
    {
        get
        {
            return data.isInteractable;
        }
    }

    public bool HasActionsLeft
    {
        get
        {
            return (data.currentTurnActionPoints > 0);
        }
    }

    public bool HasAttacksLeft
    {
        get
        {
            return (data.currentTurnAttackCharges > 0);
        }
    }

    public bool CanMove
    {
        get
        {
            return data.canMove;
        }
    }

    public bool CanAttack
    {
        get
        {
            return data.canAttack;
        }
    }

    public bool BelongsToActivePlayer
    {
        get
        {
            return (data.associatedPlayerIndex == GameManager.Instance.activePlayerIndex);
        }
    }

    void Start()
    {
        wiggler = GetComponent<Wiggler>();

        oldPosition = newPosition = this.transform.position;
        SI_EventManager.Instance.onTurnEnded += OnTurnEnded;
       // SI_EventManager.Instance.onUni
    }

    private void OnDestroy()
    {
        SI_EventManager.Instance.onTurnEnded -= OnTurnEnded;
    }


    public void OnTurnEnded(int playerIndex)
    {
        //if this is the end of this players turn, reset the checks. 
        if (data.associatedPlayerIndex == playerIndex)
        {
            ActionReset();
        }
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

    public void SetHexPath(WorldHex[] newHexPath)
    {
        hexPath = new Queue<WorldHex>(newHexPath);
    }

    public void SpawnSetup(WorldHex newHex, int playerIndex, bool exhaustOnSpawn)
    {
        data.associatedPlayerIndex = playerIndex;

        data.c = newHex.hexData.C;
        data.r = newHex.hexData.R;
        parentHex = newHex;
        parentHex.UnitIn(this);
        data.SetupData();

        if (exhaustOnSpawn)
        {
            data.currentTurnActionPoints = 0;
            data.currentTurnAttackCharges = 0;
            data.hasMoved = true;
            data.hasAttacked = true;
            ValidateRemainigActions();
        }
        else
        {
            ActionReset();
        }

    }

    public void CityCaptureAction()
    {
        data.hasMoved = true;
        data.currentTurnActionPoints = 0;
        data.currentTurnAttackCharges = 0;
        ValidateRemainigActions();
    }

    public void ActionReset()
    {
        data.hasMoved = false;
        data.hasAttacked = false;
        data.hasNoValidHexesInRange = false; //aslo probably update this if any unit dies or something

        data.currentTurnActionPoints = data.actionPoints;
        data.currentTurnAttackCharges = data.attackCharges;

        ValidateRemainigActions();
    }

    public void ValidateRemainigActions()
    {
        data.ValidateRemainigUnitActions(parentHex.HasAvailableUnitActions());
        VisualUpdate();
    }

    public void VisualUpdate()
    {
        if (data.isInteractable)
        {
            this.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitActive;
            this.GetComponent<MeshRenderer>().material.color = GameManager.Instance.GetPlayerColor(data.associatedPlayerIndex);

        }
        else
        {
          
            this.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitUsed;
            this.GetComponent<MeshRenderer>().material.color = GameManager.Instance.GetPlayerColor(data.associatedPlayerIndex);
        }
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

    public bool AttemptToKill(int value)
    {
        data.currentHealth -= value;

        if (data.currentHealth <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Heal(int value)
    {
        data.currentHealth = Mathf.Clamp(value, 0, data.health);
    }
    public void Attack(WorldHex enemyHex)
    {
        WorldUnit enemyUnit = enemyHex.associatedUnit;
        data.attackCharges--;
        data.hasAttacked = true;

        if (enemyUnit.AttemptToKill(data.attack))
        {
            enemyUnit.Death();
            Move(enemyHex, true);
        }
        else
        {
            if (AttemptToKill(enemyUnit.data.attack))
            {
                Death();
                return;
            }

            UnitManager.Instance.SelectUnit(this);
        }
    }

    void Death()
    {
        GameManager.Instance.sessionPlayers[data.associatedPlayerIndex].playerUnits.Remove(this);
        //Do some cool UI stuff
        //Maybe particles
        //def sound
        parentHex.UnitOut(this);
        Destroy(this.gameObject);
    }

    public void Move(WorldHex newHex, bool followCamera = false, bool afterAttack = false)
    {
        data.c = newHex.hexData.C;
        data.r = newHex.hexData.R;

        oldPosition = parentHex.hexData.PositionFromCamera();

        parentHex.UnitOut(this);

        parentHex = newHex;
        parentHex.UnitIn(this);
       
        transform.SetParent(parentHex.unitParent);

        if (!afterAttack)
        {
            data.hasMoved = true;
            data.currentTurnActionPoints--;

            if (data.setAPToZeroAfterWalk)
            {
                data.currentTurnActionPoints = 0;
            }
        }

        newPosition = parentHex.hexData.PositionFromCamera();

        //this si visual only
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
       
        if (followCamera)
        {
            SI_CameraController.Instance.PanToHex(newHex);
        }

        GameManager.Instance.activePlayer.lastMovedUnit = this;
        //wiggler?.AnimatedMove(newPosition);

        //check if attack possibled
        ValidateRemainigActions();

        UnitManager.Instance.SelectUnit(this);
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
