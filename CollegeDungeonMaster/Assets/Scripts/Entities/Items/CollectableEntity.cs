using UnityEngine;

public class CollectableEntity : MonoBehaviour {
   [field: SerializeField] public CollectableType Type { get; private set; }

   public enum CollectableType {
      Coin = 0,
   }
}
