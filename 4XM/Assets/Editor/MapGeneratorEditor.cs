using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapManager))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapManager mapGen = (MapManager)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Generate Map"))
        {
            mapGen.GenerateMap();
        }

        if(GUILayout.Button("Clear Map"))
        {
            mapGen.ClearMap();
        }
    }
}
