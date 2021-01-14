using UnityEditor;
using UnityEngine;

using System;

namespace wowatp
{
[CustomEditor(typeof(NoiseGenerator))]
    public class TerrainGeneratorEditor: Editor
    {
        public override void OnInspectorGUI()
        {
            NoiseGenerator tg = target as NoiseGenerator;
            if(DrawDefaultInspector())
                tg.GenNoise(tg.Grid, Vector2Int.zero);
            if(GUILayout.Button("GenNoise"))
                tg.GenNoise(tg.Grid, Vector2Int.zero, true);
        }
    }

}