using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Items;

namespace Entities.ScriptableObjects.Items {
   public class ItemEntity : MonoBehaviour {
      [field: SerializeField] public Item Item { get; private set; }
      public bool CanBeTaken { get; private set; } = false;

      private SpriteRenderer _sr;
      private PolygonCollider2D _collider;

      private void Awake() {
         _sr = GetComponent<SpriteRenderer>();
         _collider = GetComponent<PolygonCollider2D>();

         if (Item is null)
            return;

         _sr.sprite = Item.Sprite;
         UpdateCollder();
         
         StartCoroutine(SetBeTaken());
      }

      public void Initialize(Item item) {
         Item = item;
         
         _sr.sprite = Item.Sprite;
         UpdateCollder();

         StartCoroutine(SetBeTaken());
      }

      private void UpdateCollder() {
         _collider.pathCount = _sr.sprite.GetPhysicsShapeCount();

         List<Vector2> path = new();

         for (int i = 0; i < _collider.pathCount; i++) {
            path.Clear();
            _sr.sprite.GetPhysicsShape(i, path);
            _collider.SetPath(i, path.ToArray());
         }
      }

      private IEnumerator SetBeTaken() {
         yield return new WaitForSecondsRealtime(0.5f);

         CanBeTaken = true;
      }
   }
}

