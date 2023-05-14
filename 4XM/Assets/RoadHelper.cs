using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SignedInitiative;

public class RoadHelper : MonoBehaviour
{
    WorldHex parentHex;

    [SerializeField] GameObject center;
    [SerializeField] GameObject rightUp;
    [SerializeField] GameObject right;
    [SerializeField] GameObject rightDown;
    [SerializeField] GameObject leftDown;
    [SerializeField] GameObject left;
    [SerializeField] GameObject leftUp;

    public void SetParent(WorldHex hex)
    {
        parentHex = hex;
    }

    public void SetRoads()
    {
        if (parentHex.hexData.hasRoad)
        {
            center.SetActive(true);
            int roadsEnabled = 0;
            for (int i = 0; i < parentHex.adjacentHexes.Count; i++)
            {
                if (parentHex.adjacentHexes[i].hexData.hasRoad)
                {
                    int adjIndex = parentHex.adjacentHexes[i].hexData.playerOwnerIndex;

                    if (adjIndex == parentHex.hexData.playerOwnerIndex || adjIndex != -1)
                    {
                        Direction dir = MapManager.Instance.GetHexDirection(parentHex, parentHex.adjacentHexes[i]);
                        EnableRoadObjectByDir(dir);
                        parentHex.adjacentHexes[i].AdjacentRoadChanged(parentHex);
                        roadsEnabled++;
                    }
                }
            }

            parentHex.SpawnParticle(GameManager.Instance.resourceHarvestParticle);
        }
       
    }

    public void UpdateByAdjacentHex(WorldHex targetHex)
    {
        Direction dir = MapManager.Instance.GetHexDirection(parentHex, targetHex);
        EnableRoadObjectByDir(dir);
    }

    public void EnableRoadObjectByDir(Direction dir)
    {
        switch (dir)
        {
            case Direction.RightUp:
                rightUp.SetActive(true);
                break;
            case Direction.Right:
                right.SetActive(true);
                break;
            case Direction.RightDown:
                rightDown.SetActive(true);
                break;
            case Direction.LeftDown:
                leftDown.SetActive(true);
                break;
            case Direction.Left:
                left.SetActive(true);
                break;
            case Direction.LeftUp:
                leftUp.SetActive(true);
                break;
        }
    }

    public void DisableRoads()
    {
        foreach(Transform child in this.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}
