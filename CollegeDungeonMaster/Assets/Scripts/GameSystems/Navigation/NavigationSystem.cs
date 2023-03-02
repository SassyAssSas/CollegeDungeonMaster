using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using GameSystems.DungeonGeneration;
using System.Threading.Tasks;
using System.IO;

namespace GameSystems.Navigation {
   public class NavigationSystem : MonoBehaviour {
      private NavigationSystem() { }

      public static NavigationSystem Instance { get; private set; }

      [SerializeField] private Tilemap[] walkableTilemap;
      [SerializeField] private Tilemap[] unwalkableTilemaps;

      private readonly List<Node> nodeMap = new();

      private const float cellSide = 0.5f;

      private readonly Vector3[] directions = new Vector3[] {
            Vector3.up,
            Vector3.down,
            Vector3.left,
            Vector3.right,
            Vector3.up + Vector3.right,
            Vector3.up + Vector3.left,
            Vector3.down + Vector3.right,
            Vector3.down + Vector3.left
         };

      private void Awake() {
         if (Instance == null) {
            Instance = this;
         }
         else {
            Destroy(gameObject);
         }
      }

      private void Start() {
         DungeonManager.Instance.OnRoomChange += OnRoomChange;
      }

      private void OnDestroy() {
         if (DungeonManager.Instance != null)
            DungeonManager.Instance.OnRoomChange -= OnRoomChange;
      }

      private async void OnRoomChange(Room room, RoomFragment roomFragment, Vector3 direction)
         => await SetWorkingRoom(room);

      public async Task SetWorkingRoom(Room room) {
         nodeMap.Clear();

         await AddWorkingRoomAsync(room);
      }

      public async Task AddWorkingRoomAsync(Room room) {
         // DO NOT TOUCH ANY OF THIS 
         List<Node> nodes = new();
         List<Vector3> wallTilePositions = new();
         for (int x = room.Borders.left; x <= room.Borders.right; x += 1) {
            for (int y = room.Borders.top; y >= room.Borders.bottom; y -= 1) {
               if (unwalkableTilemaps.Any(map => map.GetTile(new Vector3Int(x, y))))
                  wallTilePositions.Add(new Vector3(x, y));
            }
         }

         await Task.Run(() => {
            for (int x = room.Borders.left; x <= room.Borders.right; x += 1) {
               for (int y = room.Borders.top; y >= room.Borders.bottom; y -= 1) {
                  bool onUnwalkableTile = wallTilePositions.Contains(new Vector3Int(x, y));

                  nodes.Add(new Node(new Vector3(x, y), float.PositiveInfinity, float.PositiveInfinity, !onUnwalkableTile));

                  foreach (var direction in directions) {
                     if (!onUnwalkableTile && (wallTilePositions.Contains(new Vector3(x, y + direction.y)) || wallTilePositions.Contains(new Vector3(x + direction.x, y)) || wallTilePositions.Contains(new Vector3(x, y) + direction)))
                        continue;

                     var neighbour = new Node(new Vector3(x, y) + direction * cellSide, float.PositiveInfinity, float.PositiveInfinity, !onUnwalkableTile);

                     nodes.Add(neighbour);
                  }
               }
            }
         });

         nodeMap.AddRange(nodes);
      }

      public async Task<List<Vector3>> GeneratePathAsync(Vector3Int startTilePosition, Vector3Int targetTilePosition)
         => await Task.Run(() => GeneratePath(startTilePosition, targetTilePosition));

