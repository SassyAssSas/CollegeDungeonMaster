using UnityEngine;

namespace Entities.ScriptableObjects.Enemies {
   public abstract class Enemy : ScriptableObject {
      [field: SerializeField] public Sprite Sprite { get; protected set; }
      [field: SerializeField] public float MovementSpeed { get; protected set; }
      [field: SerializeField] public int Health { get; protected set; }

      [field: SerializeField] public RuntimeAnimatorController AnimatorController { get; protected set; }
   }
}

