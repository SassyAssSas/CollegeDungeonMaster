using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using GameSystems.DungeonGeneration.ScriptableObjects;
using Entities.ScriptableObjects.Enemies;
using Entities.Controllers;
using UI;

namespace GameSystems.DungeonGeneration {
   public class DungeonManager : MonoBehaviour {
      private DungeonManager() { }

      public static DungeonManager Instance { get; private set; }

      public static Vector3Int RoomFragmentSize { get => new(14, 10); }
      public Room CurrentRoom { get; private set; }

      public event System.Action<Room, RoomFragment, Vector3> OnRoomChange;

      #region Room fragments pieces
      [Header("Corners")]
      [Header("Default")]
      [SerializeField] private SavedTilemap topLeftCorner;
      [SerializeField] private SavedTilemap topRightCorner;
      [SerializeField] private SavedTilemap bottomLeftCorner;
      [SerializeField] private SavedTilemap bottomRightCorner;

      [Header("Horizontal flat")]
      [SerializeField] private SavedTilemap topRightHorizontalFlatCorner;
      [SerializeField] private SavedTilemap topLeftHorizontalFlatCorner;
      [SerializeField] private SavedTilemap bottomRightHorizontalFlatCorner;
      [SerializeField] private SavedTilemap bottomLeftHorizontalFlatCorner;

      [Header("Vertical flat")]
      [SerializeField] private SavedTilemap topRightVerticalFlatCorner;
      [SerializeField] private SavedTilemap topLeftVerticalFlatCorner;
      [SerializeField] private SavedTilemap bottomRightVerticalFlatCorner;
      [SerializeField] private SavedTilemap bottomLeftVerticalFlatCorner;

      [Header("Empty")]
      [SerializeField] private SavedTilemap topRightEmptyCorner;
      [SerializeField] private SavedTilemap topLeftEmptyCorner;
      [SerializeField] private SavedTilemap bottomRightEmptyCorner;
      [SerializeField] private SavedTilemap bottomLeftEmptyCorner;

      [Header("Exits")]
      [Header("Opened")]
      [SerializeField] private SavedTilemap topExitOpened;
      [SerializeField] private SavedTilemap bottomExitOpened;
      [SerializeField] private SavedTilemap leftExitOpened;
      [SerializeField] private SavedTilemap rightExitOpened;

      [Header("Blocked")]
      [SerializeField] private SavedTilemap topExitBlocked;
      [SerializeField] private SavedTilemap bottomExitBlocked;
      [SerializeField] private SavedTilemap leftExitBlocked;
      [SerializeField] private SavedTilemap rightExitBlocked;

      [Header("Empty")]
      [SerializeField] private SavedTilemap topExitEmpty;
      [SerializeField] private SavedTilemap bottomExitEmpty;
      [SerializeField] private SavedTilemap leftExitEmpty;
      [SerializeField] private SavedTilemap rightExitEmpty;

      [Header("Other")]
      [SerializeField] private SavedTilemap cleanFloor;

      [Header("Fillings")]
      [SerializeField] private SavedTilemap[] startRoomFillings;
      [SerializeField] private SavedTilemap[] roomFillings1x1;
      [SerializeField] private SavedTilemap[] roomFillings1x2;
      [SerializeField] private SavedTilemap[] roomFillings2x1;
      [SerializeField] private SavedTilemap[] roomFillings2x2;
      #endregion

      [Header("Tilemaps")]
      [SerializeField] private Tilemap _foregroundWalls;
      [SerializeField] private Tilemap _backgroundWalls;
      [SerializeField] private Tilemap _decorations;
      [SerializeField] private Tilemap _floor;
      [SerializeField] private Tilemap _spikes;

      [Header("Tile sprites")]
      [SerializeField] private Tile spikesHidden;
      [SerializeField] private Tile spikesShown;

      [Header("Settings")]
      [SerializeField] private int generationIterations = 3;
      [SerializeField] private int minimumRoomsBeforeDeadEnds = 3;
      [SerializeField] private int maxRoomsBeforeDeadEnds = 6;

      [SerializeField] private EnemyController enemyPrefab;
      [SerializeField] private Enemy[] enemyTypes;

      [SerializeField] private FinishPortal _finishPortal;

