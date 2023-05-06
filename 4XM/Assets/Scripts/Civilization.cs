using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Civilization
{
    public string name;
    public UnitData[] unitOverrides;
    public Color unitColor;
    public Color uiColorActive;
    public Color uiColorInactive;
    public Color borderColor;

    public Dictionary<UnitType, UnitData> unitDictionary = new Dictionary<UnitType, UnitData>();


    public void SetupUnitDictionary()
    {
        foreach(UnitData unit in unitOverrides)
        {
            unitDictionary.Add(unit.type, unit);
        }
    }

    public UnitData GetUnitOverride(UnitType type)
    {
        return unitDictionary[type];
    }
}

public enum Civilizations
{
    Greeks,
    Romans,
    Egyptians,
    Celts,
    None
}
