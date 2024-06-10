using DragynGames.Editor.Texture;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PBRTextureSettings))]
public class PBRTextureSettingsEditor : Editor
{
    
public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        PBRTextureSettings settings = (PBRTextureSettings) target;
        if (GUILayout.Button("Create Texture Array"))
        {
            PBRTextureArrayCreator.Create(settings);
        }
    }
}