      private TilemapCollider2D _spikesCollider;

      private const int maxRoomSideLength = 2;
      private readonly Dictionary<string, Vector3Int> offsets = new() {
         { "TopLeftCorner", new(-7, 4) },
         { "TopRightCorner", new(1, 4) },
         { "BottomLeftCorner", new(-7, -2) },
         { "BottomRightCorner", new(1, -2) },

         { "TopExit", new(-2, 4) },
         { "BottomExit", new(-2, -2) },
         { "LeftExit", new(-7, 1)},
         { "RightExit", new(4, 1)},

         { "Floor", new(-4, 1)},
         { "Filling", new(2, -3)},
      };
      private readonly List<Room> generatedRooms = new();

      private readonly List<Room> visitedRooms = new();
      private Vector3Int currentRoomFragmentPosition = new();
      private int aliveEnemiesCount = 0;

      private const float maxOffCameraDistance = 0.15f;     

      private void Awake() {
         if (Instance == null) {
            Instance = this;

            _spikesCollider = _spikes.GetComponent<TilemapCollider2D>();
         }
         else {
            Destroy(gameObject);
         }
      }

      private void Update() {
         if (generatedRooms.Count == 0)
            return;

         var playerPosition = Player.Instance.transform.position;
         var previousPosition = currentRoomFragmentPosition;
         Vector3Int leaveDirection = Vector3Int.zero;

         // Checking if the player has left the boundaries of the fragment he was in
         if (playerPosition.x > currentRoomFragmentPosition.x + RoomFragmentSize.x / 2 + maxOffCameraDistance)
            leaveDirection = Vector3Int.right;

         if (playerPosition.x < currentRoomFragmentPosition.x - RoomFragmentSize.x / 2 - maxOffCameraDistance)
            leaveDirection = Vector3Int.left;

         if (playerPosition.y > currentRoomFragmentPosition.y + RoomFragmentSize.y / 2 + maxOffCameraDistance)
            leaveDirection = Vector3Int.up;

         if (playerPosition.y < currentRoomFragmentPosition.y - RoomFragmentSize.y / 2 - maxOffCameraDistance)
            leaveDirection = Vector3Int.down;

         currentRoomFragmentPosition += RoomFragmentSize * leaveDirection;

         // Update room if needed
         if (currentRoomFragmentPosition != previousPosition) {

            // If changed room
            if (!CurrentRoom.Fragments.Any(fragment => fragment.Position == currentRoomFragmentPosition)) {
               CurrentRoom = generatedRooms.Where(room => room.Fragments.Any(fragment => fragment.Position == currentRoomFragmentPosition)).FirstOrDefault();
               var currentFragment = CurrentRoom.Fragments.Where(fragment => fragment.Position == currentRoomFragmentPosition).FirstOrDefault();

               OnRoomChange?.Invoke(CurrentRoom, currentFragment, leaveDirection);

               // If haven't visited this room before
               if (!visitedRooms.Any(room => room.Fragments.Any(fragment => fragment.Position == currentRoomFragmentPosition))) {
                  StartBattle();
                  visitedRooms.Add(CurrentRoom);

                  // Summon the portal if all the rooms were visited
                  if (visitedRooms.Count == generatedRooms.Count) 
                     _finishPortal.gameObject.SetActive(true);

                  GameUI.Instance.Minimap.UpdateRoomState(CurrentRoom.TopLeftFragment.Position, Minimap.RoomState.Visited);
               }

               GameUI.Instance.Minimap.SetActiveRoom(CurrentRoom);
            }
         }
      }

