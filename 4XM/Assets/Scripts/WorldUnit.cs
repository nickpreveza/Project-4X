using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

public class WorldUnit : MonoBehaviour
{
   
    public Unit data;
    Wiggler wiggler;

    public WorldHex parentHex;

    Vector3 oldPosition;
    Vector3 newPosition;
    Vector3 currentVelocity;
    float smoothTime = 0.5f;
    bool shouldMove;

    

    void Start()
    {
        wiggler = GetComponent<Wiggler>();

        oldPosition = newPosition = this.transform.position;
        SI_EventManager.Instance.onTurnEnded += OnTurnEnded;
    }

    public void SpawnSetup(WorldHex newHex, int playerIndex)
    {
        data.associatedPlayerIndex = playerIndex;

        data.c = newHex.hex.C;
        data.r = newHex.hex.R;
        parentHex = newHex;
        parentHex.UnitIn(this);

        SetInteractable();
    }

    public void SetInteractable()
    {
        data.canBeInteracted = true;
        this.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitActive;
        this.GetComponent<MeshRenderer>().material.color = GameManager.Instance.GetPlayerColor(data.associatedPlayerIndex);
    }

    public void SetUninteractable()
    {
        data.hasMoved = true;
        data.hasAttacked = true;
        data.canBeInteracted = false;

        //TODO: Set to a transparent shader
        this.GetComponent<MeshRenderer>().material = UnitManager.Instance.unitUsed;
        this.GetComponent<MeshRenderer>().material.color = GameManager.Instance.GetPlayerColor(data.associatedPlayerIndex);
    }
    public void OnTurnEnded(int playerIndex)
    {
        //if this is the end of this players turn, reset the checks. 
        if (data.associatedPlayerIndex == playerIndex)
        {
            data.ActionReset();
        }
    }

    public void OnTurnStarted(int playerIndex)
    {
        if (data.associatedPlayerIndex == playerIndex)
        {
            data.ValidateOptions();
        }
    }

    public void Move(WorldHex newHex)
    {
        data.c = newHex.hex.C;
        data.r = newHex.hex.R;

        parentHex.UnitOut(this);

        parentHex = newHex;
        parentHex.UnitIn(this);
       
        transform.SetParent(parentHex.unitParent);

        data.hasMoved = true;

        newPosition = parentHex.hex.PositionFromCamera();
        shouldMove = true;
        SetUninteractable();
        //wiggler?.AnimatedMove(newPosition);

        //check if attack possibled
    }

    private void Update()
    {
        if (shouldMove)
        {
            this.transform.position = Vector3.SmoothDamp(this.transform.position, newPosition, ref currentVelocity, smoothTime);
            
            if (Vector3.Distance(this.transform.localPosition, newPosition) < 0.1)
            {
                shouldMove = false;
                this.transform.position = newPosition;
                currentVelocity = Vector3.zero;
            }
        }
    }

    public void Select()
    {
        wiggler?.Wiggle();
        if (GameManager.Instance.activePlayer.index == data.associatedPlayerIndex)
        {
            if (data.canBeInteracted)
            {
                UnitManager.Instance.SelectUnit(this);
            }
            else
            {
                Debug.Log("This Unit is not interactable");
            }
          
        }
        else
        {
            //TODO: Show information about the unit in the UI
            Debug.Log("This unit belongs to a different player");
        }
       
    }

    public void Deselect()
    {

    }
}
