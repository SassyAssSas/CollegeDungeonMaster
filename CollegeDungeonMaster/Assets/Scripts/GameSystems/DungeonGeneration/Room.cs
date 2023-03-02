using System.Collections.Generic;
using System.Linq;

namespace GameSystems.DungeonGeneration {
   public class Room {
      public Room(List<RoomFragment> roomFragmentss, int width, int height) {
         Fragments = roomFragmentss;
         Width = width;
         Height = height;

         foreach (var fragment in Fragments)
            fragment.Room = this;

         var bottomLeftFragment = Fragments.Aggregate((fragment, next) => fragment = next.Position.x <= fragment.Position.x && next.Position.y <= fragment.Position.y ? next : fragment);
         var bottom = bottomLeftFragment.Position.y - DungeonManager.RoomFragmentSize.y / 2;
         var left = bottomLeftFragment.Position.x - DungeonManager.RoomFragmentSize.x / 2;

         var top = bottom + DungeonManager.RoomFragmentSize.y * Height;
         var right = left + DungeonManager.RoomFragmentSize.x * Width;

         Borders = new(top, bottom, right, left);
      }

      public List<RoomFragment> Fragments { get; private set; }

      public int Width { get; private set; }
      public int Height { get; private set; }

      public Border Borders { get; private set; }
   }
}