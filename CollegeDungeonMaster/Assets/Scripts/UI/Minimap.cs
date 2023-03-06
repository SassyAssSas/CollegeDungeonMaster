using GameSystems.DungeonGeneration;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.UI;

namespace UI {
   public class Minimap : MonoBehaviour {
      [field: SerializeField] public GameObject Panel { get; private set; }

      [SerializeField] private GameObject _mask;

      [SerializeField] private Image minimapRoomPrefab;

      [SerializeField] private Sprite visitedRoomSprite;
      [SerializeField] private Sprite notVisitedRoomSprite;
      [SerializeField] private Sprite clearedMark;
      [SerializeField] private Sprite portalMark;

      private Vector3Int offset = Vector3Int.zero;

      private readonly Vector2Int roomSize = new(50, 50);

      private readonly List<MinimapRoom> rooms = new();

      public void MoveMap(Vector3Int direction) {
         foreach (var room in rooms) {
            room.gameObject.transform.position += direction * (Vector3Int)roomSize;
         }

         offset += direction * (Vector3Int)roomSize;
      }

      public void AddRoom(Vector3Int position, Vector2Int size, RoomState roomState = RoomState.NotVisited) {
         if (rooms.Any(room => room.position == position))
            return;

         minimapRoomPrefab.sprite = roomState switch {
            RoomState.NotVisited => notVisitedRoomSprite,
            RoomState.Visited => visitedRoomSprite,
            _ => throw new System.Exception("Unhandled room state.")
         };

         minimapRoomPrefab.rectTransform.sizeDelta = size * roomSize;

         var scaledPosition = new Vector3(position.x / DungeonManager.RoomFragmentSize.x * roomSize.x, position.y / DungeonManager.RoomFragmentSize.y * roomSize.y);
         var sizeOffset = (size - Vector2.one) * roomSize / 2;
         sizeOffset.y = -sizeOffset.y;

         var room = Instantiate(minimapRoomPrefab, _mask.transform.position + scaledPosition + offset + (Vector3)sizeOffset, new Quaternion(), _mask.transform);

         rooms.Add(new(room, position, size, roomState));
      }

      public void UpdateRoomState(Vector3Int position, RoomState state) {
         var room = rooms.FirstOrDefault(room => room.position == position);
         if (room is null)
            return;

         room.roomState = state;

         room.gameObject.sprite = state switch {
            RoomState.NotVisited => notVisitedRoomSprite,
            RoomState.Visited => visitedRoomSprite,
            _ => throw new System.Exception("Unhandled room state.")
         };
      }

      private class MinimapRoom {
         public MinimapRoom(Image gameObject, Vector3Int position, Vector2Int size, RoomState roomState) {
            this.gameObject = gameObject;
            this.position = position;
            this.size = size;
            this.roomState = roomState;
         }

         public void UpdateState(RoomState state) {
            roomState = state;
         }

         public Vector3Int position;
         public Vector2Int size;

         public Image gameObject;

         public RoomState roomState;
      }

      public enum RoomState {
         Visited,
         NotVisited,
      }
   }
}