      private void StartBattle() {
         _spikesCollider.isTrigger = false;   

         int distanceFromEdge = 5; // Must be at least 2

         List<Vector3Int> avaliablePositions = new();
         List<Vector3Int> spikesPositions = new();

         for (int x = CurrentRoom.Borders.left - 1; x <= CurrentRoom.Borders.right + 1; x++) {
            for (int y = CurrentRoom.Borders.bottom - 1; y <= CurrentRoom.Borders.top + 1; y++) {
               var position = new Vector3Int(x, y);

               // If far enough from the edge (will probably rewrite this)
               if (x >= CurrentRoom.Borders.left + distanceFromEdge && x <= CurrentRoom.Borders.right - distanceFromEdge
                  && y >= CurrentRoom.Borders.bottom + distanceFromEdge && y <= CurrentRoom.Borders.top - distanceFromEdge) {
                  // If on avaliable tile
                  if (_floor.GetTile(position) && !(_foregroundWalls.GetTile(position) || _backgroundWalls.GetTile(position) || _decorations.GetTile(position)))
                     avaliablePositions.Add(position);
               }
               
               Tile spikesTile = _spikes.GetTile<Tile>(position);
               if (spikesTile) {
                  _spikes.SetTile(position, spikesShown);
                  spikesPositions.Add(position);
               }
            }
         }

         var enemiesCount = 2 + Mathf.FloorToInt(CurrentRoom.Fragments.Count * 0.75f);
         aliveEnemiesCount = enemiesCount;

         for (int i = 0; i < enemiesCount; i++) {
            Vector3Int spawnPoint = avaliablePositions.ElementAt(Random.Range(0, avaliablePositions.Count));
            avaliablePositions.Remove(spawnPoint);

            var enemy = Instantiate(enemyPrefab, spawnPoint, new Quaternion());

            var index = Random.Range(0, enemyTypes.Length);
            enemy.Initialize(enemyTypes[index]);

            // Since enemy object will be destroyed we don't need to worry about unsigning
            enemy.OnDeath += () => {
               aliveEnemiesCount--;
               if (aliveEnemiesCount == 0) {
                  foreach (var position in spikesPositions) {
                     _spikes.SetTile(position, spikesHidden);
                  }
                  _spikesCollider.isTrigger = true;
               }
            };
         }
      }

      public IReadOnlyCollection<Room> GetDungeonRooms()
         => generatedRooms.AsReadOnly();

      public void GenerateDungeon() {
         ClearAllTiles();
         generatedRooms.Clear();

         // Generating first 2 rooms with fixed settings
         var startRoomEntrance = (RoomFragment.Exit)(1 << Random.Range(0, 4));
         var startRoomFilling = startRoomFillings[Random.Range(0, startRoomFillings.Length)];
         var startRoom = TryGenerateRoom(Vector3Int.zero, startRoomEntrance, Vector3Int.one, false, startRoomFilling);

         var secondRoomPosition = RoomFragment.ExitToVector3Int(startRoomEntrance) * RoomFragmentSize;
         var secondRoom = TryGenerateRoom(secondRoomPosition, RoomFragment.GetOppositeExit(startRoomEntrance), Vector3Int.one * 2, true);

         Queue<Room> previousIterationRooms = new();
         previousIterationRooms.Enqueue(startRoom);
         previousIterationRooms.Enqueue(secondRoom);

         generatedRooms.Add(startRoom);
         generatedRooms.Add(secondRoom);

         // Marking start room as visited
         GameUI.Instance.Minimap.UpdateRoomState(Vector3Int.zero, Minimap.RoomState.Visited);

         // Generating other rooms
         var exitFlags = System.Enum.GetValues(typeof(RoomFragment.Exit));
         for (int i = 0; i < generationIterations; i++) {
            var count = previousIterationRooms.Count;
            for (int j = 0; j < count; j++) {
               var room = previousIterationRooms.Dequeue();

               foreach (var fragment in room.Fragments) {
                  foreach (RoomFragment.Exit exit in exitFlags) {
                     if (!fragment.Exits.HasFlag(exit))
                        continue;

                     var newRoomPosition = fragment.Position + RoomFragment.ExitToVector3Int(exit) * RoomFragmentSize;

                     if (TryGetRoomFragment(newRoomPosition) is not null)
                        continue;

                     Room newRoom;
                     if (i < generationIterations - 1 && generatedRooms.Count < maxRoomsBeforeDeadEnds) 
                        newRoom = TryGenerateRoom(newRoomPosition, RoomFragment.GetOppositeExit(exit), Vector3Int.one * 2);
                     else
                        newRoom = TryGenerateRoom(newRoomPosition, RoomFragment.GetOppositeExit(exit), randomizeExits: false);

                     if (newRoom is not null) {
                        previousIterationRooms.Enqueue(newRoom);
                        generatedRooms.Add(newRoom);
                     }
                  }
               }
            }
         }

         // Setting public values
         CurrentRoom = generatedRooms.Where(room => room.Fragments.Any(fragment => fragment.Position == Vector3Int.zero)).FirstOrDefault();
         visitedRooms.Add(CurrentRoom);

         OnRoomChange?.Invoke(CurrentRoom, CurrentRoom.Fragments.First(), Vector2.left);
      }

