using System.Collections;
using UnityEngine;

public class CollectableEntity : MonoBehaviour {
   [SerializeField] private GameObject _object;

   [field: SerializeField] public CollectableType Type { get; private set; }

   private const float jumpHeight = 0.5f;
   private const float offset = 0.3f;

   private void Awake() {
      StopAllCoroutines();
      StartCoroutine(JumpCoroutine());
      StartCoroutine(Move());
   }

   private IEnumerator JumpCoroutine() {
      var elapsedTime = offset;

      while (elapsedTime < 1f) {
         var position = _object.transform.localPosition;
         position.y = Mathf.Sin(elapsedTime * Mathf.PI) * jumpHeight;

         _object.transform.localPosition = position;

         elapsedTime += Time.deltaTime * 2f;

         yield return null;
      }
   }

   private IEnumerator Move() {
      var angle = Random.Range(0, 360);
      var target = transform.position + Quaternion.Euler(0, 0, angle) * new Vector2(Random.value, 0f);

      var elapsedTime = 0f;
      while (transform.position != target) {
         transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime);  

         elapsedTime += Time.deltaTime;

         yield return null;
      }
   }

   public enum CollectableType {
      Coin = 0,
   }
}
