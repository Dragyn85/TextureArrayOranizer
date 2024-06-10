using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace DragynGames.Editor.Texture
{
    public class PBRTextureArrayCreator : ScriptableWizard
    {
        [MenuItem("Window/PBR Texture Array Creator")]
        public static void ShowWindow()
        {
            DisplayWizard<PBRTextureArrayCreator>("Create Texture Array", "Build Asset");
        }

        public string path = "Assets/";
        public string filename = "MyTextureArray";

        public PBRTextureSettings textureSettingss;

        private ReorderableList list;

        void OnWizardCreate()
        {
            ExtractAlbido();
        }

        private void ExtractAlbido()
        {
            OutputAlbido();
            if (textureSettingss.OutputNormals)
            {
                OutputNormalMaps();
            }

            if (textureSettingss.IgnorePBRTexures)
            {
                OutputSeparatePbr();
            }
            else
            {
                OutputPbrCombo();
            }
        }

        private void OutputPbrCombo()
        {
            List<Texture2D> combinedTextures = new();
            foreach (var set in textureSettingss.sets)
            {
                combinedTextures.Add(set.CombinedMetallicRoughnessAmbienOccolusion);
            }

            CompileArray(combinedTextures, path, $"PBR_{filename}");
        }

        private void OutputAlbido()
        {
            List<Texture2D> AlbitoTextures = new List<Texture2D>();
            foreach (var set in textureSettingss.sets)
            {
                AlbitoTextures.Add(set.Albido);
            }

            CompileArray(AlbitoTextures, path, $"Albido_{filename}");
        }

        private void OutputNormalMaps()
        {
            List<Texture2D> normalTextures = new List<Texture2D>();
            foreach (var set in textureSettingss.sets)
            {
                normalTextures.Add(set.Normal);
            }

            CompileArray(normalTextures, path, $"Normal_{filename}", true);
        }

        private void OutputSeparatePbr()
        {
            List<Texture2D> metalicTextures = new List<Texture2D>();
            List<Texture2D> roughnessTextures = new List<Texture2D>();
            List<Texture2D> AmbientOccolusion = new List<Texture2D>();

            foreach (var set in textureSettingss.sets)
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


            if (!textureSettingss.CombinePbrTextures)
            {
                CompileArray(metalicTextures, path, $"Metallic_{filename}");
                CompileArray(roughnessTextures, path, $"Roughness_{filename}");
                CompileArray(AmbientOccolusion, path, $"AO_{filename}");
            }
            else
            {
                List<Texture2D> combinedTextures = new();
                for (int i = 0; i < textureSettingss.sets.Count; i++)
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
                    //string uri = path + filename + ".asset";
                    //AssetDatabase.CreateAsset(combinedMetallicRoughnessAmbienOccolusion, uri);
                    //Debug.Log("Saved asset to " + uri);
                }

                CompileArray(combinedTextures, path, $"PBR_{filename}");
            }
        }

        /*private void CompileArray(List<Texture2D> textures, string path, string filename)
        {
            if (textures == null || textures.Count == 0)
            {
                Debug.LogError("No textures assigned");
                return;
            }

            Texture2D sample = textures[0];
            Texture2DArray textureArray =
                new Texture2DArray(sample.width, sample.height, textures.Count, sample.format, false);
            textureArray.filterMode = FilterMode.Trilinear;
            textureArray.wrapMode = TextureWrapMode.Repeat;

            for (int i = 0; i < textures.Count; i++)
            {
                Texture2D tex = textures[i];
                Color[] colors = tex.GetPixels();
                textureArray.SetPixels(colors, i, 0);
            }

            textureArray.Apply();

            string uri = path + filename + ".asset";
            AssetDatabase.CreateAsset(textureArray, uri);
            Debug.Log("Saved asset to " + uri);
        }*/
        private void CompileArray(List<Texture2D> textures, string path, string filename, bool isNormalMap = false)
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

        private Texture2D ConvertToNormalMap(Texture2D source)
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