      public List<Vector3> GeneratePath(Vector3Int startTilePosition, Vector3Int targetTilePosition) {
         var currentNode = nodeMap.FirstOrDefault(node => node.Position == startTilePosition);
         var targetNode = nodeMap.FirstOrDefault(node => node.Position == targetTilePosition);

         if (currentNode is null || targetNode is null) {
            Debug.LogWarning("Invalid nodes.");
            return new List<Vector3>();
         }

         currentNode = currentNode.Clone();
         targetNode = targetNode.Clone();

         if (!currentNode.Walkable || !targetNode.Walkable) {
            return new List<Vector3>();
         }

         // Setting first node at the start position
         currentNode.StartDistance = 0f;
         currentNode.TargetDistance = GetDiagonalDistance(startTilePosition, targetTilePosition);
         currentNode.ParentNode = null;

         List<Node> checkedNodes = new() { currentNode };
         List<Node> discoveredNodes = new() { currentNode };

         while (currentNode.Position != targetNode.Position) {
            foreach (var direction in directions) {
               var neighbourNodePosition = currentNode.Position + direction * cellSide;
               var neighbourNode = nodeMap.FirstOrDefault(node => node.Position == neighbourNodePosition);
               if (neighbourNode is null || !neighbourNode.Walkable)
                  continue;

               var neighbourClone = neighbourNode.Clone();

               // Getting values of this node depending on the way we got to it by
               var startDistance = currentNode.StartDistance + GetDiagonalDistance(currentNode.Position, neighbourClone.Position);
               var targetDistance = GetDiagonalDistance(neighbourClone.Position, targetTilePosition);

               // If our way is better than any others that were built to this node before, we set it as main by putting our values in the node
               if (startDistance + targetDistance < neighbourClone.GetEfficencyValue()) {
                  neighbourClone.StartDistance = startDistance;
                  neighbourClone.TargetDistance = targetDistance;
                  neighbourClone.ParentNode = currentNode;
               }

               // If this node was discovered before we update it, if not we just add it
               var discoveredNode = discoveredNodes.FirstOrDefault(node => node.Position == neighbourClone.Position);
               if (discoveredNode is null)
                  discoveredNodes.Add(neighbourClone);
               else
                  discoveredNode = neighbourClone;
            }

            // Getting the optimal node to continue our path from
            Node optimalNode = discoveredNodes.Aggregate((node, next) => {
               // Optimal node must not be checked before or we will stuck on it forever.
               if (checkedNodes.Any(checkedNode => checkedNode.Position == node.Position))
                  return next;

               if (checkedNodes.Any(checkedNode => checkedNode.Position == next.Position))
                  return node;

               // The lower value is - the more efficient the node is
               var nodeScore = node.GetEfficencyValue();
               var nextScore = next.GetEfficencyValue();

               if (nodeScore == nextScore)
                  return node.TargetDistance < next.TargetDistance ? node : next;

               return nodeScore < nextScore ? node : next;
            });

            // This will happen if we have checked all the avaliable nodes. It means that there is no possible way to reach the target point.
            if (checkedNodes.Any(checkedNode => checkedNode.Position == optimalNode.Position)) {
               return new List<Vector3>();
            }

            checkedNodes.Add(optimalNode);
            currentNode = optimalNode;
         }

         // Getting the path points by checking parent nodes of each node starting from the one that reached the target
         List<Vector3> path = new() { NodeToWorldPosition(currentNode.Position) };
         Node parentNode = currentNode.ParentNode;

         int iterator = 0;
         while (parentNode is not null) {
            if (iterator >= 100) {
               Debug.LogWarning("Prevented infinite loop while path building");
               break;
            }
            iterator++;

            path.Insert(0, NodeToWorldPosition(parentNode.Position));

            parentNode = parentNode.ParentNode;
         }

         return path;
      }

      private float GetDiagonalDistance(Vector3 startPosition, Vector3 targetPosition) {
         var distanceX = Mathf.Abs(startPosition.x - targetPosition.x);
         var distanceY = Mathf.Abs(startPosition.y - targetPosition.y);

         var distance = cellSide * (distanceX + distanceY) + (Mathf.Sqrt(cellSide + cellSide) - 2 * cellSide) * Mathf.Min(distanceX, distanceY);

         return distance;
      }

      private Vector3 NodeToWorldPosition(Vector3 nodePosition)
         => nodePosition + new Vector3(0.5f, 0.5f);
   }
}
