using UnityEngine;

namespace Items {
   public abstract class Item : ScriptableObject {
      [field: SerializeField] public Sprite Sprite { get; set; }
   }
}