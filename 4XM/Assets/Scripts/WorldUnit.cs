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
    public UnitData boatReference;
    public Civilizations civilization;
    public int playerOwnerIndex = -1;

    //not default, look at unitstruct
    public int localHealth;
    public int currentAttack;
    public int currentDefense;
    public int currentLevel;

    public int currentWalkRange;
    public int currentAttackRange;

    public int currentMovePoints = 1;
    public int currentAttackCharges = 1;

    public bool hasMoved;
    public bool hasAttacked;

    public bool buttonActionPossible;
    public bool noWalkHexInRange;
    public bool noAttackHexInRange;

    public bool isInteractable;

    public Color civColor;
    Color inactiveColor;

    bool attackIsRanged;
    UnitView unitView;
    GameObject visualUnit;

    [SerializeField] Direction unitDir = Direction.RightDown;
    bool shouldRotate;
    Vector3 targetRotation;
    Animator visualAnim;

    Transform particleLayer;
    Transform unitVisualLayer;
    Transform boatsLayer;
    GameObject activeParticle;
    GameObject currentBoat;
    public bool isBoat;
    public bool isShip;

    public bool shouldHealThisTurn;

    public bool BelongsToActivePlayer
    {
        get
        {
            return (playerOwnerIndex == GameManager.Instance.activePlayerIndex);
        }
    }

    void Start()
    {
       
        // SI_EventManager.Instance.onUni
    }

    private void OnDestroy()
    {
        SI_EventManager.Instance.onTurnEnded -= OnTurnEnded;
        SI_EventManager.Instance.onTurnStarted -= OnTurnStarted;
    }

    public void SpawnParticle(GameObject particlePrefab, bool overridePriority = false)
    {
        if (activeParticle == null && particlePrefab != null)
        {
            activeParticle = Instantiate(particlePrefab, particleLayer);
            Invoke("DestroyParticle", 1f);
        }
        else if (overridePriority && particlePrefab != null)
        {
            DestroyParticle();
            activeParticle = Instantiate(particlePrefab, particleLayer);
        }
    }

    void DestroyParticle()
    {
        if (activeParticle != null)
        {
            Destroy(activeParticle);
        }
    }

    public void EnableBoat()
    {
        isBoat = true;

        if (currentBoat != null)
        {
            Destroy(currentBoat);
        }

        SpawnParticle(GameManager.Instance.resourceHarvestParticle);
        // I don't like it either don't judge me 
        currentBoat = Instantiate(
            GameManager.Instance.GetCivilizationByType(GameManager.Instance.GetPlayerByIndex(playerOwnerIndex).civilization).boatPrefab,
            boatsLayer);

        boatReference = UnitManager.Instance.GetUnitDataByType(UnitType.Boat, unitReference.civType);

        currentAttack = boatReference.attack;
        currentDefense = boatReference.defense;
        currentWalkRange = boatReference.walkRange;
        currentAttackRange = boatReference.attackRange;


        ExhaustActions();
    }

    //walking should not play 
    //pathfinding = ship 
    //change unit reference in regards to combat 

    public void EnableShip()
    {
        isShip = true;
        visualUnit.SetActive(false);

        if (currentBoat != null)
        {
            Destroy(currentBoat);
        }

        SpawnParticle(GameManager.Instance.resourceHarvestParticle);
        // I don't like it either don't judge me 
        currentBoat = Instantiate(
            GameManager.Instance.GetCivilizationByType(GameManager.Instance.GetPlayerByIndex(playerOwnerIndex).civilization).shipPrefab,
            boatsLayer);

        BoatMaterialHelper helper = currentBoat.GetComponent<BoatMaterialHelper>();
        if (helper != null)
        {
            foreach (GameObject obj in helper.objectsToChangeMaterials)
            {
                obj.GetComponent<MeshRenderer>().materials[0].color = civColor;
            }
        }

        boatReference = UnitManager.Instance.GetUnitDataByType(UnitType.Ship, unitReference.civType);

        currentAttack = boatReference.attack;
        currentDefense = boatReference.defense;
        currentWalkRange = boatReference.walkRange;
        currentAttackRange = boatReference.attackRange;

        ExhaustActions();
    }

    //how we apply player materials in visual update
    //how we do hexfinding
    //change unit reference in regards to combat 

    public void DisableBoats()
    {
        visualUnit.SetActive(true);

        isBoat = false;
        isShip = false;

        currentBoat = null;

        foreach (Transform child in boatsLayer)
        {
            Destroy(child.gameObject);
        }

        currentAttack = unitReference.attack;
        currentDefense = unitReference.defense;
        currentWalkRange = unitReference.walkRange;
        currentAttackRange = unitReference.attackRange;

        if (currentAttackRange > 1)
        {
            attackIsRanged = true;
        }

        ExhaustActions();
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
        if (playerOwnerIndex == playerIndex)
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

            if (isBoat || isShip)
            {
                currentBoat.transform.eulerAngles = targetRotation;
            }

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
        wiggler = GetComponent<Wiggler>();

        oldPosition = newPosition = this.transform.position;
        SI_EventManager.Instance.onTurnEnded += OnTurnEnded;
        SI_EventManager.Instance.onTurnStarted += OnTurnStarted;

        unitView = transform.GetChild(0).GetComponent<UnitView>();
        unitVisualLayer = transform.GetChild(1);
        particleLayer = transform.GetChild(2);
        boatsLayer = transform.GetChild(3);

        unitReference = referenceData;
        playerOwnerIndex = playerIndex;

        c = newHex.hexData.C;
        r = newHex.hexData.R;

        parentHex = newHex;
        parentHex.UnitIn(this);

        localHealth = unitReference.health;
        currentAttack = unitReference.attack;
        currentDefense = unitReference.defense;
        currentWalkRange = unitReference.walkRange;
        currentAttackRange = unitReference.attackRange;

        type = unitReference.type;

        civColor = GameManager.Instance.GetCivilizationColor(playerIndex, CivColorType.unitColor);
        inactiveColor = GameManager.Instance.GetCivilizationColor(playerIndex, CivColorType.unitInactiveColor);

        if (currentAttackRange > 1)
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

        unitView.SetData(this);
        visualUnit = unitVisualLayer.GetChild(0).gameObject;

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

        SpawnParticle(UnitManager.Instance.unitSpawnParticle);
        UpdateVisualsDirection(false);
    }

    void ExhaustActions()
    {
        currentAttackCharges = 0;
        currentMovePoints = 0;
        hasMoved = true;
        hasAttacked = true;

        if (isBoat || isShip)
        {
            ValidateRemainigActions(boatReference);
        }
        else
        {
            ValidateRemainigActions(unitReference);
        }
       
    }

    void ResetActions(bool isEndOfTurn)
    {
        if (isEndOfTurn)
        {
            if (!hasMoved && !hasAttacked)
            {
                shouldHealThisTurn = true;
            }
        }
     

        hasMoved = false;
        hasAttacked = false;

        noWalkHexInRange = false;
        noAttackHexInRange = false;
        buttonActionPossible = true;

        if (!isEndOfTurn || isEndOfTurn && unitReference.healAtTurnEnd) //heal check
        {
            if (shouldHealThisTurn)
            {
                if (isBoat || isShip)
                {
                    Heal(boatReference.heal);
                }
                else
                {
                    Heal(unitReference.heal);
                }

                shouldHealThisTurn = false;
            }
        }


        if (isBoat || isShip)
        {
            currentAttackCharges = boatReference.attackCharges;
            currentMovePoints = boatReference.moveCharges;

            ValidateRemainigActions(boatReference);
        }
        else
        {
            currentAttackCharges = unitReference.attackCharges;
            currentMovePoints = unitReference.moveCharges;

            ValidateRemainigActions(unitReference);
        }

       

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
        visualAnim.SetTrigger("Capture");
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
    
    public void ValidateRemainigActions(UnitData unitDataToVerify)
    {
        if (hasMoved)
        {
            if (!unitDataToVerify.canAttackAfterMove)
            {
                currentAttackCharges = 0;
            }
        }

        if (hasAttacked)
        {
            if (!unitDataToVerify.canMoveAfterAttack)
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
                if (!isShip)
                {
                    SkinnedMeshRenderer renderer = visualAnim.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
                    renderer.materials[0].SetColor("_ColorShift", civColor);
                }
                else
                {
                    BoatMaterialHelper helper = currentBoat.GetComponent<BoatMaterialHelper>();
                    if (helper != null)
                    {
                        foreach(GameObject obj in helper.objectsToChangeMaterials)
                        {
                            obj.GetComponent<MeshRenderer>().materials[0].color = civColor;
                        }
                    }
                }
                
            }
          
        }
        else
        {
            if (visualUnit != null)
            {
                if (!isShip)
                {
                    SkinnedMeshRenderer renderer = visualAnim.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
                    renderer.materials[0].SetColor("_ColorShift", inactiveColor);
                }
                else
                {
                    BoatMaterialHelper helper = currentBoat.GetComponent<BoatMaterialHelper>();
                    if (helper != null)
                    {
                        foreach (GameObject obj in helper.objectsToChangeMaterials)
                        {
                            obj.GetComponent<MeshRenderer>().materials[0].color = inactiveColor;
                        }
                    }
                }
               
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

            if (isBoat || isShip)
            {
                currentBoat.transform.eulerAngles = targetRotation;
            }
        }
    }

    public bool TryToMoveRandomly()
    {
        bool hasValidMove;
        List<WorldHex> hexesInRadius = UnitManager.Instance.GetWalkableHexes(this, 1);

        if (hexesInRadius.Count > 0)
        {
            WorldHex selecedHex = hexesInRadius[Random.Range(0, hexesInRadius.Count)];
            Move(selecedHex, true, false);
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

    public bool ReceiveDamage(int value)
    {
        Debug.Log("Health was: " + localHealth);
        Debug.Log("Damage received was: " + (value - currentDefense));

        localHealth = localHealth - (value - currentDefense);

        unitView.UpdateData();
        if (localHealth <= 0)
        {
            return true;
        }
        else
        {
            SpawnParticle(UnitManager.Instance.unitHitParticle, true);
            visualAnim.SetTrigger("Evade");
      
            return false;
        }
    }

    public void Heal(int value)
    {
        SpawnParticle(UnitManager.Instance.unitHealParticle);
        localHealth += value;
        localHealth = Mathf.Clamp(localHealth, 0, unitReference.health);
    }

    public void HealWithDefaultValue()
    {
        Heal(unitReference.heal);
    }

    public void VisualAttack()
    {
        switch (type)
        {
            case UnitType.Defensive:
            case UnitType.Trader:
            case UnitType.Diplomat:
            case UnitType.Melee:
            case UnitType.Boat:
                visualAnim.SetTrigger("AttackSword");
                break;
            case UnitType.Ranged:
                visualAnim.SetTrigger("AttackBow");
                break;
            case UnitType.Cavalry:
                visualAnim.SetTrigger("AttackHorse");
                break;
            case UnitType.Siege:
            case UnitType.Ship:
                visualAnim.SetTrigger("AttackShield");
                break;
        }
    }

    bool isAttacking = false;
    public void Attack(WorldHex enemyHex)
    {
        if (isAttacking)
        {
            Debug.LogWarning("Unit is currently attacking already");
            return;
        }

        isAttacking = true;
        WorldUnit enemyUnit = enemyHex.associatedUnit;

        currentAttackCharges--;
        hasAttacked = true;

        if (!isBoat && !isShip)
        {
            if (unitReference.canMoveAfterAttack)
            {
                currentMovePoints++;
            }
            else
            {
                currentMovePoints = 0;
            }
        }
        else
        {
            if (boatReference.canMoveAfterAttack)
            {
                currentMovePoints++;
            }
            else
            {
                currentMovePoints = 0;
            }
        }
      
        StartCoroutine(FightSequence(enemyHex, enemyUnit));
        
    }

    IEnumerator FightSequence(WorldHex enemyHex, WorldUnit enemyUnit)
    {
        VisualAttack();
        yield return new WaitForSeconds(.7f);

        if (enemyUnit.ReceiveDamage(currentAttack))
        {
            enemyUnit.visualAnim.SetTrigger("Die");
            enemyUnit.SpawnParticle(UnitManager.Instance.unitDeathParticle);

            yield return new WaitForSeconds(1f);
            enemyUnit.Death(true);

            if (!attackIsRanged)
            {
                yield return new WaitForSeconds(0.2f);
                Move(enemyHex, true);
            }

        }
        else
        {
            yield return new WaitForSeconds( .5f);
            enemyUnit.VisualAttack();
            yield return new WaitForSeconds(0.5f);
            if (ReceiveDamage(enemyUnit.unitReference.counterAttack))
            {
                visualAnim.SetTrigger("Die");
                SpawnParticle(UnitManager.Instance.unitDeathParticle);
                yield return new WaitForSeconds(1f);
                Death(true);
                yield break;
            }
            else
            {
                SpawnParticle(UnitManager.Instance.unitHitParticle, true);
                visualAnim.SetTrigger("Evade");
            }

            UnitManager.Instance.SelectUnit(this);
        }

        isAttacking = false;
        UnitManager.Instance.SelectUnit(this);
    }

    public void Death(bool affectStats)
    {
        Deselect();
        GameManager.Instance.sessionPlayers[playerOwnerIndex].playerUnits.Remove(this);
        //Do some cool UI stuff
        //Maybe particles
        //def sound
        parentHex.UnitOut(this, true);
        Destroy(this.gameObject);
    }


    public void Move(WorldHex newHex, bool followCamera = false, bool afterAttack = false)
    {
        visualAnim.SetTrigger("Walk");
        parentHex.SpawnParticle(UnitManager.Instance.unitWalkParticle);
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
            SpawnParticle(UnitManager.Instance.unitSelectParticle);
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
