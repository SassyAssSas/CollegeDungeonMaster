using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.DungeonGeneration.ScriptableObjects {
   [CreateAssetMenu(fileName = "New room element", menuName = "Generation/Room element")]
   public class SavedTilemap : ScriptableObject {
      [field: SerializeField] public List<SavedTile> Tiles { get; set; } = new();
   }
}