using System;
using System.Collections.Generic;
using UnityEngine;

namespace DragynGames.Runtime.Texture
{
    [ExecuteInEditMode]
    public class SelectW : MonoBehaviour
    {
        [Range(0, 256)] public int index;

        private void OnValidate()
        {
            List<Vector3> uvs = new List<Vector3>();
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            mesh.GetUVs(0, uvs);

            for (int i = 0; i < uvs.Count; i++)
            {
                uvs[i] = new Vector3(uvs[i].x, uvs[i].y, index);
            }

            mesh.SetUVs(0, uvs);
        }
    }
}