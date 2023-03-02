using UnityEngine;
using UnityEngine.Tilemaps;

namespace GameSystems.DungeonGeneration {
   [System.Serializable]
   public class SavedTile {
      public SavedTile(Tile tile, Vector2Int position, TilemapLayer layer) {
         Tile = tile;
         Position = position;
         Layer = layer;
      }

      [field: SerializeField] public Tile Tile { get; private set; }
      [field: SerializeField] public Vector2Int Position { get; set; }
      [field: SerializeField] public TilemapLayer Layer { get; private set; }

      public enum TilemapLayer {
         ForegroundWalls = 0,
         BackgroundWalls = 1,
         Decorations = 2,
         Floor = 3,
         Spikes = 4
      }
   }
}