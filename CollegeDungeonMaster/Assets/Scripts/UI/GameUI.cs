using UnityEngine;

namespace UI {
   [RequireComponent(typeof(BarsManager))]
   [RequireComponent(typeof(GameOver))]
   public class GameUI : MonoBehaviour {
      private GameUI() { }

      public static GameUI Instance { get; private set; }

      public BarsManager Bars { get; private set; }
      public GameOver GameOver { get; private set; }

      private void Awake() {
         if (Instance == null) {
            Instance = this;

            Bars = GetComponent<BarsManager>();
            GameOver = GetComponent<GameOver>();
         }
         else {
            Destroy(gameObject);
         }
      }

      private void Start() {
         GameManager.Instance.OnGameStateChange += OnGameStateChange;
      }

      private void OnDestroy() {
         GameManager.Instance.OnGameStateChange -= OnGameStateChange;
      }

      private void OnGameStateChange(GameManager.GameState state) {
         switch (state) {
            case GameManager.GameState.MainMenu:
               break;
         }
      }
   }

}
