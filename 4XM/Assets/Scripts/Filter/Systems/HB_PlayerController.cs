using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class HB_PlayerController : MonoBehaviour, IDamagable
{
    public static HB_PlayerController Instance;
    public PlayerData data;
    Rigidbody2D rb;

    [Header("Extra Features")]
    public bool useMouseForAimDirection;

    [Header("Character State")]
    public bool isRunning;
    public bool isMoving;
    public bool isShooting;
    public bool isHit;
    Vector2 dir;

    public Direction currentDir = Direction.RIGHT;

    [SerializeField] float hitVisualLenght;
    public ProjectileWeapon weaponObject;
    [SerializeField] Transform weaponParent;
    float tempSpeed;
    float moveX;
    float moveY;
    Vector2 tempPos;
    Vector2 newVel;

    Direction futureDir;
    Vector3 mousePos;
    Vector3 aimDir;
    Vector3 euAngle;
    float angle;

    [Header("Interactables")]
    [SerializeField] LayerMask interactablesLayer;
    [SerializeField] LayerMask entitiesLayer;
    [SerializeField] Interactable currentInteractable;


    public bool controlsActive = true;
    public bool hasTakenDamageThisWave;

    [Header("Visuals")]

    ColorAdjustments colorAdjustments;
    [SerializeField] Gradient postGradient;
    [SerializeField] Animator playerAnimator;
    [SerializeField] Animator effectAnimator;

    public bool IsInFog;
    [SerializeField] float fogTimeToDamage = 2f;
    float internalFogTimer;

    public int Health
    {
        get
        {
            return data.currentHealth;
        }
        set
        {
            data.currentHealth = value;
        }
    }

    Color ColorFromGradient(float value)  // float between 0-1
    {
        return postGradient.Evaluate(value);
    }
    private void Awake()
    {
        Instance = this;

        rb = GetComponent<Rigidbody2D>();

        HealMax(false);
    }

    void Start()
    {
        ColorAdjustments _colorAdjustments;
        if (HB_GameManager.Instance.globalVolume.profile.TryGet<ColorAdjustments>(out _colorAdjustments))
        {
            colorAdjustments = _colorAdjustments;
        }

    }

    private void Update()
    {
        if (HB_GameManager.Instance.playerDead || !controlsActive)
        {
            rb.velocity = Vector3.zero;
            isMoving = false;
            PlayerAnimations(false);
            return;
        }

        if (HB_GameManager.Instance.isPaused || HB_UIManager.Instance.subPanelActive)
        {
            return;
        }

        if (IsInFog)
        {
            internalFogTimer += 1 * Time.deltaTime;
            if (internalFogTimer > fogTimeToDamage)
            {
                OnDamage(1);
                internalFogTimer = 0;
            }
        }
        Move();
        GetInteractablesInRange();

        if (currentInteractable != null)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentInteractable.Interact();
            }
        }

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        aimDir = (mousePos - weaponParent.position).normalized;
        angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        euAngle.z = angle;
        weaponParent.transform.eulerAngles = euAngle;

        UpdateDirection(angle);

        if (angle > 90 || angle < -90)
        {

            weaponObject.DirectionUpdate(Direction.LEFT);
        }
        else
        {
            weaponObject.DirectionUpdate(Direction.RIGHT);
        }

        if (weaponObject.weaponIsActive)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                weaponObject.Attack(currentDir, aimDir);
            }
        }

        PostProcessingEffects();
    }

    public void DataLoaded(PlayerData _data)
    {
        data = _data;
        SetWeapon("weaponPistol");
        HB_UIManager.Instance?.UpdatePlayerHealth(true, false);
        //spawn
    }

    public void UpgradeStatLevel(int associatedStatIndex)
    {
        switch (associatedStatIndex)
        {
            case 0:
                data.MaxHealth.statLevel++;
                HealMax(false);
                HB_UIManager.Instance.UpdatePlayerHealth(true, false);
                break;
            case 1:
                data.Damage.statLevel++;
                break;
            case 2:
               data.Speed.statLevel++;
                break;
            case 3:
                data.Range.statLevel++;
                break;
        }
    }

    public void UpdateWeaponLevels(int[] newWeaponLevels)
    {
        weaponObject.weaponLevels = newWeaponLevels;
    }

    public void FogStatusChanged(bool inFog)
    {
        IsInFog = inFog;
        if (!IsInFog)
        {
            internalFogTimer = 0;
        }
    }

    public void HidePlayer()
    {
        controlsActive = false;
        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    public void ShowPlayer()
    {
        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(true);
        }

        controlsActive = true;
    }

    IEnumerator DestroyWithDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }

    public void SetWeapon(string weaponKey)
    {
       
        if (ItemManager.Instance.weaponDatabase.ContainsKey(weaponKey))
        {
            data.weaponKey = weaponKey;
            SetWeapon(ItemManager.Instance.weaponDatabase[weaponKey]);
            return;
        }
        else
        {
            Debug.LogWarning("Weapon does not exist in weapon Database");
        }
    }

    public void SetWeapon(WeaponScriptable newWeapon)
    {
        weaponObject.UpdateData(newWeapon);
        HB_EventManager.Instance.OnWeaponEquipped();
    }

    public void PostProcessingEffects()
    {

    }

    public void PlayerAnimations(bool isMoving)
    {
        playerAnimator.SetBool("IsRunning", isRunning);
        playerAnimator.SetBool("IsWalking", isMoving);
    }

    public void ShootAnimation(bool isShooting)
    {
        playerAnimator.SetBool("IsShooting", isShooting);
    }

    public void PlayerSlingShot()
    {
        playerAnimator.SetTrigger("SlingShot");
    }

    /// <summary>
    /// This Move instantly works with controlelrs and all directional buttons. Plus the run thingy.
    /// </summary>
    void Move()
    {
        moveX = Input.GetAxis("Horizontal");
        moveY = Input.GetAxis("Vertical");

        bool newMovingState;

        if (moveX != 0 || moveY != 0)
        {
            newMovingState = true;
        }
        else
        {
            newMovingState = false;
        }

        tempSpeed = data.Speed.CurrentLevelValue();
        isRunning = false;

        /* //Running removed to give more emphases in the speed upgrades. WIll be moved to a cheatcode
        if (Input.GetKey(KeyCode.LeftShift))
        {
            tempSpeed = data.runningSpeed;
            isRunning = true;
        }
        else
        {
            isRunning = false;
        }*/ 

        tempPos.x = transform.position.x + moveX * tempSpeed * Time.deltaTime;
        tempPos.y = transform.position.y + moveY * tempSpeed * Time.deltaTime;
        newVel.x = moveX * tempSpeed;
        newVel.y = moveY * tempSpeed;

        rb.velocity = newVel; ;

        if (moveX > 0)
        {
            futureDir = Direction.RIGHT;
            dir.x = 1;
            dir.y = 0;
        }
        if (moveX < 0)
        {
            futureDir = Direction.LEFT;
            dir.x = -1;
            dir.y = 0;
        }
        if (moveY > 0)
        {
            futureDir = Direction.UP;
            dir.x = 0;
            dir.y = 1;
        }
        if (moveY < 0)
        {
            futureDir = Direction.DOWN;
            dir.x = 0;
            dir.y = -1;
        }

        if (newMovingState != isMoving)
        {
            isMoving = newMovingState;
            PlayerAnimations(isMoving);
        }

        if (futureDir != currentDir)
        {
            currentDir = futureDir;
        }
    }

    void GetInteractablesInRange()
    {
        if (isMoving)
        {
            Collider2D[] results = Physics2D.OverlapCircleAll(this.transform.position, data.interactableRadius, interactablesLayer); //this.transform.up, Mathf.Infinity, interactablesLayer);
            if (results.Length > 0)
            {
                Collider2D selectedHit = results[0];

                if (results.Length > 1)
                {
                    float smallestDistance = Vector3.Distance(transform.position, selectedHit.transform.position);
                    for (int i = 0; i < results.Length - 1; i++)
                    {
                        if (results[i].transform.gameObject.GetComponent<Interactable>() != null)
                        {
                            float newDistance = Vector3.Distance(transform.position, results[i].transform.position);
                            if (newDistance < smallestDistance)
                            {
                                if (results[i].transform.gameObject.GetComponent<Interactable>().isInteractable)
                                {
                                    smallestDistance = newDistance;
                                    selectedHit = results[i];
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }

                    }
                }

                if (selectedHit.transform.gameObject.GetComponent<Interactable>().isInteractable)
                {
                    currentInteractable = selectedHit.transform.gameObject.GetComponent<Interactable>();
                    HB_UIManager.Instance.ShowOverlay(selectedHit.transform.gameObject, currentInteractable.overlayOffset, currentInteractable.usePlacementOverlay);
                }
                else
                {
                    HB_UIManager.Instance.HideOverlay();
                }
            }
            else
            {
                HB_UIManager.Instance.HideOverlay();
                currentInteractable = null;
            }
        }

    }

    public void HealMax(bool updateUI)
    {
        Health = (int)data.MaxHealth.CurrentLevelValue();

        if (updateUI)
        {
            HB_UIManager.Instance.UpdatePlayerHealth(false, false);
        }
    }

    public void Heal(int amount)
    {
        //healFeedback?.PlayFeedbacks();
        Health = Mathf.Clamp(Health + amount, 0, (int)data.MaxHealth.CurrentLevelValue());
        HB_UIManager.Instance.UpdatePlayerHealth(false, false);
    }

    public void OnDamage(int amount)
    {
        if (HB_GameManager.Instance.devMode && HB_GameManager.Instance.infiniteHealth)
        {
            return;
        }

        if (HB_GameManager.Instance.playerDead)
        {
            return;
        }

        HB_AudioManager.Instance.Play("hit");
        //hitFeedback?.PlayFeedbacks();

        hasTakenDamageThisWave = true;
       
        if (Health - amount <= 0)
        {
            isMoving = false;
            playerAnimator.SetBool("IsDead", true);
            HB_AudioManager.Instance.Stop("hit");
            HB_AudioManager.Instance.Play("death");
            HB_GameManager.Instance.GameOver();
            return;
        }

        Health = Mathf.Clamp(Health - amount, 0, (int)data.MaxHealth.CurrentLevelValue());
        HB_UIManager.Instance.UpdatePlayerHealth(false, false);
    }

    public void UpdateDirection(float angle)
    {
        if (useMouseForAimDirection)
        {
            if (angle > 90 || angle < -90)
            {
                futureDir = Direction.LEFT;
                dir.x = -1;
                dir.y = 0;
            }
            else
            {
                futureDir = Direction.RIGHT;
                dir.x = 1;
                dir.y = 0;
            }

            if (angle > 45 && angle <= 135)
            {
                futureDir = Direction.UP;
                dir.x = 0;
                dir.y = 1;
            }

            if (angle < -45 && angle >= -135)
            {
                futureDir = Direction.DOWN;
                dir.x = 0;
                dir.y = -1;
            }

            if (futureDir != currentDir)
            {
                currentDir = futureDir;
            }

        }

        weaponObject.DirectionUpdate(currentDir);

        playerAnimator.SetFloat("PosX", dir.x);
        playerAnimator.SetFloat("PosY", dir.y);

    }

}

public enum Direction
{
    UP,
    DOWN,
    LEFT,
    RIGHT
}

//Old system to spawn weapons instead of changing them through data. Kept just in case. 
/*
     Vector3 currentRotation = Vector3.zero;

     if (activeWeapon != null)
     {
         currentRotation = activeWeapon.transform.eulerAngles;
         ProjectileWeapon projectileWeapon = activeWeapon.GetComponent<ProjectileWeapon>();
         if (projectileWeapon != null)
         {
             StartCoroutine(DestroyWithDelay(projectileWeapon.bulletPoolParent, 2f));
         }
     }

     if (weaponParent.transform.childCount > 0)
     {
         GameObject previousWeapon = weaponParent.transform.GetChild(0).gameObject;
         currentRotation = previousWeapon.transform.eulerAngles;

         if (previousWeapon != null)
         {
             Destroy(previousWeapon);
         }
     }

     GameObject newWeapon = Instantiate(prefabWeapon, weaponParent.transform.position, Quaternion.Euler(currentRotation));

     ProjectileWeapon newWeaponComponent = newWeapon.GetComponent<ProjectileWeapon>();

     if (newWeaponComponent != null)
     {
         newWeapon.transform.parent = weaponParent.transform;

         newWeapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
         newWeapon.transform.localPosition = Vector3.zero;

         newWeaponComponent.CacluateWeaponStats(data.baseDamage, data.critDamage, data.critChance);

         hasActiveWeapon = true;
         activeWeapon = newWeaponComponent;
         newWeaponComponent.DirectionUpdate(currentDir);

     }
     else
     {
         hasActiveWeapon = false;
     }

     HB_EventManager.Instance.OnWeaponEquipped();
     */

