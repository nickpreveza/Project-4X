using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HexView : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI hexName;
    [SerializeField] Image hexAvatar;

    [SerializeField] Transform horizontalScrollParent;

    [SerializeField] GameObject actionItemPrefab;
    WorldHex hex;
    bool isUnitView;

    public void SetData(WorldHex newHex, WorldUnit newUnit = null)
    {
        foreach(Transform child in horizontalScrollParent)
        {
            Destroy(child.gameObject);
        }

        hex = newHex;

        if (newUnit != null)
        {
            isUnitView = true;
        }
        else
        {
            isUnitView = false;
        }

        if (isUnitView)
        {
            hexName.text = newUnit.data.unitName;
        }
        else
        {
            if (hex.hexData.hasCity)
            {
                hexName.text = hex.hexData.cityName;

                GenerateUnitButtons();
            }
            else
            {
                hexName.text = SetName(hex.hexData.type);

                GenerateResourceButtons();

            }
        }
        
    }

    void GenerateUnitButtons()
    {
       // List<Units>

    }

    void GenerateResourceButtons()
    {

    }


    public string SetName(TileType type)
    {
        switch (type)
        {
            case TileType.DEEPSEA:
                return "Ocean";
            case TileType.SEA:
                return "Sea";
            case TileType.SAND:
                return "Sand";
            case TileType.GRASS:
                return "Grasslands";
            case TileType.HILL:
                return "Hills";
            case TileType.MOUNTAIN:
                return "Mountains";
            case TileType.ICE:
                return "Icecaps";
        }

        return null;
    }

}
