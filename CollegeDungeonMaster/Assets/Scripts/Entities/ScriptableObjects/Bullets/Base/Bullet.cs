using UnityEngine;
using Items.Weapons;

namespace Entities.ScriptableObjects.Generic {
   public abstract class Bullet : ScriptableObject {  
      [field: SerializeField] public Sprite Sprite { get; protected set; }
      [field: SerializeField] public float MovementSpeed { get; protected set; }
      
      /// <summary>
      /// The gun this bullet was shot from.
      /// </summary>
      public Gun gun;
   }
}