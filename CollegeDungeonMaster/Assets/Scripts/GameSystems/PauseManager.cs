using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour {
   private PauseManager() { }

   public static PauseManager Instance { get; private set; }

   private PlayerInput input;

   [SerializeField] private Canvas _canvas;

   [SerializeField] private Button _resumeButton;
   [SerializeField] private Button _exitButton;

   private bool isPaused = false;

   private void Awake() {
      if (Instance == null) {
         Instance = this;

         input = new();
         input.Player.Pause.performed += OnPausePerformed;

         _resumeButton.onClick.AddListener(OnResumeButtonPressed);
         _exitButton.onClick.AddListener(OnExitButtonPressed);
      }
      else {
         Destroy(gameObject);
      }
   }

   private void OnEnable() {
      input.Enable();
   }

   private void OnDisable() {
      input.Disable();
   }

   private void OnDestroy() {
      _resumeButton.onClick.RemoveListener(OnResumeButtonPressed);
      _exitButton.onClick.RemoveListener(OnExitButtonPressed);

      if (input == null)
         return;
      
      input.Player.Pause.performed -= OnPausePerformed;
      input.Dispose();
   }

   public void OnResumeButtonPressed() {
      Time.timeScale = 1f;

      GameManager.Instance.UpdateGameState(GameManager.GameState.InGame);

      _canvas.gameObject.SetActive(false);

      isPaused = false;
   }

   public void OnExitButtonPressed() {
      GameManager.Instance.ReturnToMenu();

      isPaused = false;
   }
   
   private void OnPausePerformed(InputAction.CallbackContext context) {
      if (isPaused) {
         OnResumeButtonPressed();
      }
      else {
         Time.timeScale = 0f;

         GameManager.Instance.UpdateGameState(GameManager.GameState.Paused);

         _canvas.gameObject.SetActive(true);

         isPaused = true;
      }
   }
}
