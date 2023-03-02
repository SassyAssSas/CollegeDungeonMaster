using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSystems.SceneLoading {
   [RequireComponent(typeof(Animator))]
   public class SceneLoader : MonoBehaviour {
      private SceneLoader() { }

      public static SceneLoader Instance { get; private set; }

      private string currentLoadingScene;

      public System.Action OnSceneLoad;

      private bool sceneLoaded = false;

      private void Awake() {
         if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);

            SceneManager.sceneLoaded += OnSceneLoaded;
         }
         else {
            Destroy(gameObject);
         }
      }

      private void OnDestroy() {
         SceneManager.sceneLoaded -= OnSceneLoaded;
      }

      private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
         if (scene.name == currentLoadingScene)
            sceneLoaded = true;
      }

      private bool SceneLoaded() {
         if (sceneLoaded) {
            sceneLoaded = false;
            return true;
         }

         return false;
      }

      public void LoadScene(string sceneName, TransitionManager.FullTransitionType transition, GameManager.GameState gameStateOnLoad) {
         currentLoadingScene = sceneName;

         StartCoroutine(LoadSceneCoroutine(transition, gameStateOnLoad));
      }

      public IEnumerator LoadSceneCoroutine(TransitionManager.FullTransitionType transition, GameManager.GameState gameStateOnLoad) {
         GameManager.Instance.UpdateGameState(GameManager.GameState.Loading);

         TransitionManager.Instance.CoverScreen(transition, 1f);

         yield return new WaitForEndOfFrame();
         yield return new WaitForSecondsRealtime(TransitionManager.Instance.GetCurrentTransitionDuration());

         SceneManager.LoadScene("LoadingScreen");
         SceneManager.LoadScene(currentLoadingScene);

         yield return new WaitUntil(SceneLoaded);

         TransitionManager.Instance.UncoverScreen(transition, 1f);

         OnSceneLoad?.Invoke();

         GameManager.Instance.UpdateGameState(gameStateOnLoad);
      }

      public enum Transition {
         Fade
      }
   }
}
