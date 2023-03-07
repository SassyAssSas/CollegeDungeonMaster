using GameSystems.DungeonGeneration;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace UI {
   public class Minimap : MonoBehaviour {
      [field: SerializeField] public GameObject Panel { get; private set; }

      [SerializeField] private GameObject _roomsContainer;

      [SerializeField] private Image minimapRoomPrefab;

      [SerializeField] private Sprite visitedRoomSprite;
      [SerializeField] private Sprite notVisitedRoomSprite;
      [SerializeField] private Sprite clearedMark;
      [SerializeField] private Sprite portalMark;

      private Vector2 offset = Vector2.zero;

      private readonly Vector2Int roomSize = new(50, 50);

      private List<MinimapRoom> rooms;

      public void Awake() {
         rooms = new();
         rooms.Clear();
      }

      public void SetActiveRoom(Room room) {
         var minimapRoom = rooms.FirstOrDefault(minimapRoom => minimapRoom.topLeftPosition == room.TopLeftFragment.Position);
         if (minimapRoom == null) {
            Debug.LogWarning("Couldn't find this room on the minimap");
            return;
         }

         var movement = offset - minimapRoom.rectTransform.anchoredPosition;
         foreach (var item in rooms) {
            item.rectTransform.anchoredPosition += movement;
         }

         offset = minimapRoom.rectTransform.anchoredPosition;
      }

      public void AddRoom(Room room, RoomState roomState = RoomState.NotVisited) {
         if (rooms.Any(minimapRoom => minimapRoom.topLeftPosition == room.TopLeftFragment.Position)) {
            Debug.LogWarning("This room is aleady shown on the minimap");
            return;
         }

         minimapRoomPrefab.sprite = roomState switch {
            RoomState.NotVisited => notVisitedRoomSprite,
            RoomState.Visited => visitedRoomSprite,
            _ => throw new System.Exception("Unhandled room state.")
         };

         var size = new Vector2Int(room.Width, room.Height);

         minimapRoomPrefab.rectTransform.sizeDelta = size * roomSize;

         // Scaling room size to the screen size so the rooms will be positioned correctly with different resolutions
         var scaledSize = new Vector2(
            roomSize.x / 640f * Screen.width,
            roomSize.y / 480f * Screen.height
         );

         // Scaling position from real to minimap
         var scaledPosition = new Vector3(
            room.TopLeftFragment.Position.x / DungeonManager.RoomFragmentSize.x * scaledSize.x, 
            room.TopLeftFragment.Position.y / DungeonManager.RoomFragmentSize.y * scaledSize.y
         );

         // Creating an offset for the room depending on its size to position it correctly
         Vector3 sizeOffset = (size - Vector2.one) * scaledSize / 2;
         sizeOffset.y = -sizeOffset.y;

         var minimapRoom = Instantiate(minimapRoomPrefab, _roomsContainer.transform.position + scaledPosition + (Vector3)offset + sizeOffset, new Quaternion(), _roomsContainer.transform);

         rooms.Add(new(minimapRoom, room.TopLeftFragment.Position, roomState));
      }

      public void UpdateRoomState(Vector3Int position, RoomState state) {
         var room = rooms.FirstOrDefault(room => room.topLeftPosition == position);
         if (room is null)
            return;

         room.roomState = state;

         room.image.sprite = state switch {
            RoomState.NotVisited => notVisitedRoomSprite,
            RoomState.Visited => visitedRoomSprite,
            _ => throw new System.Exception("Unhandled room state.")
         };
      }

      private class MinimapRoom {
         public MinimapRoom(Image image, Vector3Int topLeftPosition, RoomState roomState) {
            this.image = image;
            this.topLeftPosition = topLeftPosition;
            this.roomState = roomState;

            rectTransform = image.GetComponent<RectTransform>();
         }

         public Vector3Int topLeftPosition;
         public Image image;
         public RectTransform rectTransform;

         public RoomState roomState;
      }

      public enum RoomState {
         Visited,
         NotVisited,
      }
   }
}