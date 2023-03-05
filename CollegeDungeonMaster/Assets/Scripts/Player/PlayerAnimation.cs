using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class PlayerAnimation : MonoBehaviour {
   private Animator _animator;

   private PlayerInput input;

   private bool rotatePlayer = true;

   private void Awake() {
      input = new();
      input.Player.Movement.performed += OnMovementPerformed;
      input.Player.Movement.canceled += OnMovementCanceled;

      _animator = GetComponent<Animator>();
      _animator.Play("Idle");
   }

   private void Start() {
      Player.Instance.OnPlayerGotHit += OnPlayerHit;
   }

   private void OnEnable() {
      input.Enable();
   }

   private void OnDisable() {
      input.Disable();
   }

   private void OnDestroy() {
      Player.Instance.OnPlayerGotHit -= OnPlayerHit;

      if (input is not null) {
         input.Player.Movement.performed -= OnMovementPerformed;
         input.Player.Movement.canceled -= OnMovementCanceled;
         input.Dispose();
      }
   }

   private void Update() {
      if (!rotatePlayer)
         return;

      var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      mousePosition.z = 0f;

      var playerXScale = (mousePosition - transform.position).x > 0 ? 1 : -1f;

      if (transform.localScale.x != playerXScale)
         transform.localScale = new Vector3(playerXScale, 1f, 1f);
   }

   public void Enable() {
      input.Enable();
      rotatePlayer = true;
   }

   public void Disable() {
      input.Disable();
      rotatePlayer = false;
   }

   public void OnPlayerHit() {
      _animator.Play("Hit");
   }

   public void OnPlayerDeath() {
      Debug.Log("No death animation");
   }

   private void OnMovementPerformed(InputAction.CallbackContext context) {
      _animator.Play("Walking");
   }

   private void OnMovementCanceled(InputAction.CallbackContext context) {
      _animator.Play("Idle");
   }
}
