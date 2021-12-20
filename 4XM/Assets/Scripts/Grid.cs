using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid 
{
    private int width;
    private int height;
    private int[,] gridArray;

    public Grid(int x, int y)
    {
        this.width = x;
        this.height = y;

        gridArray = new int[width, height];
    }
}
