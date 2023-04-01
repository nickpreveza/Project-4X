using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Hex 
{
    public Hex(int c, int r)
    {
        this.C = c;
        this.R = r;
        this.S = -(c + r);
    }

    public readonly int C; //Column
    public readonly int R; //Row
    public readonly int S;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;
    public Vector3 Position()
    {
        float radius = 1f;
        float height = radius * 2;
        float width = WIDTH_MULTIPLIER * height;

        float horiz = width; //row
        float vert = height * 0.75f; //column

        return new Vector3(horiz * (this.C + this.R / 2f), 0, vert * this.R);
    }
}
