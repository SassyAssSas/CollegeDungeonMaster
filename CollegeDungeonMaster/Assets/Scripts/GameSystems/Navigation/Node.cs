using System.Collections.Generic;
using UnityEngine;

namespace GameSystems.Navigation {
   public class Node {
      public Node(Vector3 position, float startDistance, float targetDistance, bool walkable, List<Node> neighbourNodes = null, Node parentNode = null) {
         Position = position;
         StartDistance = startDistance;
         TargetDistance = targetDistance;
         Walkable = walkable;

         NeighbourNodes = neighbourNodes ?? new();
         ParentNode = parentNode;
      }

      public Vector3 Position { get; set; }

      public float StartDistance { get; set; }
      public float TargetDistance { get; set; }

      public bool Walkable { get; set; }

      public List<Node> NeighbourNodes { get; set; }
      public Node ParentNode { get; set; }

      public float GetEfficencyValue()
         => StartDistance + TargetDistance;

      public Node Clone()
         => new(Position, StartDistance, TargetDistance, Walkable, NeighbourNodes, ParentNode);
   }
}
