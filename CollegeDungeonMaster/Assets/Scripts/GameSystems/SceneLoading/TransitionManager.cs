using System.Collections;
using UnityEngine;

namespace GameSystems.SceneLoading {
   public class TransitionManager : MonoBehaviour {
      private TransitionManager() { }

      public static TransitionManager Instance { get; private set; }

      private float currentSpeed = 1f;

      private Animator _animator;

      private void Awake() {
         if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);

            _animator = GetComponent<Animator>();
         }
         else {
            Destroy(gameObject);
         }
      }

      public float GetCurrentTransitionDuration()
         => _animator.GetCurrentAnimatorStateInfo(0).length * currentSpeed;

      public void Slide(SlideTransitionType transitionType, float transitionSpeed) {
         var animationName = transitionType.ToString();

         _animator.speed = transitionSpeed;
         _animator.Play(animationName);

         currentSpeed = transitionSpeed; 
      }

      public void CoverScreen(FullTransitionType transitionType, float transitionSpeed) {
         var animationName = transitionType switch {
            FullTransitionType.Fade => "Fade In",

            _ => throw new System.Exception("Unhandled transition type.")
         };

         _animator.speed = transitionSpeed;
         _animator.Play(animationName);

         currentSpeed = transitionSpeed;
      }

      public void UncoverScreen(FullTransitionType transitionType, float transitionSpeed) {
         var animationName = transitionType switch {
            FullTransitionType.Fade => "Fade Out",

            _ => throw new System.Exception("Unhandled transition type.")
         };

         _animator.speed = transitionSpeed;
         _animator.Play(animationName);

         currentSpeed = transitionSpeed;
      }

      public enum FullTransitionType {
         Fade,
      }

      public enum SlideTransitionType {
         SlideLeftToRight,
         SlideRightToLeft,
         SlideTopToBottom,
         SlideBottomToTop
      }
   }
}
