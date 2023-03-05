using UnityEngine;
using UnityEngine.UI;

namespace UI {
   public class GameOver : MonoBehaviour {
      [field: SerializeField] public GameObject Panel { get; private set; }

      [SerializeField] private Button _restartButton;
      [SerializeField] private Button _exitButton;

      private void Awake() {
         _restartButton.onClick.AddListener(OnRestartButtonPressed);
         _exitButton.onClick.AddListener(OnExitButtonPressed);
      }

      private void OnDestroy() {
         _restartButton.onClick.RemoveListener(OnRestartButtonPressed);
         _exitButton.onClick.RemoveListener(OnExitButtonPressed);
      }

      private void OnRestartButtonPressed()
         => GameManager.Instance.StartNewRun();

      private void OnExitButtonPressed()
         => GameManager.Instance.ReturnToMenu();     
   }
}
