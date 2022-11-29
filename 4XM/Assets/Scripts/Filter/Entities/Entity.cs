using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class Entity : MonoBehaviour, IDamagable
{
    [HideInInspector] public WaveManager spawner;
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public GameObject player; //The AI should be aware of the player at all times. 
    [HideInInspector] public GameObject targetPlant;
    [HideInInspector] public SpriteRenderer sprite;
    [HideInInspector] public Animator animator;
    [HideInInspector] public bool hasDied;
    [HideInInspector] public bool inViewOfPlayer;

    public GameObject deathParticleEffect;
    public GameObject weaponSource;
    [SerializeField] bool hitColliderAnimationControlled;

    [Header("Stats")]
    public int maximumHealth = 10; // Max health
    public int speed = 1 ; //entity speed
    public float hitFeedbackTime = 0.5f; //visual related 
    public int scoreReward = 1;
    public int attackDamage = 1;

    [Header("Attack Config")]
    public float attackSpeed = 1; //internval in time between attacks
    public float attackRange = 2;  //attack range
    public int attackCount = 1; //attacks that happen before cooldown occurs
    public int attackCooldown =1; //cooldown between attacks;

    [HideInInspector] public float internalAttackSpeed;
    [HideInInspector] public float internalAttackCooldown;
    [HideInInspector] public float internalAttackCount;

    [Header("Knockback Settings")]
    public float getKnockedBackDuration;
    public float getKnockedBackStrength;
    private bool gettingKnockedBack = false;
    [HideInInspector] public bool canGetKnockedBack = true;

    [Header("Behaviour")]
    [HideInInspector] public  GameObject target;
    [HideInInspector] public bool targetInRange;
    public Direction direction;
    public EntityAIState state;
    public bool isHit;

    public int Health { get; set; }

    Vector3 previousPos;
    Direction futureDir;
    float previousX;
    float previousY;
    Vector3 weaponUp = new Vector3(0, .5f, 0);
    Vector3 weaponDown = new Vector3(0, -.5f, 0);
    Vector3 weaponRight= new Vector3(.5f, 0, 0);
    Vector3 weaponLeft = new Vector3(-.5f, 0, 0);

    public virtual void SetUpAgent()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.speed = speed;
    }

    public virtual void OnDamage(int amount)
    {
        Health -= amount;
        if (Health <= 0)
        {
            if (spawner != null && !hasDied)
            {
                spawner.enemiesRemaining--;
                HB_GameManager.Instance.RewardScore(scoreReward);
                //maybe pass in chance here
                GameObject drop = ItemManager.Instance.GetRandomDrop(1);
                if (drop != null)
                {
                    Instantiate(drop, transform.position, Quaternion.identity);
                }

                hasDied = true;
            }
          
            //Instantiate(deathParticleEffect, this.transform.position, Quaternion.identity);
            Destroy(this.gameObject);
            return;
        }

        HitEffects();
       
    }

    public virtual void HitEffects()
    {
        animator.SetTrigger("Hit");
    }

    public virtual void SetSpeed(int amount)
    {
        speed = amount;
        SetUpAgent();
    }

    public virtual void SetHealth(int amount, bool fillHealth)
    {
        maximumHealth = amount;
        if (fillHealth)
        {
            MaxHealth();
        }
    }

    public virtual void MaxHealth()
    {
        Health = maximumHealth;
    }

    public virtual void UpdateDirection()
    {
        float xAxis = (transform.position - previousPos).x;
        float yAxis = (transform.position - previousPos).y;
        float yDif = Mathf.Abs(previousY - yAxis);
        float xDif = Mathf.Abs(previousX - xAxis);

        float animX = 0;
        float animY = 0;
        if (yDif > xDif)
        {
            if (yAxis < previousY)
            {
                futureDir = Direction.DOWN;
                if (!hitColliderAnimationControlled)
                {
                    weaponSource.transform.localPosition = weaponDown;
                }
               
                animY = -1f;
            }
            else if (yAxis > previousY)
            {
                futureDir = Direction.UP;
                if (!hitColliderAnimationControlled)
                {
                    weaponSource.transform.localPosition = weaponUp;
                }

                animY = 1f;
            }
        }
        else
        {
            if (xAxis < previousX)
            {
                futureDir = Direction.LEFT;
                if (!hitColliderAnimationControlled)
                {
                    weaponSource.transform.localPosition = weaponLeft;
                }
                animX = -1f;
            }
            else if (xAxis > previousX)
            {
                futureDir = Direction.RIGHT;
                if (!hitColliderAnimationControlled)
                {
                    weaponSource.transform.localPosition = weaponRight;
                }
                animX = 1f;
            }
        }

        if (futureDir != direction)
        {
            direction = futureDir;
        }

        animator.SetFloat("PosX", animX);
        animator.SetFloat("PosY", animY);

        previousX = xAxis;
        previousY = yAxis;
    }



    #region Getting knocked back by an object
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Entity"))
        {
            GetKnockedBack(collision.gameObject, getKnockedBackStrength);
        }
        else if (collision.gameObject.CompareTag("Bullet") && player != null)
        {
            if (collision.gameObject.GetComponent<Projectile>().nonPlayerOrigin != gameObject)
            {
                GetKnockedBack(player, getKnockedBackStrength);
            }
        }
        if (player == null) { Debug.Log("Player game object not assigned, knock back won't work with bullets!"); }
    }

    public virtual void GetKnockedBack(GameObject collidedGO, float amount)
    {
        if (amount != 0)
        {
            Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
            Vector3 enemyDir = (collidedGO.transform.position - gameObject.transform.position).normalized;
            Vector3 knockBackDir = -enemyDir * amount;

            if (!gettingKnockedBack)
            {
                rb.velocity = Vector2.zero;
                if (gameObject.GetComponent<NavMeshAgent>() != null)
                {
                    gameObject.GetComponent<NavMeshAgent>().velocity = Vector2.zero;
                    gameObject.GetComponent<NavMeshAgent>().Move(knockBackDir);
                }
                else
                {
                    rb.AddForce(knockBackDir);
                }
                StartCoroutine(KnockBackDurationCo(getKnockedBackDuration));
                gettingKnockedBack = true;
            }
        }
    }
    //Waits certain duration before setting the velocity of the GO to zero, so the following movement is smooth
    public virtual IEnumerator KnockBackDurationCo(float duration)
    {
        yield return new WaitForSeconds(duration);
        gettingKnockedBack = false;
    }
    #endregion
}

public enum EntityAIState
{
    IDLE,
    MOVINGTOTARGET,
    ATTACKING,
}

public enum EnemyType
{
    LOCUST,
}
