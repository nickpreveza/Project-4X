using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractable : Interactable
{
    [SerializeField] ItemScriptable scriptable;
    [SerializeField] int amount;
    SpriteRenderer baseSpriteRenderer;
    public string key
    {
        get
        {
            return scriptable.item.key;
        }
    }

    public Sprite sprite
    {
        get
        {
            return scriptable.item.sprite;
        }
    }
    private void Start()
    {
        if (scriptable != null)
        {
            baseSpriteRenderer = GetComponent<SpriteRenderer>();
            if (baseSpriteRenderer != null)
            {
                baseSpriteRenderer.sprite = scriptable.item.sprite;
            }
        }
    }

    public void SetAmount(int _amount)
    {
        amount = _amount;
    }
    public override void Interact()
    {
        ItemManager.Instance.AddToInventory(scriptable.item.key, amount, false);
        base.Interact();
    }

    
}
