using System.Collections.Generic;
using UnityEngine;
using Entities.ScriptableObjects.Generic;
using Entities.Controllers;

namespace GameSystems {
   public class BulletPooler : MonoBehaviour {
      private BulletPooler() { }

      public static BulletPooler Instance { get; private set; }

      [SerializeField] private int poolSize;
      private BulletController bulletPrefab;

      private Queue<BulletController> pool;

      private void Awake() {
         if (Instance == null) {
            Instance = this;

            pool = new();

            bulletPrefab = Resources.Load<BulletController>("Prefabs/Bullet");

            for (int i = 0; i < poolSize; i++) {
               var bullet = Instantiate(bulletPrefab);
               bullet.gameObject.SetActive(false);

               pool.Enqueue(bullet);
            }
         }
         else {
            Destroy(gameObject);
         }
      }

      public void CreateFromPool(Bullet bulletSettings, Vector3 position, Quaternion rotation) {
         var controller = pool.Dequeue();

         controller.gameObject.SetActive(true);

         controller.Initialize(bulletSettings, rotation);
         controller.transform.position = position;

         pool.Enqueue(controller);
      }
   }
}