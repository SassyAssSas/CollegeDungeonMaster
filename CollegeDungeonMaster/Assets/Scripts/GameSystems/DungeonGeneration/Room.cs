using System.Collections.Generic;
using System.Linq;

namespace GameSystems.DungeonGeneration {
   public class Room {
      public Room(List<RoomFragment> roomFragments, int width, int height) {
         Fragments = roomFragments;
         Width = width;
         Height = height;

         foreach (var fragment in Fragments)
            fragment.Room = this;

         // Will be optimized. I just don't have time for it rn
         var bottomLeftFragment = Fragments.Aggregate((fragment, next) => fragment = next.Position.x <= fragment.Position.x && next.Position.y <= fragment.Position.y ? next : fragment);
        
         var bottom = bottomLeftFragment.Position.y - DungeonManager.RoomFragmentSize.y / 2;
         var left = bottomLeftFragment.Position.x - DungeonManager.RoomFragmentSize.x / 2;

         var top = bottom + DungeonManager.RoomFragmentSize.y * Height;
         var right = left + DungeonManager.RoomFragmentSize.x * Width;

         Borders = new(top, bottom, right, left);

         TopLeftFragment = Fragments.Aggregate((fragment, next) => fragment = next.Position.x <= fragment.Position.x && next.Position.y >= fragment.Position.y ? next : fragment);
      }

      public List<RoomFragment> Fragments { get; private set; }

      public RoomFragment TopLeftFragment { get; private set; }

      public int Width { get; private set; }
      public int Height { get; private set; }

      public Border Borders { get; private set; }
   }
}