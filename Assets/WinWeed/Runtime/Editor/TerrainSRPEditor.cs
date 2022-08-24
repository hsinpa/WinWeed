using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Hsinpa.Winweed;
using Hsinpa.Winweed.Uti;
namespace Hsinpa.Winweed.EditorCode
{
    [CustomEditor(typeof(TerrainSRP))]
    public class TerrainSRPEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            TerrainSRP srp = (TerrainSRP)target;
            if (GUILayout.Button("Clear"))
            {
                srp.Dispose();
                EditorUtility.SetDirty(srp);
                return;
            }
        }

    }
}