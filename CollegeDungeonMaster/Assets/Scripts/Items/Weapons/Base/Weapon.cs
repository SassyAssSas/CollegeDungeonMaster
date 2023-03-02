using UnityEngine;

namespace Items.Weapons {
   public abstract class Weapon : Item {
      [field: SerializeField] public int Damage { get; set; }
   }
}