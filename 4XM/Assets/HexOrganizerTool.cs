using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexOrganizerTool : MonoBehaviour
{
    public static HexOrganizerTool Instance;

    [SerializeField] Transform hexParent;

    public int mapRows;
    public int mapColumns;
    public bool ShowCoords;

    [SerializeField] GameObject cityPrefab;
    public GameObject hexBasePrefab;
    public GameObject[] hexVisualPrefabs;

    private WorldHex[,] hexes;
    
    public int numContinents = 2;
    public int continentSpacing = 20;

    public TerrainType[] regions;

    public List<WorldHex> worldHexes = new List<WorldHex>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SetUpHexes()
    {
        ClearHexes();
        worldHexes.Clear();
        hexes = new WorldHex[mapColumns, mapRows];

        for (int column = 0; column < mapColumns; column++)
        {
            for (int row = 0; row < mapRows; row++)
            {
                GameObject prefab = hexBasePrefab;
                GameObject spawnedObject = Instantiate(prefab, hexParent.transform);
                WorldHex newHex = spawnedObject.GetComponent<WorldHex>();

                if (ShowCoords)
                {
                    newHex.UpdateDebugText(string.Format("{0},{1}", column, row));
                }
                else
                {
                    newHex.UpdateDebugText("");
                }

                newHex.HideHighlight();

                newHex.hex.SetData(column, row);

                spawnedObject.transform.position = newHex.hex.Position();
                spawnedObject.transform.SetParent(hexParent.transform);

                hexes[column, row] = newHex;
                worldHexes.Add(newHex);

                newHex.hex.Elevation = 0f;

               
                newHex.UpdateVisuals();

            }
        }

        ElevateArea(5, 5, 4, 1f);

    }

    void ElevateArea(int q, int r, int radius, float elevation)
    {
        Hex centerHex = GetHexAt(q, r).hex;
        WorldHex[] areaHexes = GetHexesWithinRadiusOf(centerHex, radius);

        foreach (WorldHex worldHex in areaHexes)
        {
            if (worldHex.hex.Elevation < 0)
            {
                worldHex.hex.Elevation = 0;
            }
            //we need a way to find distance from center hex 
            worldHex.hex.Elevation += elevation * Mathf.Lerp(1f, 0.25f, Hex.Distance(mapColumns, mapRows, centerHex, worldHex.hex) / radius);
            worldHex.UpdateVisuals();
        }
    }

    public WorldHex[] GetHexesWithinRadiusOf(Hex centerHex, int range)
    {
        List<WorldHex> results = new List<WorldHex>();

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
            {
                results.Add(GetHexAt(centerHex.C + dx, centerHex.R + dy));
            }
        }

        return results.ToArray();
    }

    public WorldHex GetHexAt(int x, int y)
    {
        if (hexes == null)
        {
            Debug.LogError("Hexes Array not yet instantiated.");
            return null;
        }

        if (y >= 0 && y < mapRows)
        {
            x = x % mapColumns;
            if (x < 0)
            {
                x += mapColumns;
            }

            return hexes[x, y];
        }
        else
        {
            Debug.LogWarning("GetHexAt: Value outside of grid bounds");
            return null;
        }
    }

    public void ClearHexes()
    {
#if UNITY_EDITOR
        foreach (Transform child in hexParent)
        {
            DestroyImmediate(child.gameObject);
        }
#endif
        foreach (Transform child in hexParent)
        {
            Destroy(child.gameObject);
        }
    }
}
