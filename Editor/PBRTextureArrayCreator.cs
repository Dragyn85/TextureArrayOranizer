using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace DragynGames.Editor.Texture
{
    public class PBRTextureArrayCreator 
    {
        private static string path = "Assets/";
        private static string filename = "MyTextureArray";

        public static TextureGroupSettings TextureGroupSettings;

        //private ReorderableList list;
        
        public static void Create(TextureGroupSettings groupSettings)
        {
            TextureGroupSettings = groupSettings;
            OnWizardCreate();
        }
        static void OnWizardCreate()
        {
            path = TextureGroupSettings.path;
            filename = TextureGroupSettings.filename;
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(filename))
            {
                Debug.LogError("You need to enter a path or filename in the texture settings asset.");
                return;
            }
            ExtractAllTexturs();
        }

        private static void ExtractAllTexturs()
        {
            OutputAlbido();
            if (TextureGroupSettings.OutputNormals)
            {
                OutputNormalMaps();
            }

            if (TextureGroupSettings.IgnorePBRTexures)
            {
                OutputSeparatePbr();
            }
            else
            {
                OutputPbrCombo();
            }
        }

        private static void OutputPbrCombo()
        {
            List<Texture2D> combinedTextures = new();
            foreach (var set in TextureGroupSettings.sets)
            {
                combinedTextures.Add(set.CombinedMetallicRoughnessAmbienOccolusion);
            }

            ExportArray(combinedTextures, path, $"{filename}_ORM");
        }

        private static void OutputAlbido()
        {
            List<Texture2D> AlbitoTextures = new List<Texture2D>();
            foreach (var set in TextureGroupSettings.sets)
            {
                AlbitoTextures.Add(set.Albido);
            }

            ExportArray(AlbitoTextures, path, $"{filename}_Albedos");
        }

        private static void OutputNormalMaps()
        {
            List<Texture2D> normalTextures = new List<Texture2D>();
            foreach (var set in TextureGroupSettings.sets)
            {
                normalTextures.Add(set.Normal);
            }

            ExportArray(normalTextures, path, $"{filename}_Normals", true);
        }

        private static void OutputSeparatePbr()
        {
            List<Texture2D> metalicTextures = new List<Texture2D>();
            List<Texture2D> roughnessTextures = new List<Texture2D>();
            List<Texture2D> AmbientOccolusion = new List<Texture2D>();

            foreach (var set in TextureGroupSettings.sets)
            {
                var metTexture = new Texture2D(set.Metallic.width, set.Metallic.height);
                metTexture.SetPixels(set.Metallic.GetPixels(0));
                metalicTextures.Add(metTexture);
                var roughTexture = new Texture2D(set.Roughness.width, set.Roughness.height);
                roughTexture.SetPixels(set.Roughness.GetPixels(0));
                roughnessTextures.Add(roughTexture);
                var AOtexture = new Texture2D(set.AO.width, set.AO.height);
                AOtexture.SetPixels(set.AO.GetPixels(0));
                AmbientOccolusion.Add(AOtexture);
            }


            if (!TextureGroupSettings.CombinePbrTextures)
            {
                ExportArray(metalicTextures, path, $"{filename}_Metallic");
                ExportArray(roughnessTextures, path, $"{filename}_Rooughness");
                ExportArray(AmbientOccolusion, path, $"{filename}_AmbientOccolusion");
            }
            else
            {
                List<Texture2D> combinedTextures = new();
                for (int i = 0; i < TextureGroupSettings.sets.Count; i++)
                {
                    Texture2D combinedMetallicRoughnessAmbienOccolusion = new Texture2D(metalicTextures[0].width,
                        metalicTextures[0].height, TextureFormat.RGBA32, false);
                    for (int x = 0; x < combinedMetallicRoughnessAmbienOccolusion.width; x++)
                    {
                        for (int y = 0; y < combinedMetallicRoughnessAmbienOccolusion.height; y++)
                        {
                            float r = !(AmbientOccolusion[i] == null) ? AmbientOccolusion[i].GetPixel(x, y).r : 0;
                            float g = !(metalicTextures[i] == null) ? metalicTextures[i].GetPixel(x, y).g : 0;
                            float b = !(roughnessTextures[i] == null) ? roughnessTextures[i].GetPixel(x, y).b : 0;
                            float a = 1;
                            combinedMetallicRoughnessAmbienOccolusion.SetPixel(x, y, new Color(r, g, b, a));
                        }
                    }

                    combinedMetallicRoughnessAmbienOccolusion.Apply();
                    combinedTextures.Add(combinedMetallicRoughnessAmbienOccolusion);
                }

                ExportArray(combinedTextures, path, $"{filename}_ORM");
            }
        }

        private static void ExportArray(List<Texture2D> textures, string path, string filename, bool isNormalMap = false)
        {
            if (textures == null || textures.Count == 0)
            {
                Debug.LogError("No textures assigned");
                return;
            }

            Texture2D sample = textures[0];
            TextureFormat format = isNormalMap ? TextureFormat.RGBA32 : sample.format;
            Texture2DArray textureArray =
                new Texture2DArray(sample.width, sample.height, textures.Count, format, false);
            textureArray.filterMode = FilterMode.Trilinear;
            textureArray.wrapMode = TextureWrapMode.Repeat;

            for (int i = 0; i < textures.Count; i++)
            {
                Texture2D tex = textures[i];
                if (isNormalMap)
                {
                    // Convert normal map to a suitable format if necessary
                    tex = ConvertToNormalMap(tex);
                }

                Color[] colors = tex.GetPixels();
                textureArray.SetPixels(colors, i, 0);
            }

            textureArray.Apply();

            string uri = path + filename + ".asset";
            AssetDatabase.CreateAsset(textureArray, uri);
            Debug.Log("Saved asset to " + uri);
        }

        private static Texture2D ConvertToNormalMap(Texture2D source)
        {
            // Ensure the texture is readable
            Texture2D normalMap = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);

            for (int y = 0; y < source.height; y++)
            {
                for (int x = 0; x < source.width; x++)
                {
                    Color color = source.GetPixel(x, y);

                    // Assuming the normal map is in RGB format where R and G represent X and Y directions,
                    // and B represents the Z direction which can be reconstructed.
                    Color normalColor = new Color(color.r * 2.0f - 1.0f, color.g * 2.0f - 1.0f, color.b, color.a);

                    normalMap.SetPixel(x, y, normalColor);
                }
            }

            normalMap.Apply();
            return normalMap;
        }
    }
}