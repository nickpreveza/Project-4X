using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridInstance : MonoBehaviour
{
    public static GridInstance Instance;

    [SerializeField] int height;
    [SerializeField] int width;
    [SerializeField] float cellSize;

    public Grid gameGrid;
    // Start is called before the first frame update

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

       gameGrid = new Grid(width, height, cellSize, Vector3.zero);
    }
    public void CreateGrid()
    {
        gameGrid = new Grid(width, height, cellSize, Vector3.zero);
    }

}
