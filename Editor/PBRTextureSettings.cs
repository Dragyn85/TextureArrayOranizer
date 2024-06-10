using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragynGames.Editor.Texture
{
    [CreateAssetMenu(fileName = "PBRTextureSettings", menuName = "Art/PBRTextureSettings")]
    public class PBRTextureSettings : ScriptableObject
    {
        public string path = "Assets/";
        public string filename = "MyTextureArray";
        
        public List<PBRTextureSet> sets;
        public bool CombinePbrTextures;
        public bool IgnorePBRTexures;
        public bool OutputNormals = true;
    }

    [Serializable]
    public class PBRTextureSet
    {
        public Texture2D Albido;
        public Texture2D Normal;

        public Texture2D Metallic;
        public Texture2D Roughness;
        public Texture2D AO;
        public Texture2D CombinedMetallicRoughnessAmbienOccolusion;
    }
}