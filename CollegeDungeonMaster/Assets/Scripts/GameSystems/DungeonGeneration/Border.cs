namespace GameSystems.DungeonGeneration {
   public class Border {
      public Border() { }

      public Border(int top, int bottom, int right, int left) {
         this.top = top;
         this.bottom = bottom;
         this.right = right;
         this.left = left;
      }

      public int top;
      public int bottom;
      public int right;
      public int left;
   }
}
