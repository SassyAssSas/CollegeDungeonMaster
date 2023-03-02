using UnityEngine;
using UnityEngine.UI;

namespace UI.Elements {
   public class Bar : MonoBehaviour {
      [SerializeField] private RectTransform bar;

      private RectTransform _rectTransform;

      private float maxBarLength;

      private void Awake() {
         _rectTransform = GetComponent<RectTransform>();

         maxBarLength = _rectTransform.sizeDelta.x;
      }

      /// <summary>
      /// Sets the filling of the bar to the given value. 0 is the empty bar and 1 is the full bar.
      /// </summary>
      /// <param name="value">The value of filling. Clamped between 0 and 1</param>
      public void SetFillingValue(float value) {
         value = Mathf.Clamp01(value);

         bar.sizeDelta = new Vector2(maxBarLength * value, bar.sizeDelta.y);    
      }
   }
}