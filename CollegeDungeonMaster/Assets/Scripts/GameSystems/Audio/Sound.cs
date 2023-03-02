using UnityEngine;

namespace GameSystems.Audio {
   [System.Serializable]
   public class Sound {
      [field: SerializeField] public string Name { get; private set; }

      [field: SerializeField] public AudioClip AudioClip { get; private set; }

      [field: SerializeField] public SoundType Type { get; private set; }

      public AudioSource AudioSource { get; set; }
      public float Volume { get; set; } = 0.4f;

      public enum SoundType {
         Music = 0,
         SFX = 1
      }
   }
}