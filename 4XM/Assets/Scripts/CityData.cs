using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityData
{
    public int level = 0;
    public int playerIndex;
    public List<WorldUnit> availableUnits = new List<WorldUnit>();
    public int range;


    public int output;
    public List<WorldHex> cityHexes = new List<WorldHex>();

}