      private Room TryGenerateRoom(Vector3Int position, RoomFragment.Exit entrance, Vector3Int size = new(), bool randomizeExits = true, SavedTilemap filling = null) {
         var roomGrowDirection = -RoomFragment.ExitToVector3Int(entrance);
         if (roomGrowDirection.x == 0)
            roomGrowDirection.x = Random.Range(0, 2) == 0 ? -1 : 1;
         else
            roomGrowDirection.y = Random.Range(0, 2) == 0 ? -1 : 1;

         if (size == Vector3Int.zero) 
            size = new(Random.Range(1, maxRoomSideLength + 1), Random.Range(1, maxRoomSideLength + 1));

         for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
               var fragment = TryGetRoomFragment(position + new Vector3Int(x * 14, y * 10) * roomGrowDirection);

               if (fragment is not null) {
                  if (x == 0 && y == 0) {
                     return null;
                  }   
                  else {
                     size = Vector3Int.one;
                  }
               }
            }
         }

         var roomExitFlags = System.Enum.GetValues(typeof(RoomFragment.Exit));

         List<RoomFragment> roomFragments = new();
         for (int x = 0; x < size.x; x++) {
            for (int y = 0; y < size.y; y++) {
               RoomFragment fragment = new() {
                  Position = position + new Vector3Int(x * 14, y * 10) * roomGrowDirection
               };

               // Getting possible exits for this fragment
               RoomFragment.Exit possibleExits = 0;
               if (x == 0)
                  possibleExits |= roomGrowDirection.x == 1 ? RoomFragment.Exit.Left : RoomFragment.Exit.Right;

               if (y == 0)
                  possibleExits |= roomGrowDirection.y == 1 ? RoomFragment.Exit.Bottom : RoomFragment.Exit.Top;

               if (x == size.x - 1)
                  possibleExits |= roomGrowDirection.x == 1 ? RoomFragment.Exit.Right : RoomFragment.Exit.Left;

               if (y == size.y - 1)
                  possibleExits |= roomGrowDirection.y == 1 ? RoomFragment.Exit.Top : RoomFragment.Exit.Bottom;


               // Filtering exits by checking if they would lead to an already created room
               RoomFragment.Exit guaranteedExits = 0;
               RoomFragment.Exit bannedExits = 0;
               foreach (RoomFragment.Exit exit in roomExitFlags) {
                  var gFragment = TryGetRoomFragment(fragment.Position + RoomFragmentSize * RoomFragment.ExitToVector3Int(exit));
                  if (gFragment is not null) {
                     if (gFragment.Exits.HasFlag(RoomFragment.GetOppositeExit(exit)))
                        guaranteedExits |= exit;
                     else
                        bannedExits |= exit;
                  }
               }

               // Setting exits to the fragment
               fragment.Exits = randomizeExits
                  ? ((RoomFragment.Exit)Random.Range(1, 16) | guaranteedExits) & possibleExits & ~bannedExits
                  : possibleExits & guaranteedExits & ~bannedExits;

               if (x == 0 && y == 0)
                  fragment.Exits |= entrance;

               roomFragments.Add(fragment);

               // Äàëüøå Áîãà íåò. Åñòü òîëüêî Âåëèêèé ÁÈÍÀÐÍÛÉ ÊÒÓËÕÓ
               RoomFragmentSampleType sampleType;
               RoomFragment.Exit blockedExits = ~(~possibleExits | fragment.Exits);
               RoomFragment.Exit emptyExits = ~(~fragment.Exits | possibleExits);

               if (x == 0 && y == 0) {
                  if (x == size.x - 1 && y == size.y - 1) {
                     sampleType = RoomFragmentSampleType.AllWalls;
                  }
                  else if (x != size.x - 1 && y == size.y - 1) {
                     sampleType = roomGrowDirection.x == 1
                        ? RoomFragmentSampleType.BraceLeft
                        : RoomFragmentSampleType.BraceRight;
                  }
                  else if (x == size.x - 1 && y != size.y - 1) {
                     sampleType = roomGrowDirection.y == 1
                        ? RoomFragmentSampleType.BraceBottom
                        : RoomFragmentSampleType.BraceTop;
                  }
                  else {
                     sampleType = entrance switch {
                        RoomFragment.Exit.Top => roomGrowDirection.x == 1
                           ? RoomFragmentSampleType.CornerTopLeft
                           : RoomFragmentSampleType.CornerTopRight,

                        RoomFragment.Exit.Bottom => roomGrowDirection.x == 1
                           ? RoomFragmentSampleType.CornerBottomLeft
                           : RoomFragmentSampleType.CornerBottomRight,

                        RoomFragment.Exit.Left => roomGrowDirection.y == 1
                           ? RoomFragmentSampleType.CornerBottomLeft
                           : RoomFragmentSampleType.CornerTopLeft,

                        RoomFragment.Exit.Right => roomGrowDirection.y == 1
                           ? RoomFragmentSampleType.CornerBottomRight
                           : RoomFragmentSampleType.CornerTopRight,

                        _ => throw new System.Exception("Exit cannot contain multiple flags in this context.")
                     };
                  }
               }
               else if (x == size.x - 1 && y == 0) {
                  if (y == size.y - 1) {
                     sampleType = roomGrowDirection.x == 1
                        ? RoomFragmentSampleType.BraceRight
                        : RoomFragmentSampleType.BraceLeft;
                  }
                  else {
                     sampleType = entrance switch {
                        RoomFragment.Exit.Top => roomGrowDirection.x == 1
                           ? RoomFragmentSampleType.CornerTopRight
                           : RoomFragmentSampleType.CornerTopLeft,

                        RoomFragment.Exit.Bottom => roomGrowDirection.x == 1
                           ? RoomFragmentSampleType.CornerBottomRight
                           : RoomFragmentSampleType.CornerBottomLeft,

                        RoomFragment.Exit.Left => roomGrowDirection.y == 1
                           ? RoomFragmentSampleType.CornerBottomRight
                           : RoomFragmentSampleType.CornerTopRight,

                        RoomFragment.Exit.Right => roomGrowDirection.y == 1
                           ? RoomFragmentSampleType.CornerBottomLeft
                           : RoomFragmentSampleType.CornerTopLeft,

                        _ => throw new System.Exception("Exit cannot contain multiple flags in this context.")
                     };
                  }
               }
               else if (x == 0 && y == size.y - 1) {
                  if (x == size.x - 1) {
                     sampleType = roomGrowDirection.y == 1
                        ? RoomFragmentSampleType.BraceTop
                        : RoomFragmentSampleType.BraceBottom;
                  }
                  else {
                     sampleType = entrance switch {
                        RoomFragment.Exit.Top => roomGrowDirection.x == 1
                           ? RoomFragmentSampleType.CornerBottomLeft
                           : RoomFragmentSampleType.CornerBottomRight,

                        RoomFragment.Exit.Bottom => roomGrowDirection.x == 1
                           ? RoomFragmentSampleType.CornerTopLeft
                           : RoomFragmentSampleType.CornerTopRight,

                        RoomFragment.Exit.Left => roomGrowDirection.y == 1
                           ? RoomFragmentSampleType.CornerTopLeft
                           : RoomFragmentSampleType.CornerBottomLeft,

                        RoomFragment.Exit.Right => roomGrowDirection.y == 1
                           ? RoomFragmentSampleType.CornerTopRight
                           : RoomFragmentSampleType.CornerBottomRight,

                        _ => throw new System.Exception("Exit cannot contain multiple flags in this context.")
                     };
                  }
               }
               else if (x == size.x - 1 && y == size.y - 1) {
                  sampleType = entrance switch {
                     RoomFragment.Exit.Top => roomGrowDirection.x == 1
                        ? RoomFragmentSampleType.CornerBottomRight
                        : RoomFragmentSampleType.CornerBottomLeft,

                     RoomFragment.Exit.Bottom => roomGrowDirection.x == 1
                        ? RoomFragmentSampleType.CornerTopRight
                        : RoomFragmentSampleType.CornerTopLeft,

                     RoomFragment.Exit.Left => roomGrowDirection.y == 1
                        ? RoomFragmentSampleType.CornerTopRight
                        : RoomFragmentSampleType.CornerBottomRight,

                     RoomFragment.Exit.Right => roomGrowDirection.y == 1
                        ? RoomFragmentSampleType.CornerTopLeft
                        : RoomFragmentSampleType.CornerBottomLeft,

                     _ => throw new System.Exception("Exit cannot contain multiple flags in this context.")
                  };
               }
               else if (y > 0 && y < size.y - 1) { // Edges, corridors and middles
                  if (size.x - 1 == 0) {
                     sampleType = RoomFragmentSampleType.CorridorVertical;
                  }
                  else if (x == size.x - 1) {
                     sampleType = roomGrowDirection.x == 1
                        ? RoomFragmentSampleType.EdgeRight
                        : RoomFragmentSampleType.EdgeLeft;
                  }
                  else if (x == 0) { // x == 0
                     sampleType = roomGrowDirection.x == 1
                        ? RoomFragmentSampleType.EdgeLeft
                        : RoomFragmentSampleType.EdgeRight;
                  }
                  else {
                     sampleType = RoomFragmentSampleType.Empty;
                  }
               }
               else {
                  if (size.y - 1 == 0) {
                     sampleType = RoomFragmentSampleType.CorridorHorizontal;
                  }
                  else if (y == size.y - 1) {
                     sampleType = roomGrowDirection.y == 1
                           ? RoomFragmentSampleType.EdgeTop
                           : RoomFragmentSampleType.EdgeBottom;
                  }
                  else if (y == 0) {
                     sampleType = roomGrowDirection.y == 1
                           ? RoomFragmentSampleType.EdgeBottom
                           : RoomFragmentSampleType.EdgeTop;
                  }
                  else {
                     sampleType = RoomFragmentSampleType.Empty;
                  }
               }

               PlaceRoomFragmentSample(sampleType, fragment.Position);

               // Exits
               foreach (RoomFragment.Exit flag in roomExitFlags) {
                  SavedTilemap exit = flag switch {
                     RoomFragment.Exit.Top => topExitEmpty,
                     RoomFragment.Exit.Bottom => bottomExitEmpty,
                     RoomFragment.Exit.Left => leftExitEmpty,
                     RoomFragment.Exit.Right => rightExitEmpty,
                     _ => throw new System.Exception("Unknown exit side.")
                  }; ;

                  var offset = flag switch {
                     RoomFragment.Exit.Top => offsets["TopExit"],
                     RoomFragment.Exit.Bottom => offsets["BottomExit"],
                     RoomFragment.Exit.Left => offsets["LeftExit"],
                     RoomFragment.Exit.Right => offsets["RightExit"],
                     _ => throw new System.Exception("Unknown exit side.")
                  };

                  if (fragment.Exits.HasFlag(flag)) {
                     exit = flag switch {
                        RoomFragment.Exit.Top => topExitOpened,
                        RoomFragment.Exit.Bottom => bottomExitOpened,
                        RoomFragment.Exit.Left => leftExitOpened,
                        RoomFragment.Exit.Right => rightExitOpened,
                        _ => throw new System.Exception("Unknown exit side.")
                     };
                  }
                  if (emptyExits.HasFlag(flag)) {
                     exit = flag switch {
                        RoomFragment.Exit.Top => topExitEmpty,
                        RoomFragment.Exit.Bottom => bottomExitEmpty,
                        RoomFragment.Exit.Left => leftExitEmpty,
                        RoomFragment.Exit.Right => rightExitEmpty,
                        _ => throw new System.Exception("Unknown exit side.")
                     };
                  }
                  if (blockedExits.HasFlag(flag)) {
                     exit = flag switch {
                        RoomFragment.Exit.Top => topExitBlocked,
                        RoomFragment.Exit.Bottom => bottomExitBlocked,
                        RoomFragment.Exit.Left => leftExitBlocked,
                        RoomFragment.Exit.Right => rightExitBlocked,
                        _ => throw new System.Exception("Unknown exit side.")
                     };
                  }

                  PlaceSavedTilemap(exit, fragment.Position + offset);
               }

               // Floor
               PlaceSavedTilemap(cleanFloor, fragment.Position + offsets["Floor"]);
            }
         }

