using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;
using System.Linq;

public class WorldUnit : MonoBehaviour
{
    public UnitData unitReference;
    Wiggler wiggler;

    public WorldHex parentHex;

    Vector3 oldPosition;
    Vector3 newPosition;
    Vector3 currentVelocity;
    float smoothTime = 0.5f;
    bool shouldMove;

    Queue<WorldHex> hexPath;

    public int c;
    public int r;

    public UnitType type;
    public Civilizations civilization;
    public int playerOwnerIndex = -1;

    //not default, look at unitstruct
    public int currentHealth;
    public int currentAttack;
    public int currentDefense;
    public int currentLevel;

    public int currentMovePoints = 1;
    public int currentAttackCharges = 1;

    public bool hasMoved;
    public bool hasAttacked;

    public bool buttonActionPossible;
    public bool noWalkHexInRange;
    public bool noAttackHexInRange;

    public bool isInteractable;

    public Color civColor;

    bool attackIsRanged;
    UnitView unitView;
    GameObject visualUnit;
    public bool BelongsToActivePlayer
    {
        get
        {
            return (playerOwnerIndex == GameManager.Instance.activePlayerIndex);
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
        if (playerOwnerIndex == playerIndex)
        {
            ResetActions();
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

    public void SpawnSetup(WorldHex newHex, int playerIndex, UnitData referenceData, bool exhaustOnSpawn)
    {
        unitReference = referenceData;
        playerOwnerIndex = playerIndex;

        c = newHex.hexData.C;
        r = newHex.hexData.R;

        parentHex = newHex;
        parentHex.UnitIn(this);

        currentHealth = unitReference.health;
        currentAttack = unitReference.attack;
        currentDefense = unitReference.defense;

        if (unitReference.attackRange > 1)
        {
            attackIsRanged = true;
        }

        if (exhaustOnSpawn)
        {
            ExhaustActions();
        }
        else
        {
            ResetActions();
        }

        unitView = transform.GetChild(0).GetComponent<UnitView>();
        unitView.SetData(this);

        if (visualUnit == null)
        {
            Instantiate(referenceData.GetPrefab(), this.transform);
        }
        else
        {
            Destroy(visualUnit);
            Instantiate(referenceData.GetPrefab(), this.transform);
        }
    }

    void ExhaustActions()
    {
        currentAttackCharges = 0;
        currentMovePoints = 0;
        hasMoved = true;
        hasAttacked = true;

        ValidateRemainigActions();
    }

    void ResetActions()
    {
        hasMoved = false;
        hasAttacked = false;
        currentAttackCharges = unitReference.attackCharges;
        currentMovePoints = unitReference.moveCharges;
        noWalkHexInRange = false;
        noAttackHexInRange = false;
        buttonActionPossible = true;
        ValidateRemainigActions();
    }

    public void CityCaptureAction()
    {
        if (!isInteractable)
        {
            Debug.LogError("Unit was not interactable but tried to capture a city");
            return;
        }

        //more checks here to be double sure;

        GameManager.Instance.activePlayer.AddCity(parentHex);
        ExhaustActions();
        OnActionEnded();
    }

    void OnActionEnded() //not really an event, but treated as one
    {
        UnitManager.Instance.SelectUnit(this);
        UIManager.Instance.ShowHexView(this.parentHex, this);
    }

    public void ValidateRemainigActions()
    {
        
        if (hasMoved)
        {
            if (!unitReference.canAttackAfterMove)
            {
                currentAttackCharges = 0;
            }
        }

        if (hasAttacked)
        {
            if (!unitReference.canMoveAfterAttack)
            {
                currentMovePoints = 0;
            }
        }

        bool interactabilityCheckForWalk = false;
        bool interactabilityCheckForAttack = false;

        if (currentMovePoints > 0)
        {
            if (!noWalkHexInRange)
            {
                interactabilityCheckForWalk = true;
            }
        }

        if (currentAttackCharges > 0)
        {
            if (!noAttackHexInRange)
            {
                interactabilityCheckForAttack = true;
            }
        }

        isInteractable = false;

        if (interactabilityCheckForAttack || interactabilityCheckForWalk)
        {
            isInteractable = true;
        }

        if (hasMoved || hasAttacked)
        {
            buttonActionPossible = false;
        }

        VisualUpdate();
    }

    public void VisualUpdate()
    {
        if (isInteractable)
        {
            if (visualUnit != null)
            {
                visualUnit.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitActive;
                visualUnit.GetComponent<MeshRenderer>().material.color = civColor;
            }
          
        }
        else
        {
            if (visualUnit != null)
            {
                visualUnit.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitUsed;
                visualUnit.GetComponent<MeshRenderer>().material.color = civColor;
            }
           
        }

        unitView?.UpdateData();
    }

    public void AutomoveRandomly()
    {
        List<WorldHex> hexesInRadius = MapManager.Instance.GetHexesListWithinRadius(parentHex.hexData, unitReference.walkRange);
        
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

        //Move(newHex);
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
        float baseTurnsToEnterHex = MovementCostOfHex(hex) / unitReference.moveCharges;
        float turnsRemaining = currentMovePoints / unitReference.moveCharges;
        return 0f;
    }

    public bool AttemptToKill(int value)
    {
        currentHealth -= value;
        unitView.UpdateData();
        if (currentHealth <= 0)
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
        currentHealth = Mathf.Clamp(value, 0, unitReference.health);
    }
    public void Attack(WorldHex enemyHex)
    {
        WorldUnit enemyUnit = enemyHex.associatedUnit;

        currentAttackCharges--;
        hasAttacked = true;

        if (enemyUnit.AttemptToKill(unitReference.attack))
        {
            enemyUnit.Death();

            if (!attackIsRanged)
            {
                Move(enemyHex, true);
            }
           
        }
        else
        {
            if (AttemptToKill(enemyUnit.unitReference.attack))
            {
                Death();
                return;
            }

            UnitManager.Instance.SelectUnit(this);
        }
    }

    void Death()
    {
        Deselect();
        GameManager.Instance.sessionPlayers[playerOwnerIndex].playerUnits.Remove(this);
        //Do some cool UI stuff
        //Maybe particles
        //def sound
        parentHex.UnitOut(this);
        Destroy(this.gameObject);
    }

    public void Move(WorldHex newHex, bool followCamera = false, bool afterAttack = false)
    {
        Deselect();

        c = newHex.hexData.C;
        r = newHex.hexData.R;

        oldPosition = parentHex.hexData.PositionFromCamera();

        parentHex.UnitOut(this);

        parentHex = newHex;
        parentHex.UnitIn(this);
       
        transform.SetParent(parentHex.unitParent);

        if (!afterAttack)
        {
            hasMoved = true;
            currentMovePoints--;
        }

        newPosition = parentHex.hexData.PositionFromCamera();
        shouldMove = true;
        /*
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
        }*/
       
        if (followCamera)
        {
            SI_CameraController.Instance.PanToHex(newHex);
        }

        GameManager.Instance.activePlayer.lastMovedUnit = this;
        //ValidateRemainigActions();
        //wiggler?.AnimatedMove(newPosition);

        //check if attack possibled

       
        UnitManager.Instance.SelectUnit(this);
    }

    


    public void Select()
    {
        wiggler?.Wiggle();
        parentHex.ShowHighlight(false);
        if (GameManager.Instance.activePlayer.index == playerOwnerIndex)
        {
            //ValidateRemainigActions();

            if (isInteractable)
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
        parentHex.HideHighlight();
    }
}
