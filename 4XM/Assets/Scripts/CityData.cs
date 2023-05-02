using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CityData
{
    public string cityName;

    public int level = 1;
    public int targetLevelPoints = 2;
    public int levelPointsToNext = 0;

    public int negativeLevelPoints = 0;

    public int playerIndex;
    public List<WorldUnit> availableUnits = new List<WorldUnit>();
    public int range;


    public int output;
    public List<WorldHex> cityHexes = new List<WorldHex>();
}
