using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Items.Weapons;

namespace Entities.Items {
   [RequireComponent(typeof(SpriteRenderer))]
   [RequireComponent(typeof(PolygonCollider2D))]
   public class WeaponEntity : MonoBehaviour {
      [field: SerializeField] public Weapon Weapon { get; private set; }

      public bool CanBePicked { get; private set; }

      private SpriteRenderer _sr;
      private PolygonCollider2D _collider;

      private void Awake() {
         _sr = GetComponent<SpriteRenderer>();
         _collider = GetComponent<PolygonCollider2D>();

         Initialize(Weapon);
      }

      public void Initialize(Weapon weapon) {
         Weapon = weapon;

         _sr.sprite = weapon.Sprite;

         _collider.pathCount = _sr.sprite.GetPhysicsShapeCount();

         List<Vector2> path = new();

         for (int i = 0; i < _collider.pathCount; i++) {
            path.Clear();
            _sr.sprite.GetPhysicsShape(i, path);
            _collider.SetPath(i, path.ToArray());
         }

         CanBePicked = false;
         StartCoroutine(AllowPick(0.5f));
      }

      private IEnumerator AllowPick(float delay) {
         yield return new WaitForSeconds(delay);

         CanBePicked = true;
      }
   }
}