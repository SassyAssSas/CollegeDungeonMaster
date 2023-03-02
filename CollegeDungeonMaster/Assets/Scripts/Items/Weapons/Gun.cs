using UnityEngine;
using Entities.ScriptableObjects.Generic;

namespace Items.Weapons {
   [CreateAssetMenu(fileName = "New gun", menuName = "Item/Weapon/Gun")]
   public class Gun : Weapon {
      [field: SerializeField] public Bullet Bullet { get; protected set; }
      [field: SerializeField] public Vector2 GunpointOffset { get; protected set; }

      [field: SerializeField] public int BulletsPerShot { get; protected set; }
      [field: SerializeField] public float AngleBetweenBullets { get; protected set; }

      [field: SerializeField] public float ShootCooldownSeconds { get; protected set; }

      [field: SerializeField] public int Ammo { get; protected set; }
      [field: SerializeField] public float ReloadTimeSeconds { get; protected set; }

      [field: SerializeField] public string ShootSoundName { get; protected set; }

      public GunOwner Owner { get; set; } 

      public int BulletsLeft { get; set; } = -1;

      public enum GunOwner {
         Player,
         Enemy
      }
   }
}