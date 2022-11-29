using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestInteractable : Interactable
{
    [SerializeField] string rewardItemKey;
    [SerializeField] bool rewardVisibility;
    SpriteRenderer visibleSprite;
    GameObject itemToDrop;
    private void Start()
    {
        HB_GameManager.Instance.onDataReady += Setup;
    }

    private void OnDestroy()
    {
        HB_GameManager.Instance.onDataReady -= Setup;
    }

    void Setup()
    {
        itemToDrop = ItemManager.Instance.GetWorlditem(rewardItemKey);
        if (itemToDrop != null)
        {
            visibleSprite = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
            if (rewardVisibility)
            {
                visibleSprite.sprite = itemToDrop.GetComponent<ItemInteractable>().sprite;
            }
            else
            {
                visibleSprite.sprite = null;
            }

            isInteractable = true;
        }
        else
        {
            Debug.LogWarning("Chest self-destoryed because the item key was not found in ItemManager");
            Destroy(this.gameObject);
        }
       
    }
    public override void Interact()
    {
        Instantiate(itemToDrop, transform.position, Quaternion.identity);
        base.Interact();
    }

}
