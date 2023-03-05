using UnityEngine;
using UnityEngine.InputSystem;
using GameSystems.DungeonGeneration;

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour {
   [SerializeField] private float movementSpeed = 6f;
   [SerializeField] private float movementSnapiness = 0.1f;
   [SerializeField] private float stopSnapiness = 0.2f;

   private Rigidbody2D _rb;
   
   private PlayerInput input;

   private void Awake() {
      input = new();

      _rb = GetComponent<Rigidbody2D>();  
   }

   private void Start() {
      DungeonManager.Instance.OnRoomChange += OnRoomChange;
   }

   private void OnEnable() {
      input.Enable();
   }

   private void OnDisable() {
      input.Disable();
   }

   private void OnDestroy() {
      if (DungeonManager.Instance != null)
         DungeonManager.Instance.OnRoomChange -= OnRoomChange;

      input?.Dispose();
   }

   void FixedUpdate() {
      var movementDirection = input.Player.Movement.ReadValue<Vector2>();

      var xMovement = movementDirection.x == 0
         ? Mathf.Lerp(_rb.velocity.x, movementDirection.x * movementSpeed, stopSnapiness)
         : Mathf.Lerp(_rb.velocity.x, movementDirection.x * movementSpeed, movementSnapiness);

      var yMovement = movementDirection.y == 0
         ? Mathf.Lerp(_rb.velocity.y, movementDirection.y * movementSpeed, stopSnapiness)
         : Mathf.Lerp(_rb.velocity.y, movementDirection.y * movementSpeed, movementSnapiness);

      _rb.velocity = new Vector2(xMovement, yMovement);
   }

   public void OnRoomChange(Room room, RoomFragment fragment, Vector3 relativeDirection) {
      transform.position += relativeDirection;
   }

   public void Enable()
      => input.Enable();

   public void Disable()
      => input.Disable();
}
