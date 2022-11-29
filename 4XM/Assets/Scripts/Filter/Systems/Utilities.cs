using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities 
{
    public static Vector3 MouseWorldPosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
