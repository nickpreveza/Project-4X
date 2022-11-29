using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public ProjectileWeapon origin;
    public GameObject nonPlayerOrigin;
    public bool nonPlayerOriginBool;
    Rigidbody2D rb;
    Vector2 force;
    float lifetimeLeft;
    bool shouldCount;
    int damage;
    int critDamage;
    float critChance;
    bool hasAppliedDamage;
    public bool playSound;
    [HideInInspector] public Vector3 dir = new Vector3();
    [SerializeField] GameObject destroyParticleEffect;

    public static Vector2 rotate(Vector2 v, float delta)
    {
        delta *= Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }

    /// <summary>
    /// If <paramref name="rotationOffset"/> is set to 0, then it will shoot in the direction of the aim.
    /// </summary>
    /// <param name="rotationOffset"></param>
    /// <param name="_damage"></param>
    /// <param name="speed"></param>
    /// <param name="lifetime"></param>
    public void Attack(Vector2 testDir, float rotationOffset, Vector3 damageParameters, Vector2 projectileParameters)
    {
        //Rotation
        if (rotationOffset != 0)
        {
            dir = Quaternion.Euler(0, 0, rotationOffset) * testDir;
        }
        else if (rotationOffset == 0)
        {
            dir = testDir;
        }

        lifetimeLeft = projectileParameters.x;
        dir *= projectileParameters.y;
        shouldCount = true;
        damage = (int)damageParameters.x;
        critDamage = (int)damageParameters.y;
        critChance = damageParameters.z;
    }

    private void FixedUpdate()
    {
        if (shouldCount)
        {
            lifetimeLeft -= Time.deltaTime;
            gameObject.transform.position += dir * Time.fixedUnscaledDeltaTime;
            if (lifetimeLeft <= 0)
            {
                ReturnToPool();
            }
        }
    }

    void ReturnToPool()
    {
        shouldCount = false;
        if (!this.gameObject.activeSelf)
        {
            return;
        }

        hasAppliedDamage = false;
        gameObject.SetActive(false);
    }


    public void RotateProjectile(Vector2 targetPos)
    {
        transform.LookAt(targetPos, transform.up);
    }


    public IEnumerator DelayColliderEnable(float delayDur)
    {
        gameObject.GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(delayDur);
        gameObject.GetComponent<Collider2D>().enabled = true;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Entity"))
        {
            IDamagable targetInterface = collision.gameObject.GetComponent<IDamagable>();
            if (targetInterface != null && !hasAppliedDamage)
            {
                targetInterface.OnDamage(damage);
                hasAppliedDamage = true;
            }
        }
       
        if (hasAppliedDamage || collision.gameObject.CompareTag("Obstacles"))
        {
            if (collision.gameObject.CompareTag("Obstacles"))
            {
                if (playSound)
                {
                    HB_AudioManager.Instance.Play("bulletimpact");
                }

                //Instantiate(destroyParticleEffect, this.transform.position, Quaternion.identity);
            }
           
            ReturnToPool();
        }
    }
}
