using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LocustEnemy : Entity
{
    //float distanceFromPlayer;
    [SerializeField] float distanceFromTarget;
    bool changedState;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        Setup();
    }

    public void Setup()
    {
        target = HB_PlayerController.Instance.gameObject;
        MaxHealth();
        SetUpAgent();
        direction = Direction.RIGHT;
        player = HB_PlayerController.Instance.gameObject;
        weaponSource.GetComponent<EntityWeaponMelee>().Setup(this);
        HB_AudioManager.Instance.Play("locustAmbient");
    }

    public override void UpdateDirection()
    {
        base.UpdateDirection();
    }
    public override void SetUpAgent()
    {
        base.SetUpAgent();
    }

    public override void OnDamage(int amount)
    {
        HB_AudioManager.Instance.Play("locustHit");
        base.OnDamage(amount);
    }


    bool CheckIfTargetFound()
    {
        if (target != null)
        {
            state = EntityAIState.MOVINGTOTARGET;
            animator.SetBool("IsWalking", true);
            changedState = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    bool CheckifTargetLost()
    {
        if (target == null)
        {
            state = EntityAIState.IDLE;
            animator.SetBool("IsWalking", false);
            changedState = true;

            return true;
        }
        else
        {
            return false;
        }
    }

    bool CheckIfTargetInRange()
    {
        if (distanceFromTarget <= attackRange)
        {
            targetInRange = true;
            state = EntityAIState.ATTACKING;
            animator.SetBool("IsWalking", false);
            changedState = true;
            return true;
        }
        else
        {
            targetInRange = false;
            return false;
        }
    }

    bool CheckIfTargetMovedOutOfRange()
    {
        if (distanceFromTarget > attackRange)
        {
            targetInRange = false;
            state = EntityAIState.MOVINGTOTARGET;
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsWalking", true);
            changedState = true;
            return true;
        }
        else
        {
            targetInRange = true;
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.isActiveAndEnabled) { return; }

        if (HB_GameManager.Instance.playerDead) { target = null; }

        if (target != null) 
        {
            distanceFromTarget = Vector3.Distance(target.transform.position, transform.position);
        }

        internalAttackSpeed += 1f * Time.deltaTime;

        if (!agent.isStopped)
        {
            UpdateDirection();
        }

        switch (state)
        {
            case EntityAIState.IDLE:
                changedState = false;
                if (CheckIfTargetFound())
                {
                    return;
                }
                return;

            case EntityAIState.MOVINGTOTARGET:
                changedState = false;
                agent.isStopped = false;
                if (CheckifTargetLost())
                {
                    return;
                }
                if (CheckIfTargetInRange())
                {
                    return;
                }

                agent.SetDestination(target.transform.position);
                break;
            case EntityAIState.ATTACKING:
                if (CheckifTargetLost())
                {
                    return;
                }
                if (CheckIfTargetMovedOutOfRange())
                {
                    return;
                }

                changedState = false;
                agent.isStopped = true;
                CalculateTargetDirection();
                Attack();
                /*
                if (internalAttackSpeed > attackSpeed)
                {
                   
                    return;
                }
                else
                {
                    state = EntityAIState.IDLE;
                    animator.SetBool("isAttacking", false);
                    changedState = true;
                }*/
                break;
        }
    }

    void Attack()
    {
        weaponSource.GetComponent<EntityWeaponMelee>().haveAppliedDamage = false;
        HB_AudioManager.Instance.Play("locustAttack");
        animator.SetBool("IsAttacking", true);
        internalAttackSpeed = 0;
        //HB_PlayerController.Instance.OnDamage(attackDamage);
    }


    void CalculateTargetDirection()
    {
        float yDif = Mathf.Abs((transform.position - target.transform.position).y);
        float xDif = Mathf.Abs((transform.position - target.transform.position).x);
        float animX = 0;
        float animY = 0;
        Direction futureDir;

        if (yDif > xDif)
        {
            if (transform.position.y > target.transform.position.y)
            {
                futureDir = Direction.DOWN;
                animY = -1f;
            }
            else 
            {
                futureDir = Direction.UP;
                animY = 1f;
            }
        }
        else
        {
            if (transform.position.x > target.transform.position.x)
            {
                futureDir = Direction.LEFT;
                animX = -1f;
            }
            else
            {
                futureDir = Direction.RIGHT;
                animX= 1f;
            }
        }

        if (futureDir != direction)
        {
            direction = futureDir;
        }

        animator.SetFloat("PosX", animX);
        animator.SetFloat("PosY", animY);
    }

}


