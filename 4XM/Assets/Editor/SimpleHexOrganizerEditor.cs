using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HexOrganizerTool))]
public class SimpleHexOrganizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        HexOrganizerTool hexOrganizer = (HexOrganizerTool)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Map"))
        {
            hexOrganizer.SetUpHexes();
        }

        if(GUILayout.Button("Clear Map"))
        {
            hexOrganizer.ClearHexes();
        }
    }
}
