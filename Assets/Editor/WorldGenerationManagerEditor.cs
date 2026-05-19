using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldGenerationManager))]
public class WorldGenerationManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10f);

        WorldGenerationManager generator = (WorldGenerationManager)target;

        if (GUILayout.Button("Auto Fill Project World Assets"))
        {
            Undo.RecordObject(generator, "Auto Fill World Assets");
            generator.AutoFillProjectWorldAssets();
        }

        if (GUILayout.Button("Generate World"))
        {
            generator.GenerateWorld();
        }

        if (GUILayout.Button("Clear Generated World"))
        {
            generator.ClearGeneratedWorld();
        }
    }
}
