using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SI_CameraController))]
public class CameraControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SI_CameraController cameraController = (SI_CameraController)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Update Bounds"))
        {
            cameraController.UpdateBounds();
        }
    }
}
