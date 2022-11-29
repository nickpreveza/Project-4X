using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingDoorInteractable : Interactable
{
    [SerializeField] float distance = 0.1f;
    [SerializeField] float openPositionXOffset = -2f;
    [SerializeField] bool isOpen;
    
    [SerializeField] bool moving;
    Vector3 targetPos;
    Vector3 openPos;
    Vector3 lockPos;
   
    void Start()
    {
        lockPos = transform.position;
        openPos = transform.position + new Vector3(openPositionXOffset, 0, 0);
    }

    public override void Interact()
    {
        if (moving)
        {
            return;
        }
        if (isOpen)
        {
            Close();
        }
        else
        {
            Open();
        }
        base.Interact();
    }

    private void Update()
    {

        if (moving)
        {
           if (isOpen)
           {

                transform.position = Vector3.MoveTowards(transform.position, openPos, distance);
                if (transform.position == openPos)
                {
                    moving = false;
                }
              
           }
           else
           {
                transform.position = Vector3.MoveTowards(transform.position, lockPos, distance);
                if (transform.position == lockPos)
                {
                    moving = false;
                }
            }
        }
    }
    public void Unlock(bool open)
    {
        isInteractable = true;
        if (open)
        {
            Open();
            Lock(false);
        }
    }

    public void Lock(bool close)
    {
        isInteractable = false;
        if (close)
        {
            Close();
        }
    }

    void Open()
    {
        isOpen = true;
        moving = true;
    }

    void Close()
    {
        isOpen = false;
        moving = true;
    }
}
