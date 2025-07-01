using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileMapGenerator))]
public class TileMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TileMapGenerator generator = (TileMapGenerator)target;

        if (GUILayout.Button("Generate Map"))
        {
            generator.GenerateMap();
        }
        if (GUILayout.Button("Clear Map"))
        {
            generator.ClearMap();
        }
        if (GUILayout.Button("Set Tiles Setting"))
        {
            generator.SetTilesSetting();
        }
    }
}