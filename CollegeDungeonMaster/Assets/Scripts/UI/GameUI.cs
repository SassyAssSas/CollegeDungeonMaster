using UnityEngine;

namespace UI {
   [RequireComponent(typeof(BarsManager))]
   [RequireComponent(typeof(GameOver))]
   [RequireComponent(typeof(Minimap))]
   [RequireComponent(typeof(CoinsDisplay))]
   public class GameUI : MonoBehaviour {
      private GameUI() { }

      public static GameUI Instance { get; private set; }

      public BarsManager Bars { get; private set; }
      public GameOver GameOver { get; private set; }
      public Minimap Minimap { get; private set; }
      public CoinsDisplay CoinsDisplay { get; private set; }

      private void Awake() {
         if (Instance == null) {
            Instance = this;

            Bars = GetComponent<BarsManager>();
            GameOver = GetComponent<GameOver>();
            Minimap = GetComponent<Minimap>();
            CoinsDisplay = GetComponent<CoinsDisplay>();
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
               Destroy(gameObject);
               break;
         }
      }
   }
}
