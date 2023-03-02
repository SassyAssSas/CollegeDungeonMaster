using UnityEngine;

namespace GameSystems.DungeonGeneration {
   public class RoomFragment {
      public Vector3Int Position { get; set; }

      public Exit Exits { get; set; }

      public Room Room { get; set; }

      public const Exit AllExits = Exit.Top | Exit.Bottom | Exit.Left | Exit.Right;

      public static Vector3Int ExitToVector3Int(Exit exit) {
         return exit switch {
            Exit.Top => Vector3Int.up,
            Exit.Bottom => Vector3Int.down,
            Exit.Right => Vector3Int.right,
            Exit.Left => Vector3Int.left,
            _ => throw new System.Exception("Exit cannot contain multiple flags in this context.")
         };
      }

      public static Exit GetOppositeExit(Exit exit) {
         return exit switch {
            Exit.Top => Exit.Bottom,
            Exit.Bottom => Exit.Top,
            Exit.Right => Exit.Left,
            Exit.Left => Exit.Right,
            _ => throw new System.Exception("Exit cannot contain multiple flags in this context.")
         };
      }

      [System.Flags]
      public enum Exit {
         Top = 0b_0001,
         Bottom = 0b_0010,
         Left = 0b_0100,
         Right = 0b_1000,
      }
   }

}