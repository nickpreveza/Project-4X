using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMenu : MonoBehaviour
{
    Vector3 prevPos;
    Vector3 posDelta;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            posDelta = Input.mousePosition - prevPos;
            if (Vector3.Dot(transform.up, Vector3.up) >= 0)
            {
                transform.Rotate(transform.up, -Vector3.Dot(posDelta, Camera.main.transform.right), Space.World);
            }
          
            transform.Rotate(Camera.main.transform.right, Vector3.Dot(posDelta, Camera.main.transform.up), Space.World);
        }

        prevPos = Input.mousePosition;
    }
}
