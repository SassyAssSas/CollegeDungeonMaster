#if UNITY_EDITOR

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using GameSystems.DungeonGeneration.ScriptableObjects;

namespace GameSystems.DungeonGeneration.Development {
   public class TilemapManager : MonoBehaviour {
      [SerializeField] private Tilemap _foregroundWalls, _backgroundWalls, _decorations, _floor, _spikes;
      [SerializeField] private string fileName = "New tilemap";

      [SerializeField] private SavedTilemap mapToMove;
      [SerializeField] private Vector2Int moveDirection = Vector2Int.zero;

      public void Save() {
         var roomElement = ScriptableObject.CreateInstance<SavedTilemap>();

         for (int x = 0; x < 50; x++) {
            for (int y = 0; y >= -50; y--) {
               var tile = _foregroundWalls.GetTile<Tile>(new Vector3Int(x, y));
               if (tile)
                  roomElement.Tiles.Add(new(tile, new(x, y), SavedTile.TilemapLayer.ForegroundWalls));

               tile = _backgroundWalls.GetTile<Tile>(new Vector3Int(x, y));
               if (tile)
                  roomElement.Tiles.Add(new(tile, new(x, y), SavedTile.TilemapLayer.BackgroundWalls));

               tile = _decorations.GetTile<Tile>(new Vector3Int(x, y));
               if (tile)
                  roomElement.Tiles.Add(new(tile, new(x, y), SavedTile.TilemapLayer.Decorations));

               tile = _floor.GetTile<Tile>(new Vector3Int(x, y));
               if (tile)
                  roomElement.Tiles.Add(new(tile, new(x, y), SavedTile.TilemapLayer.Floor));

               tile = _spikes.GetTile<Tile>(new Vector3Int(x, y));
               if (tile)
                  roomElement.Tiles.Add(new(tile, new(x, y), SavedTile.TilemapLayer.Spikes));
            }
         }

         AssetDatabase.CreateAsset(roomElement, $"Assets/Resources/Tilemaps/{fileName}.asset");
      }

      public void MoveSavedTilemap() {
         var roomElement = ScriptableObject.CreateInstance<SavedTilemap>();

         roomElement.Tiles = new(mapToMove.Tiles);

         roomElement.Tiles.ForEach(tile => tile.Position += moveDirection);

         AssetDatabase.CreateAsset(roomElement, $"Assets/Resources/Tilemaps/movedClone.asset");
      }
   }
}

#endif