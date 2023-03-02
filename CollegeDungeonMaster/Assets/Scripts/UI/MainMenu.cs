using UnityEngine;
using UnityEngine.UI;

namespace UI {
   public class MainMenu : MonoBehaviour {
      private MainMenu() { }

      public static MainMenu Instance { get; private set; }

      [SerializeField] private Button _playButton;
      [SerializeField] private Button _optionsButton;
      [SerializeField] private Button _exitButton;

      private void Awake() {
         if (Instance == null) {
            Instance = this;

            _playButton.onClick.AddListener(OnPlayButtonPressed);
            _optionsButton.onClick.AddListener(OnOptionsButtonPressed);
            _exitButton.onClick.AddListener(OnExitButtonPressed);
         }
         else {
            Destroy(gameObject);
         }
      }

      private void OnDestroy() {
         _playButton.onClick.RemoveListener(OnPlayButtonPressed);
         _optionsButton.onClick.RemoveListener(OnOptionsButtonPressed);
         _exitButton.onClick.RemoveListener(OnExitButtonPressed);
      }

      public void OnPlayButtonPressed() {
         _playButton.onClick.RemoveListener(OnPlayButtonPressed);

         GameManager.Instance.StartNewRun();
      }

      public void OnOptionsButtonPressed() {
         Debug.Log("Behaviour is not implemented.");
      }

      public void OnExitButtonPressed() 
         => Application.Quit();
   }
}
