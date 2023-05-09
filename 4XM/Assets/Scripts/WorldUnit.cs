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
    Vector3 currentRotationVelocity;
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

    [SerializeField] Direction unitDir = Direction.RightDown;
    bool shouldRotate;
    Vector3 targetRotation;
    Animator visualAnim;

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
        SI_EventManager.Instance.onTurnStarted += OnTurnStarted;
       // SI_EventManager.Instance.onUni
    }

    private void OnDestroy()
    {
        SI_EventManager.Instance.onTurnEnded -= OnTurnEnded;
        SI_EventManager.Instance.onTurnStarted -= OnTurnStarted;
    }


    public void OnTurnEnded(int playerIndex)
    {
        //if this is the end of this players turn, reset the checks. 
        if (playerOwnerIndex == playerIndex)
        {
            ResetActions(true);
        }
    }

    public void OnTurnStarted(int playerIndex)
    {
        if(playerOwnerIndex == playerIndex)
        {
            ResetActions(false);
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
                visualAnim.SetTrigger("Idle");
            }
        }

        if (shouldRotate)
        {
           visualUnit.transform.eulerAngles = Vector3.SmoothDamp(visualUnit.transform.eulerAngles, targetRotation, ref currentRotationVelocity, smoothTime);

            if (Vector3.Distance(visualUnit.transform.rotation.eulerAngles, targetRotation) < 0.1)
            {
                shouldRotate = false;
                visualUnit.transform.eulerAngles = targetRotation;
                currentRotationVelocity = Vector3.zero;
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

        civColor = GameManager.Instance.GetCivilizationColor(playerIndex, CivColorType.unitColor);

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
            ResetActions(false);
        }

        unitView = transform.GetChild(0).GetComponent<UnitView>();
        unitView.SetData(this);

        if (this.transform.childCount > 1)
        {
            visualUnit = transform.GetChild(1).gameObject;
        }

        if (visualUnit == null)
        {
            visualUnit = Instantiate(referenceData.GetPrefab(), this.transform);
            visualAnim = visualUnit.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
            visualAnim.SetTrigger("Idle");

            SkinnedMeshRenderer renderer = visualAnim.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
            renderer.materials[0].SetColor("_ColorShift", civColor);
        }
        else
        {
            Destroy(visualUnit);
            visualUnit = Instantiate(referenceData.GetPrefab(), this.transform);
            visualAnim = visualUnit.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
            visualAnim.SetTrigger("Idle");

            SkinnedMeshRenderer renderer = visualAnim.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
            renderer.materials[0].SetColor("_ColorShift", civColor);
        }

        UpdateVisualsDirection(false);
    }

    void ExhaustActions()
    {
        currentAttackCharges = 0;
        currentMovePoints = 0;
        hasMoved = true;
        hasAttacked = true;

        ValidateRemainigActions();
    }

    void ResetActions(bool isEndOfTurn)
    {
        hasMoved = false;
        hasAttacked = false;
        currentAttackCharges = unitReference.attackCharges;
        currentMovePoints = unitReference.moveCharges;
        noWalkHexInRange = false;
        noAttackHexInRange = false;
        buttonActionPossible = true;

        if (isEndOfTurn)
        {
            if (unitReference.healAtTurnEnd)
            {
                Heal(unitReference.heal);
            }
        }
        else
        {
            if (unitReference.canHeal)
            {
                Heal(unitReference.heal);
            }
        }
      

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

    public void MonumentCapture()
    {
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
                //visualUnit.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitActive;
                //visualUnit.GetComponent<MeshRenderer>().material.color = civColor;
            }
          
        }
        else
        {
            if (visualUnit != null)
            {
                //visualUnit.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitUsed;
                //visualUnit.GetComponent<MeshRenderer>().material.color = civColor;
            }
           
        }

        unitView?.UpdateData();
    }

    public void UpdateDirection(WorldHex origin, WorldHex target, bool animate)
    {
        Direction dir = MapManager.Instance.GetHexDirection(origin, target);

        if (dir != unitDir)
        {
            unitDir = dir;
            UpdateVisualsDirection(animate);
        }
    }

    void UpdateVisualsDirection(bool animate)
    {
        targetRotation = Vector3.zero;

        switch (unitDir)
        {
            case Direction.RightUp:
                targetRotation.y = 45;
                break;
            case Direction.Right:
                targetRotation.y = 90;
                break;
            case Direction.RightDown:
                targetRotation.y = 135;
                break;
            case Direction.LeftDown:
                targetRotation.y = 225;
                break;
            case Direction.Left:
                targetRotation.y = 270;
                break;
            case Direction.LeftUp:
                targetRotation.y = 315;
                break;

        }

        if (animate)
        {
            shouldRotate = true;
        }
        else
        {
            visualUnit.transform.eulerAngles = targetRotation;
        }
    }

    public bool TryToMoveRandomly()
    {
        bool hasValidMove;
        List<WorldHex> hexesInRadius = UnitManager.Instance.GetWalkableHexes(this);
        if (hexesInRadius.Count > 0)
        {
            WorldHex selecedHex = hexesInRadius[Random.Range(0, hexesInRadius.Count)];
            Move(selecedHex, true);
            return true;
        }
        else
        {
            return false;
        }

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

        currentHealth -= value - currentDefense;
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

    public void HealWithDefaultValue()
    {
        Heal(unitReference.heal);
    }
    public void Attack(WorldHex enemyHex)
    {
        WorldUnit enemyUnit = enemyHex.associatedUnit;

        switch (type)
        {
            case UnitType.Defensive:
            case UnitType.Trader:
            case UnitType.Diplomat:
            case UnitType.Melee:
                visualAnim.SetTrigger("AttackSword");
                break;
            case UnitType.Ranged:
                visualAnim.SetTrigger("AttackBow");
                break;
            case UnitType.Cavalry:
                visualAnim.SetTrigger("AttackHorse");
                break;
            case UnitType.Siege:
                visualAnim.SetTrigger("AttackShield");
                break;
        }

        currentAttackCharges--;
        hasAttacked = true;

        if (unitReference.canMoveAfterAttack)
        {
            currentMovePoints++;
        }

        if (enemyUnit.AttemptToKill(unitReference.attack))
        {
            enemyUnit.Death(true);

            if (!attackIsRanged)
            {
                Move(enemyHex, true);
            }
           
        }
        else
        {
            if (AttemptToKill(enemyUnit.unitReference.attack))
            {
                Death(true);
                return;
            }
            visualAnim.SetTrigger("Evade");
            UnitManager.Instance.SelectUnit(this);
        }

        UnitManager.Instance.SelectUnit(this);
    }

    public void Death(bool affectStats)
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
        visualAnim.SetTrigger("Walk");
        Deselect();

        c = newHex.hexData.C;
        r = newHex.hexData.R;

        oldPosition = parentHex.hexData.PositionFromCamera();
        UpdateDirection(parentHex, newHex, true);
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
        //this is visual only
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

        if (parentHex.hexData.hasCity)
        {
            if (parentHex.hexData.playerOwnerIndex != playerOwnerIndex)
            {
                MapManager.Instance.SetHexUnderSiege(parentHex);
            }
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
            UnitManager.Instance.SelectUnit(this);

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
