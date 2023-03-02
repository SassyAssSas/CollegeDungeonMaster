using System.Collections;
using System.Linq;
using UnityEngine;
using GameSystems;
using GameSystems.DungeonGeneration;

public class CameraMovement : MonoBehaviour {
   [SerializeField] private float movementSpeed;

   private float maxY, minY, maxX, minX;

   private bool followPlayer = true;

   private void Start() {
      DungeonManager.Instance.OnRoomChange += OnRoomChanged;
   }

   private void OnDestroy() {
      if (DungeonManager.Instance is not null)
         DungeonManager.Instance.OnRoomChange -= OnRoomChanged;
   }

   private void LateUpdate() {
      if (!followPlayer)
         return;

      Vector3 targetPosition = new(
         Mathf.Clamp(Player.Instance.transform.position.x, minX, maxX),
         Mathf.Clamp(Player.Instance.transform.position.y, minY, maxY),
         -10f
      );

      transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * movementSpeed);
   }

   private void OnRoomChanged(Room room, RoomFragment fragment, Vector3 leaveDirection) {
      followPlayer = false;

      StopAllCoroutines();

      maxY = room.Borders.top - DungeonManager.RoomFragmentSize.y / 2;
      minY = room.Borders.bottom + DungeonManager.RoomFragmentSize.y / 2;
      maxX = room.Borders.right - DungeonManager.RoomFragmentSize.x / 2;
      minX = room.Borders.left + DungeonManager.RoomFragmentSize.x / 2;

      var targetPosition = fragment.Position;
      targetPosition.z = -10;

      StartCoroutine(MoveToPosition(targetPosition));
   }

   private IEnumerator MoveToPosition(Vector3 targetPosition) {
      float elapsedTime = 0;

      while (transform.position != targetPosition) {
         transform.position = Vector3.Lerp(transform.position, targetPosition, 0.5f * elapsedTime);
         elapsedTime += Time.deltaTime;

         yield return new WaitForEndOfFrame();
      }

      followPlayer = true;
   }
}
