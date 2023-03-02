using UnityEngine;
using UI.Elements;

namespace UI {
   public class BarsManager : MonoBehaviour {
      private BarsManager() { }

      public static BarsManager Instance { get; private set; }

      [field: SerializeField] public Bar HealthBar { get; private set; }
      [field: SerializeField] public Bar AmmoBar { get; private set; }

      private void Awake() {
         if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);

            GameManager.Instance.OnGameStateChange += OnGameStateChange;
         }
         else {
            Destroy(gameObject);
         }
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