using UnityEngine;
using Entities.ScriptableObjects.Generic;
using Items.Weapons;
using System.Collections;

namespace Entities.Controllers {
   public class BulletController : MonoBehaviour {
      public Bullet Bullet { get; set; }

      [SerializeField] private float maxDistance;

      private SpriteRenderer _sr;

      private delegate void BulletAction();
      private BulletAction bulletBehaviour;

      private float passedDistance;

      private void Awake() {
         _sr = GetComponent<SpriteRenderer>();
      }

      public void Initialize(Bullet bullet, Quaternion rotation) {
         Bullet = bullet;

         _sr.sprite = Bullet.Sprite;
         transform.localRotation = rotation;

         gameObject.layer = bullet.gun.Owner == Gun.GunOwner.Player ? 10 : 11;

         passedDistance = 0;

         switch (Bullet) {
            case DefaultBullet:
               StartCoroutine(DefaultBulletBehaviour());
               break;

            default:
               throw new System.Exception("Not implemented bullet behaviour.");
         }
      }

      private IEnumerator DefaultBulletBehaviour() {
         var bullet = Bullet as DefaultBullet;

         while (true) {
            var movement = bullet.MovementSpeed * Time.deltaTime * transform.right;

            transform.position += movement;

            passedDistance += movement.magnitude;

            if (passedDistance >= maxDistance) {
               gameObject.SetActive(false);
               break;
            }

            yield return null;
         }
      }

      private void OnTriggerEnter2D(Collider2D collision) {
         switch (collision.gameObject.layer) {
            case 8:
               gameObject.SetActive(false);
               break;

            case 6:
               if (Bullet.gun.Owner == Gun.GunOwner.Player) {
                  gameObject.SetActive(false);

                  var controller = collision.GetComponent<EnemyController>();
                  controller.DealDamage(Bullet.gun.Damage);
               }
               break;

            case 3:
               if (Bullet.gun.Owner == Gun.GunOwner.Enemy) {
                  gameObject.SetActive(false);

                  Player.Instance.DealDamage(Bullet.gun.Damage);
               }
               break;
            default:
               break;
         }
      }
   }

}