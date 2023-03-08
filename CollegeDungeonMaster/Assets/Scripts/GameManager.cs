using UnityEngine;
using UnityEngine.InputSystem;
using GameSystems.SceneLoading;
using GameSystems.DungeonGeneration;
using GameSystems.Navigation;
using GameSystems.Audio;

public class GameManager : MonoBehaviour {
   private GameManager() { }

   public static GameManager Instance { get; private set; }

   public delegate void GameManagerStateUpdate(GameState state);
   public event GameManagerStateUpdate OnGameStateChange;

   public delegate void GameManagerAction();
   public event GameManagerAction OnRunStarted;

   public PlayerInput input;

   public GameState CurrentGameState { get; private set; }

   public GameObject[] createOnRunStart;

   private void Awake() {
      if (Instance == null) {
         Instance = this;
         DontDestroyOnLoad(this);

         input = new();
         input.Game.FullscreenToggle.performed += OnFullScreenToggle;
      }
      else {
         Destroy(gameObject);
      }
   }

   private void OnEnable() {
      input?.Enable();
   }

   private void OnDisable() {
      input?.Disable();
   }

   private void OnDestroy() {
      if (input is null)
         return;

      input.Game.FullscreenToggle.performed -= OnFullScreenToggle;
      input.Dispose();
   }

   public void StartNewRun() {
      SceneLoader.Instance.OnSceneLoad += OnSceneLoad;
      SceneLoader.Instance.LoadScene("Dungeon", TransitionManager.FullTransitionType.Fade, GameState.InGame);
      
      void OnSceneLoad() {
         DungeonManager.Instance.GenerateDungeon();
         Time.timeScale = 1f;

         if (!AudioManager.Instance.IsPlaying("DungeonTheme"))
            AudioManager.Instance.Play("DungeonTheme");

         OnRunStarted?.Invoke();

         SceneLoader.Instance.OnSceneLoad -= OnSceneLoad;
      }
   }

   public void ReturnToMenu() {
      SceneLoader.Instance.OnSceneLoad += OnSceneLoad;
      SceneLoader.Instance.LoadScene("MainMenu", TransitionManager.FullTransitionType.Fade, GameState.MainMenu);

      static void OnSceneLoad() {
         Time.timeScale = 1f;

         AudioManager.Instance.StopAllSounds();

         SceneLoader.Instance.OnSceneLoad -= OnSceneLoad;
      }
   }

   public void UpdateGameState(GameState state) {
      CurrentGameState = state;

      OnGameStateChange?.Invoke(state);
   }

   private void OnFullScreenToggle(InputAction.CallbackContext context) {
      if (Screen.fullScreen)
         Screen.SetResolution(640, 480, false);
      else
         Screen.SetResolution(1440, 1080, true);
   }

   public enum GameState {
      MainMenu = 0,
      Loading = 1,
      Paused = 2,
      InGame = 3,
   }
}
