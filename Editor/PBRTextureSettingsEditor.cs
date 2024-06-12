using DragynGames.Editor.Texture;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureGroupSettings))]
public class PBRTextureSettingsEditor : Editor
{
    
public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        TextureGroupSettings groupSettings = (TextureGroupSettings) target;
        if (GUILayout.Button("Create Texture Array"))
        {
            PBRTextureArrayCreator.Create(groupSettings);
        }
    }
}
