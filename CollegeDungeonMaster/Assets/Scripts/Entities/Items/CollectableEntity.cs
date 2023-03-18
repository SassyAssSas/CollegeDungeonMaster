using System.Collections;
using UnityEngine;

public class CollectableEntity : MonoBehaviour {
   [SerializeField] private GameObject _object;

   [field: SerializeField] public CollectableType Type { get; private set; }

   private const float jumpHeight = 0.5f;
   private const float jumpSpeed = 2f;
   private const float offset = 0.3f;

   private const float followPlayerDistance = 1.5f;
   private const float followPlayerSpeed = 3f;

   private void Start() {
      StopAllCoroutines();
      StartCoroutine(JumpCoroutine());
      StartCoroutine(Move());
   }

   private void Update() {
      if (Vector2.Distance(Player.Instance.transform.position, transform.position) < followPlayerDistance) {
         transform.position += followPlayerSpeed * Time.deltaTime * (Player.Instance.transform.position - transform.position).normalized;
      }
   }

   private IEnumerator JumpCoroutine() {
      var elapsedTime = offset;

      while (elapsedTime < 1f) {
         var position = _object.transform.localPosition;
         position.y = Mathf.Sin(elapsedTime * Mathf.PI) * jumpHeight;

         _object.transform.localPosition = position;

         elapsedTime += Time.deltaTime * jumpSpeed;

         yield return null;
      }
   }

   private IEnumerator Move() {
      var angle = Random.Range(0, 360);
      var target = transform.position + Quaternion.Euler(0, 0, angle) * new Vector2(Random.value, 0f);

      var elapsedTime = 0f;
      while (Vector2.Distance(transform.position, target) > 0.1f && Vector2.Distance(Player.Instance.transform.position, transform.position) >= followPlayerDistance) {
         transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime);  

         elapsedTime += Time.deltaTime;

         yield return null;
      }
   }

   private void OnCollisionEnter2D(Collision2D collision) {
      int wallLayer = 8;
      if (collision.collider.gameObject.layer == wallLayer)
         StopCoroutine(Move());   
   }

   public enum CollectableType {
      Coin = 0,
   }
}
