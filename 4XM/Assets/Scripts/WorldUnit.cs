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
    public WorldHex originCity;
    public Vector3 oldPosition;
    public Vector3 newPosition;
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
    public bool interactableButIgnored;

    public Color civColor;
    Color inactiveColor;

    public bool attackIsRanged;
    UnitView unitView;
    GameObject visualUnit;

    [SerializeField] Direction unitDir = Direction.RightDown;
    bool shouldRotate;
    Vector3 targetRotation;
    public Animator visualAnim;

    Transform particleLayer;
    Transform unitVisualLayer;
    Transform boatsLayer;
    GameObject activeParticle;
    GameObject currentBoat;
    public bool isBoat;
    public bool isShip;

    public bool shouldHealThisTurn;
    WorldHex tempPathParent;
    public WorldHex assignedPathTarget;

    public bool HealPossible()
    {
        if (isBoat || isShip)
        {
            return localHealth < boatReference.health;
        }
        else
        {
            return localHealth < unitReference.health;
        }
    }
    public int PathfindingActionPoints //a normal move actions will cost 2.
    {
        get
        {
            return currentWalkRange * 2;
        }
    }

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

        if (currentAttackRange > 1)
        {
            attackIsRanged = true;
        }
        else
        {
            attackIsRanged = false;
        }

        ExhaustActions();
        UpdateVisualsDirection(false);
        VisualUpdate();
        unitView.UpdateData();
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

        VisualMaterialHelper helper = currentBoat.GetComponent<VisualMaterialHelper>();
        if (helper != null)
        {
            UpdateMaterialColor(civColor);
        }

        boatReference = UnitManager.Instance.GetUnitDataByType(UnitType.Ship, unitReference.civType);

        currentAttack = boatReference.attack;
        currentDefense = boatReference.defense;
        currentWalkRange = boatReference.walkRange;
        currentAttackRange = boatReference.attackRange;

        if (currentAttackRange > 1)
        {
            attackIsRanged = true;
        }
        else
        {
            attackIsRanged = false;
        }

        ExhaustActions();
        UpdateVisualsDirection(false);
        VisualUpdate();
        unitView.UpdateData();
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
        else
        {
            attackIsRanged = false;
        }
        SpawnParticle(GameManager.Instance.resourceHarvestParticle);
        ExhaustActions();
        VisualUpdate();
    }

    public void TraderAction()
    {
        //more checks here probably
        GameManager.Instance.AddStars(GameManager.Instance.activePlayerIndex, GameManager.Instance.data.traderActionReward);
        Consume();
        
    }

    IEnumerator DeathWithDelay()
    {
        Deselect();
        if (originCity.cityData.population > 0)
        {           
            originCity.RemovePopulation();
        }
      
        GameManager.Instance.sessionPlayers[playerOwnerIndex].playerUnits.Remove(this);
        parentHex.UnitOut(this, true);
        yield return new WaitForSeconds(.1f);
        Death(false); //switch this to consume or something, different animation probably. It still is death. 
    }

    void Consume()
    {
        ExhaustActions();
        visualAnim.SetTrigger("traderAction");
        SpawnParticle(GameManager.Instance.traderActionParticle);
        StartCoroutine(DeathWithDelay());

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

    public void SpawnSetup(WorldHex newHex, int playerIndex, UnitData referenceData, bool exhaustOnSpawn, bool addToCityPopulation)
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
        if (newHex.hexData.hasCity)
        {
            originCity = newHex;
            if (addToCityPopulation)
            {
                originCity.AddPopulation();
            }
        }
       
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
            ResetActions(true, true);
        }

        unitView.SetData(this);
        visualUnit = unitVisualLayer.GetChild(0).gameObject;

        if (visualUnit != null)
        {
            Destroy(visualUnit);
        }

        visualUnit = Instantiate(referenceData.GetPrefab(), unitVisualLayer);

        visualAnim = visualUnit.transform.GetChild(0).GetChild(0).GetComponent<Animator>();
        visualAnim.SetTrigger("Idle");
        UpdateMaterialColor(civColor);
        SpawnParticle(UnitManager.Instance.unitSpawnParticle);
        UpdateVisualsDirection(false);
        ValidateRemainigActions();
        VisualUpdate();

        if (!GameManager.Instance.GetPlayerByIndex(playerOwnerIndex).isAI())
        {
            UnitManager.Instance.SelectUnit(this);
        }
     
    }

    void UpdateMaterialColor(Color newColor)
    {
        if (type == UnitType.Ship || isShip)
        {
            
            VisualMaterialHelper helper = currentBoat.GetComponent<VisualMaterialHelper>();
            if (helper != null)
            {
                foreach (GameObject obj in helper.objectsToChangeMaterials)
                {
                    obj.GetComponent<MeshRenderer>().materials[0].color = newColor;
                }
            }
        }
        else if (type == UnitType.Siege)
        {
            VisualMaterialHelper helper = visualUnit.GetComponent<VisualMaterialHelper>();
            if (helper != null)
            {
                foreach (GameObject obj in helper.objectsToChangeMaterials)
                {
                    obj.GetComponent<MeshRenderer>().materials[0].SetColor("_ColorShift", newColor);
                }
            }
        }
        else if (type == UnitType.Cavalry)
        {
            SkinnedMeshRenderer renderer = visualAnim.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
            renderer.materials[0].SetColor("_ColorShift", newColor);
            renderer = visualAnim.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>();
            renderer.materials[0].SetColor("_ColorShift", newColor);
        }
        else
        {
            SkinnedMeshRenderer renderer = visualAnim.transform.GetChild(0).GetComponent<SkinnedMeshRenderer>();
            renderer.materials[0].SetColor("_ColorShift", newColor);
        }
    }

    public void ExhaustActions()
    {
        currentAttackCharges = 0;
        currentMovePoints = 0;
        hasMoved = true;
        hasAttacked = true;

        ValidateRemainigActions();
        UnitManager.Instance.RefresetSelection();
    }

    void ResetActions(bool isEndOfTurn, bool skipHeal = false)
    {
        if (!skipHeal)
        {
            if (isEndOfTurn)
            {

                if (!hasMoved && !hasAttacked)
                {
                    if (isBoat || isShip)
                    {
                        if (boatReference.healAtTurnEnd)
                        {
                            HealWithDefaultValue();

                        }
                        else
                        {
                            shouldHealThisTurn = true;
                        }
                    }
                    else
                    {
                        if (unitReference.healAtTurnEnd)
                        {
                            HealWithDefaultValue();

                        }
                        else
                        {
                            shouldHealThisTurn = true;
                        }
                    }

                }
                else
                {
                    shouldHealThisTurn = false;
                }
            }
            else
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
        }

        hasMoved = false;
        hasAttacked = false;

        noWalkHexInRange = false;
        noAttackHexInRange = false;
        buttonActionPossible = true;

        //optimize here by skipping this check at end of turn
        if (isBoat || isShip)
        {
            currentAttackCharges = boatReference.attackCharges;
            currentMovePoints = boatReference.moveCharges;
        }
        else
        {
            currentAttackCharges = unitReference.attackCharges;
            currentMovePoints = unitReference.moveCharges;
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

        if (originCity != null)
        {
            if (originCity.cityData.population > 0)
            {
                originCity.RemovePopulation();
            }
        }

        originCity = parentHex;
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
        if (!GameManager.Instance.activePlayer.isAI())
        {
            UnitManager.Instance.SelectUnit(this);
            UIManager.Instance.ShowHexView(this.parentHex, this);
        }
     
    }

    public void ValidateRemainigActions()
    {
        UnitData unitDataToVerify = unitReference;
        if (isBoat || isShip)
        {
            unitDataToVerify = boatReference;
        }
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

        if (!isInteractable && !buttonActionPossible)
        {
            GameManager.Instance.GetPlayerByIndex(playerOwnerIndex).unitsWithActions.Remove(this);
        }
        VisualUpdate();
    }

    public void VisualUpdate()
    {
        //you can further optimize this ez
        if (isInteractable)
        {
            if (visualUnit != null)
            {
                UpdateMaterialColor(civColor);
            }
        }
        else
        {
            if (visualUnit != null)
            {
                UpdateMaterialColor(inactiveColor);
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
            UnitManager.Instance.StartMoveSequence(this, selecedHex, true, true);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool ReceiveDamage(int value)
    {
        int adjustedDamage = Mathf.Clamp(value - currentDefense, 0, 100);
        localHealth = localHealth - adjustedDamage;

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
        if (isBoat || isShip)
        {
            if (localHealth < boatReference.health)
            {
                localHealth += value;
                localHealth = Mathf.Clamp(localHealth, 0, unitReference.health);
                unitView.UpdateData();
                SpawnParticle(UnitManager.Instance.unitHealParticle);
            }
        }
        else
        {
            if (localHealth < unitReference.health)
            {
                localHealth += value;
                localHealth = Mathf.Clamp(localHealth, 0, unitReference.health);
                unitView.UpdateData();
                SpawnParticle(UnitManager.Instance.unitHealParticle);
            }
        }
    }

    public void HealWithDefaultValue()
    {
        Heal(unitReference.heal);
    }

    public void HealAction()
    {
        HealWithDefaultValue();
        ExhaustActions();
    }

    public void Death(bool affectStats)
    {
        this.parentHex.HideHighlight();
        Destroy(this.gameObject);
    }

    public void InstantDeath(bool affectstats)
    {
        Deselect();

        if (originCity != null)
        {
            if (originCity.cityData.population > 0)
            {
                originCity.RemovePopulation();
            }
        }

        GameManager.Instance.sessionPlayers[playerOwnerIndex].playerUnits.Remove(this);
        parentHex.UnitOut(this, true);
        Destroy(this.gameObject);
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
