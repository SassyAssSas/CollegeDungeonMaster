#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace GameSystems.DungeonGeneration.Development {
   [CustomEditor(typeof(TilemapManager))]
   public class TilemapManagerEditor : Editor {
      public override void OnInspectorGUI() {
         DrawDefaultInspector();

         var manager = (TilemapManager)target;
         if (GUILayout.Button("Save")) {
            manager.Save();
         }

         if (GUILayout.Button("Move")) {
            manager.MoveSavedTilemap();
         }
      }
   }
}
#endif