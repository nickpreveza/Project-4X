using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HB_EnemySpawner : MonoBehaviour 
{
    SpriteRenderer thisRenderer;
    public Transform spawnPosition;
    public Sprite activeSpawner;
    public Sprite disabledSpawner;
    //public GameObject localLight;

    private void Start()
    {
        thisRenderer = this.GetComponent<SpriteRenderer>();
        SetAsDisabled();
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(spawnPosition.position, 1f);
    }

    public void SetAsActive()
    {
        thisRenderer.sprite = activeSpawner;
        //localLight.SetActive(true);
    }

    public void SetAsDisabled()
    {
        thisRenderer.sprite = disabledSpawner;
        //localLight.SetActive(false);
    }
    public void Spawn(EnemyType type, WaveManager spawner)
    {
        GameObject obj;

        switch (type)
        {
            case EnemyType.LOCUST:
                obj = Instantiate(HB_GameManager.Instance.locustPrefab, spawnPosition.position, Quaternion.identity);
                obj.GetComponent<Entity>().spawner = spawner;
                obj.transform.parent = null;
                break;
        }
    }
}
