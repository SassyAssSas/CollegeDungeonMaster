using UnityEngine;
using UI.Elements;

namespace UI {
   public class BarsManager : MonoBehaviour {
      [field: SerializeField] public GameObject Panel { get; private set; }

      [field: SerializeField] public Bar HealthBar { get; private set; }
      [field: SerializeField] public Bar AmmoBar { get; private set; }
   }
}