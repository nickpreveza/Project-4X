using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUnit : MonoBehaviour
{
    int x;
    int y;
    public WorldHex parentTile;
    [SerializeField] Unit data;
    Wiggler wiggler;

    Vector3 oldPosition;
    Vector3 newPosition;
    Vector3 currentVelocity;
    float smoothTime = 0.5f;
    bool shouldMove;
    void Start()
    {
        wiggler = GetComponent<Wiggler>();
    }

    public void SetData(int tX, int tY, WorldHex tTile)
    {
        x = tX;
        y = tY;
        parentTile = tTile;
        data.movePossible = true;
    }

    public void Move(WorldHex hex)
    {
        x = hex.hex.C;
        y = hex.hex.R;
        parentTile.UnitOut(this);
        parentTile = hex;
        parentTile.UnitIn(this.gameObject);
        data.movePossible = false;

        newPosition = Vector3.zero;
        shouldMove = true;
        //wiggler?.AnimatedMove(parentTile.transform.localPosition);
        //check if attack possibled
    }

    private void Update()
    {
        if (shouldMove)
        {
            this.transform.localPosition = Vector3.SmoothDamp(this.transform.localPosition, newPosition, ref currentVelocity, smoothTime);
            
            if (Vector3.Distance(this.transform.localPosition, newPosition) < 0.05)
            {
                shouldMove = false;
                this.transform.localPosition = newPosition;
                currentVelocity = Vector3.zero;
            }
        }
    }

    public void Select()
    {
        wiggler?.Wiggle();
        UnitManager.Instance.SelectUnit(this);
    }

    public void Deselect()
    {

    }
}
