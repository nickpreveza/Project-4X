using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    [SerializeField] GameObject unitParent;
    [SerializeField] GameObject tileParent;
    [SerializeField] GameObject[] tilePrefabs;

    //float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);
    private void Start()
    {
      
    }
    public void GenerateMap()
    {
        foreach(Transform child in tileParent.transform)
        {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                GameObject chosenTile = TileSelect(x, y);
                GameObject obj = Instantiate(chosenTile, tileParent.transform.position, Quaternion.identity);
                obj.transform.SetParent(tileParent.transform);
                Vector3 position = new Vector3(x, 0, y);
                obj.transform.position = position;
            }
        }

        SI_EventManager.Instance.OnMapGenerated();
    }

    GameObject TileSelect(int x, int y)
    {
        if (x == 0 || y == 0 || x == mapWidth -1 || y == mapHeight -1)
        {
            return tilePrefabs[(int)TileType.ICE];
        }
        else
        {
            return tilePrefabs[(int)TileType.DEEPSEA];
        }
    }
}


