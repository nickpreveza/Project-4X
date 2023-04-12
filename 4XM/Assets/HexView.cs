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

    public void SetData(WorldHex newHex)
    {
        hex = newHex;
        //TODO: Handle avatars

        if (hex.hex.hasCity)
        {
            hexName.text = hex.hex.cityName;

            GenerateUnitButtons();
        }
        else
        {
            hexName.text = SetName(hex.hex.type);

            GenerateResourceButtons();

        }
    }

    void GenerateUnitButtons()
    {


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