         var room = new Room(roomFragments, size.x, size.y);
         
         // Filling the room with obstacles and decorations
         if (filling == null) {
            filling = (size.x, size.y) switch {
               (1, 1) => roomFillings1x1[Random.Range(0, roomFillings1x1.Length)],
               (1, 2) => roomFillings1x2[Random.Range(0, roomFillings1x2.Length)],
               (2, 1) => roomFillings2x1[Random.Range(0, roomFillings2x1.Length)],
               (2, 2) => roomFillings2x2[Random.Range(0, roomFillings2x2.Length)],
               _ => null
            };
         } 

         if (filling != null) {
            var fillingPosition = new Vector3Int(room.Borders.left, room.Borders.top) + offsets["Filling"];

            PlaceSavedTilemap(filling, fillingPosition);
         }

         // Adding room to the minimap
         GameUI.Instance.Minimap.AddRoom(room);

         return room;
      }

      private RoomFragment TryGetRoomFragment(Vector3Int position) {
         foreach (var room in generatedRooms) {
            var fragment = room.Fragments.FirstOrDefault(fragment => fragment.Position == position);
            if (fragment is not null)
               return fragment;
         }

         return null;
      }

      private void PlaceRoomFragmentSample(RoomFragmentSampleType type, Vector3Int position) {
         var topLeftPosition = position + offsets["TopLeftCorner"];
         var topRightPosition = position + offsets["TopRightCorner"];
         var bottomLeftPosition = position + offsets["BottomLeftCorner"];
         var bottomRightPosition = position + offsets["BottomRightCorner"];

         switch (type) {
            // Corners
            case RoomFragmentSampleType.CornerTopLeft:
               PlaceSavedTilemap(topLeftCorner, topLeftPosition);
               PlaceSavedTilemap(topRightHorizontalFlatCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftVerticalFlatCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightEmptyCorner, bottomRightPosition);
               break;
            case RoomFragmentSampleType.CornerTopRight:
               PlaceSavedTilemap(topRightCorner, topRightPosition);
               PlaceSavedTilemap(topLeftHorizontalFlatCorner, topLeftPosition);
               PlaceSavedTilemap(bottomRightVerticalFlatCorner, bottomRightPosition);
               PlaceSavedTilemap(bottomLeftEmptyCorner, bottomLeftPosition);
               break;
            case RoomFragmentSampleType.CornerBottomLeft:
               PlaceSavedTilemap(bottomRightHorizontalFlatCorner, bottomRightPosition);
               PlaceSavedTilemap(bottomLeftCorner, bottomLeftPosition);
               PlaceSavedTilemap(topLeftVerticalFlatCorner, topLeftPosition);
               PlaceSavedTilemap(topRightEmptyCorner, topRightPosition);
               break;
            case RoomFragmentSampleType.CornerBottomRight:
               PlaceSavedTilemap(bottomLeftHorizontalFlatCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightCorner, bottomRightPosition);
               PlaceSavedTilemap(topRightVerticalFlatCorner, topRightPosition);
               PlaceSavedTilemap(topLeftEmptyCorner, topLeftPosition);
               break;

            // Edges
            case RoomFragmentSampleType.EdgeTop:
               PlaceSavedTilemap(topLeftHorizontalFlatCorner, topLeftPosition);
               PlaceSavedTilemap(topRightHorizontalFlatCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftEmptyCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightEmptyCorner, bottomRightPosition);
               break;
            case RoomFragmentSampleType.EdgeBottom:
               PlaceSavedTilemap(topLeftEmptyCorner, topLeftPosition);
               PlaceSavedTilemap(topRightEmptyCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftHorizontalFlatCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightHorizontalFlatCorner, bottomRightPosition);
               break;
            case RoomFragmentSampleType.EdgeLeft:
               PlaceSavedTilemap(topLeftVerticalFlatCorner, topLeftPosition);
               PlaceSavedTilemap(topRightEmptyCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftVerticalFlatCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightEmptyCorner, bottomRightPosition);
               break;
            case RoomFragmentSampleType.EdgeRight:
               PlaceSavedTilemap(topLeftEmptyCorner, topLeftPosition);
               PlaceSavedTilemap(topRightVerticalFlatCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftEmptyCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightVerticalFlatCorner, bottomRightPosition);
               break;

            // Braces
            case RoomFragmentSampleType.BraceTop:
               PlaceSavedTilemap(topLeftCorner, topLeftPosition);
               PlaceSavedTilemap(topRightCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftVerticalFlatCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightVerticalFlatCorner, bottomRightPosition);
               break;
            case RoomFragmentSampleType.BraceBottom:
               PlaceSavedTilemap(topLeftVerticalFlatCorner, topLeftPosition);
               PlaceSavedTilemap(topRightVerticalFlatCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightCorner, bottomRightPosition);
               break;
            case RoomFragmentSampleType.BraceLeft:
               PlaceSavedTilemap(topLeftCorner, topLeftPosition);
               PlaceSavedTilemap(topRightHorizontalFlatCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightHorizontalFlatCorner, bottomRightPosition);
               break;
            case RoomFragmentSampleType.BraceRight:
               PlaceSavedTilemap(topLeftHorizontalFlatCorner, topLeftPosition);
               PlaceSavedTilemap(topRightCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftHorizontalFlatCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightCorner, bottomRightPosition);
               break;

            // Corridors
            case RoomFragmentSampleType.CorridorHorizontal:
               PlaceSavedTilemap(topLeftHorizontalFlatCorner, topLeftPosition);
               PlaceSavedTilemap(topRightHorizontalFlatCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftHorizontalFlatCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightHorizontalFlatCorner, bottomRightPosition);
               break;
            case RoomFragmentSampleType.CorridorVertical:
               PlaceSavedTilemap(topLeftVerticalFlatCorner, topLeftPosition);
               PlaceSavedTilemap(topRightVerticalFlatCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftVerticalFlatCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightVerticalFlatCorner, bottomRightPosition);
               break;

            // All walls
            case RoomFragmentSampleType.AllWalls:
               PlaceSavedTilemap(topLeftCorner, topLeftPosition);
               PlaceSavedTilemap(topRightCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightCorner, bottomRightPosition);
               break;

            // Empty
            case RoomFragmentSampleType.Empty:
               PlaceSavedTilemap(topLeftEmptyCorner, topLeftPosition);
               PlaceSavedTilemap(topRightEmptyCorner, topRightPosition);
               PlaceSavedTilemap(bottomLeftEmptyCorner, bottomLeftPosition);
               PlaceSavedTilemap(bottomRightEmptyCorner, bottomRightPosition);
               break;
         }
      }

      private void PlaceSavedTilemap(SavedTilemap savedTilemap, Vector3Int position) {
         foreach (var savedTile in savedTilemap.Tiles) {
            var tilemap = savedTile.Layer switch {
               SavedTile.TilemapLayer.ForegroundWalls => _foregroundWalls,
               SavedTile.TilemapLayer.BackgroundWalls => _backgroundWalls,
               SavedTile.TilemapLayer.Decorations => _decorations,
               SavedTile.TilemapLayer.Floor => _floor,
               SavedTile.TilemapLayer.Spikes => _spikes,
               _ => throw new System.Exception("Unhandled tilemap layer.")
            };

            tilemap.SetTile(position + (Vector3Int)savedTile.Position, savedTile.Tile);
         }
      }

      public void ClearAllTiles() {
         _floor.ClearAllTiles();
         _backgroundWalls.ClearAllTiles();
         _decorations.ClearAllTiles();
         _foregroundWalls.ClearAllTiles();
         _spikes.ClearAllTiles();
      }

      private enum RoomFragmentSampleType {
         CornerTopLeft,
         CornerTopRight,
         CornerBottomLeft,
         CornerBottomRight,
         EdgeTop,
         EdgeBottom,
         EdgeLeft,
         EdgeRight,
         BraceTop,
         BraceBottom,
         BraceLeft,
         BraceRight,
         CorridorHorizontal,
         CorridorVertical,
         AllWalls,
         Empty
      }
   }
}