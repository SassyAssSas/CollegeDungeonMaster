using UnityEngine;
using Items.Weapons;

namespace Entities.ScriptableObjects.Enemies {
   [CreateAssetMenu(fileName = "New Shooting enemy", menuName = "Entity/Enemy/Shooting enemy")]
   public class Shooter : Enemy {
      [field: SerializeField] public Gun Gun { get; set; }
   }
}
