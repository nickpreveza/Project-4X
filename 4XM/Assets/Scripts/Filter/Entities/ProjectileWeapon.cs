using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileWeapon : MonoBehaviour
{
    public bool weaponIsActive;
    public int[] weaponLevels;
    [SerializeField] WeaponData weapon;

    [SerializeField] Vector3 damageParamemeters = new Vector3(); //damage, criticalDamage, criticalChance
    [SerializeField] Vector2 projectileParameters = new Vector2(); //projectileLifetime, projectileSpeed
    [SerializeField] float calculatedFirerate = 1;
    [Header("Weapon Settings")]
    public GameObject handler;
    public Vector3 weaponOffset;
    public bool oneOffset;

    [Header("Projectile Weapon Settings")]
    public string animatorTrigger;
    public float delayForShot;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    [SerializeField] int bulletsInPool = 20;
    [SerializeField] List<GameObject> bulletPool = new List<GameObject>();
    [SerializeField] Vector3 bulletSpawnOffset;
    [SerializeField] GameObject bulletSpawnPosition;

    [SerializeField] Direction gunDirection = Direction.RIGHT;

    GameObject bulletObject;
    float internalFireRate;
    [HideInInspector] public GameObject bulletPoolParent;

    [Header("Visuals")]
    SpriteRenderer weaponSprite;
    SpriteRenderer weaponEffect;
    Animator weaponAnimator;

    public bool shootingReady;
    bool canShoot;
    
    [SerializeField] float cameraShakeAmount = 1;
    [SerializeField] float cameraShakeTime = 0.1f;

    [SerializeField] Vector3 UpSpawnOffset;
    [SerializeField] Vector3 DownSpawnOffset;
    [SerializeField] Vector3 RightSpawnOffset;
    [SerializeField] Vector3 LeftSpawnOffset;


    private void OnDrawGizmos()
    {
        if (oneOffset)
        {
            Gizmos.DrawSphere(this.transform.position + weaponOffset, 0.1f);
            Gizmos.DrawSphere(this.transform.position + bulletSpawnOffset, 0.1f);
        }
        else
        {
            Gizmos.DrawSphere(this.transform.position + weaponOffset, 0.1f);
            Gizmos.DrawSphere(this.transform.position + UpSpawnOffset, 0.1f);
            Gizmos.DrawSphere(this.transform.position + DownSpawnOffset, 0.1f);
            Gizmos.DrawSphere(this.transform.position + RightSpawnOffset, 0.1f);
            Gizmos.DrawSphere(this.transform.position + LeftSpawnOffset, 0.1f);
        }
       
    }

    private void Start()
    {
        CreateProjectilePool();

        weaponSprite = GetComponent<SpriteRenderer>();
        weaponAnimator = GetComponent<Animator>();

        weaponEffect = transform.GetChild(1).GetComponent<SpriteRenderer>();
        weaponEffect.gameObject.SetActive(false);

        weaponIsActive = false;
    }

    public void UpdateData(WeaponScriptable newWeapon)
    {
        if (newWeapon == null)
        {
            weaponIsActive = false;
        }
        weaponIsActive = true;
        weapon = newWeapon.weaponData;
        switch (weapon.attackPattern)
        {
            case AttackPattern.Pistol:
                weaponLevels = HB_PlayerController.Instance.data.pistolLevelData;
                break;
            case AttackPattern.Shotgun:
                weaponLevels = HB_PlayerController.Instance.data.shotgunLevelData;
                break;
        }

        weaponSprite.sprite = weapon.weaponSprite;
        CacluateWeaponStats();
    }

    public void CacluateWeaponStats()
    {
        damageParamemeters.x = weapon.Damage.LevelValue(weaponLevels[0]) + HB_PlayerController.Instance.data.Damage.CurrentLevelValue();
        damageParamemeters.y = weapon.CriticalDamage.LevelValue(weaponLevels[1]); //+ HB_PlayerController.Instance.data.GetCritDamage());
        damageParamemeters.z = weapon.CriticalChance.LevelValue(weaponLevels[2]); //+ HB_PlayerController.Instance.data.GetCritChance());
        calculatedFirerate = weapon.FireRate.LevelValue(weaponLevels[3]);

        projectileParameters.x = weapon.projectileLifetime + HB_PlayerController.Instance.data.Range.CurrentLevelValue();
        projectileParameters.y = weapon.projectileSpeed;
    }
    void CreateProjectilePool()
    {
        if (bulletPoolParent != null)
        {
            Destroy(bulletPoolParent);
        }

        bulletPoolParent = new GameObject("BulletPoolParent");
        bulletPool = new List<GameObject>();

        for (int i = 0; i < bulletsInPool; i++)
        {
            GameObject projectile = Instantiate(bulletPrefab, bulletPoolParent.transform);
            projectile.SetActive(false);
            bulletPool.Add(projectile);
        }

        shootingReady = true;
    }

    private void Update()
    {
        internalFireRate += Time.deltaTime;
        if (internalFireRate > calculatedFirerate)
        {
            canShoot = true;
        }
        else
        {
            canShoot = false;
        }
    }


    public void Attack(Direction dir, Vector3 vectorDir)
    {
        if (!weaponIsActive)
        {
            return;
        }

        if (canShoot &&  ItemManager.Instance.DoesPlayerHaveItem("hemppebbles", weapon.ammoPerShot))
        {
            //HB_PlayerController.Instance.shootingFeedback?.PlayFeedbacks();
            ItemManager.Instance.UseItem("hemppebbles", weapon.ammoPerShot, false);
            Vector3 directionVector = Vector3.zero;
            if (!oneOffset)
            {
                switch (dir)
                {
                    case Direction.UP:
                        directionVector.y = 1;
                        break;
                    case Direction.DOWN:
                        directionVector.y = -1;
                        break;
                    case Direction.LEFT:
                        directionVector.x = -1;
                        break;
                    case Direction.RIGHT:
                        directionVector.x = 1;
                        break;

                }
            }
            else
            {
                directionVector = vectorDir;
            }

            HB_AudioManager.Instance.Play("bullet_fall");
            internalFireRate = 0;
            switch (weapon.attackPattern)
            {
                case AttackPattern.Slingshot:
                    StartCoroutine(SlingshotShot(directionVector));
                    break;
                case AttackPattern.Pistol:
                    HB_AudioManager.Instance.Play("pistol");
                    Pistol(directionVector);
                    break;
                case AttackPattern.Shotgun:
                    HB_AudioManager.Instance.Play("shotgun");
                    Invoke("PlayShotgunPump", 0.4f);
                    Shotgun(directionVector, weapon.shotgunSpreadAngle);
                    break;
                case AttackPattern.Burst:
                    StartCoroutine(BurstShot(directionVector, weapon.burstRate, weapon.burstAmmo));
                    break;
                case AttackPattern.DoubleBarrel:
                    HB_AudioManager.Instance.Play("shotgun");
                    Invoke("PlayShotgunPump", 0.4f);
                    DoubleBarrel(directionVector, weapon.shotgunSpreadAngle);
                    break;
            }
        }
        else
        {
            HB_AudioManager.Instance.Play("failed_shot");
        }

    }

    public void PlayShotgunPump()
    {
        HB_AudioManager.Instance.Play("shotgun_pump");
    }

    IEnumerator SlingshotShot(Vector3 dir)
    {
        HB_PlayerController.Instance.PlayerSlingShot();
        yield return new WaitForSeconds(delayForShot);
        Pistol(dir);
    }

    public void Pistol(Vector3 dir)
    {
        HB_AudioManager.Instance.Play("slingshot");
        weaponAnimator.SetTrigger("Shoot");
        bulletObject = null;
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (!bulletPool[i].activeSelf)
            {
                bulletObject = bulletPool[i];
                break;
            }
        }

        if (bulletObject == null)
        {
            bulletObject = bulletPool[0];
            Debug.LogError("Bullet Pool does not have enough bullets for Fire Rate");
        }

        bulletObject.transform.position = bulletSpawnPosition.transform.position;
        bulletObject.transform.rotation = this.transform.rotation;
        bulletObject.GetComponent<Projectile>().origin = this;
        bulletObject.SetActive(true);
        bulletObject.GetComponent<Projectile>().Attack(dir, 0, damageParamemeters, projectileParameters);
      
    }


    public void Shotgun(Vector3 dir, float spread)
    {
        weaponAnimator.SetTrigger("Shoot");
        List<GameObject> bulletObjects = new List<GameObject>();
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (!bulletPool[i].activeSelf)
            {
                if (!bulletObjects.Contains(bulletPool[i]))
                {
                    bulletObjects.Add(bulletPool[i]);
                }

                if (bulletObjects.Count >= 3)
                {
                    break;
                }
               
            }
        }

        for (int i = 0; i < 3; i++)
        {
            bulletObjects[i].transform.position = bulletSpawnPosition.transform.position;
            bulletObjects[i].transform.rotation = this.transform.rotation;
            bulletObjects[i].GetComponent<Projectile>().origin = this;
            bulletObjects[i].SetActive(true);
            bulletObjects[i].GetComponent<Projectile>().Attack(dir, spread - spread * i, damageParamemeters, projectileParameters);
        }
        bulletObjects.Clear();
    }

    public void DoubleBarrel(Vector3 dir, float spread)
    {
        weaponAnimator.SetTrigger("Shoot");
        List<GameObject> bulletObjects = new List<GameObject>();
        for (int i = 0; i < bulletPool.Count; i++)
        {
            if (!bulletPool[i].activeSelf)
            {
                if (!bulletObjects.Contains(bulletPool[i]))
                {
                    bulletObjects.Add(bulletPool[i]);
                }

                if (bulletObjects.Count >= 2)
                {
                    break;
                }

            }
        }

        for (int i = 0; i < 2; i++)
        {
            bulletObjects[i].transform.position = bulletSpawnPosition.transform.position;
            bulletObjects[i].transform.rotation = this.transform.rotation;
            bulletObjects[i].GetComponent<Projectile>().origin = this;
            bulletObjects[i].SetActive(true);
            if (i != 0)
            {
                bulletObjects[i].GetComponent<Projectile>().Attack(dir, spread * -i, damageParamemeters, projectileParameters);
            }
            else
            {
                bulletObjects[i].GetComponent<Projectile>().Attack(dir, spread, damageParamemeters, projectileParameters);
            }
           
        }
        bulletObjects.Clear();
    }

    public IEnumerator BurstShot(Vector3 dir, float burstRate, int ammo)
    {
        for (int i = 0; i < ammo; i++)
        {
            HB_AudioManager.Instance.Play("burst");
            Pistol(dir);
            yield return new WaitForSeconds(burstRate);
        }
    }

    public void DirectionUpdate(Direction newDir)
    {
        if (newDir == gunDirection)
        {
            return;
        }

        gunDirection = newDir;
        weaponSprite = GetComponent<SpriteRenderer>();
        if (oneOffset)
        {
            switch (gunDirection)
            {
                case Direction.LEFT:
                case Direction.UP:
                    //bulletSpawnPosition.transform.localPosition = bulletSpawnOffset;
                    weaponSprite.flipY = true;
                    break;
                case Direction.RIGHT:
                case Direction.DOWN:
                   //bulletSpawnPosition.transform.localPosition = bulletSpawnOffset;
                    weaponSprite.flipY = false;
                    break;
            }
        }
        else
        {
            switch (gunDirection)
            {
                case Direction.LEFT:
                    bulletSpawnPosition.transform.localPosition = LeftSpawnOffset;
                    break;
                case Direction.RIGHT:
                    bulletSpawnPosition.transform.localPosition = RightSpawnOffset;
                    break;
                case Direction.UP:
                    bulletSpawnPosition.transform.localPosition = UpSpawnOffset;
                    break;
                case Direction.DOWN:
                    bulletSpawnPosition.transform.localPosition = DownSpawnOffset;
                    break;
            }
        }
    }
}

public enum AttackPattern
{
    Slingshot,
    Pistol,
    Shotgun,
    Charge,
    Burst,
    DoubleBarrel
}

