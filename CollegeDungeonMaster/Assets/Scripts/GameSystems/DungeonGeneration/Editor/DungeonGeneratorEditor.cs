#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameSystems.DungeonGeneration {
   [CustomEditor(typeof(DungeonManager))]
   public class DungeonGeneratorEditor : Editor {
      public override void OnInspectorGUI() {
         DrawDefaultInspector();

         var generator = (DungeonManager)target;
         if (GUILayout.Button("Generate new dungeon"))
            generator.GenerateDungeon();

         if (GUILayout.Button("Clear map"))
            generator.ClearAllTiles();
      }
   }
}
#endif