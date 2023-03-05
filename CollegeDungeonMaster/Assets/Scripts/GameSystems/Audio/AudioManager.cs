using UnityEngine;
using System.Linq;

namespace GameSystems.Audio {
   public class AudioManager : MonoBehaviour {
      private AudioManager() { }

      public static AudioManager Instance { get; private set; }

      [SerializeField] private Sound[] sounds;

      private void Awake() {
         if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this);

            foreach (var sound in sounds) {
               sound.AudioSource = gameObject.AddComponent<AudioSource>();
               sound.AudioSource.clip = sound.AudioClip;
               sound.AudioSource.volume = sound.Volume;
            }
         }
         else {
            Destroy(gameObject);
         }
      }

      public void PlayOneShot(string name) {
         if (!TryGetSoundByName(name, out Sound sound)) {
            Debug.LogError($"Couldn't find a sound named {name}.");
            return;
         }

         sound.AudioSource.PlayOneShot(sound.AudioClip);
      }

      public void Play(string name) {
         if (!TryGetSoundByName(name, out Sound sound)) {
            Debug.LogError($"Couldn't find a sound named {name}.");
            return;
         }
            
         sound.AudioSource.Play();
         sound.AudioSource.loop = true;
      }

      public void Stop(string name) {
         if (!TryGetSoundByName(name, out Sound sound)) {
            Debug.LogError($"Couldn't find a sound named {name}.");
            return;
         }

         sound.AudioSource.Stop();
      }

      public void StopAllSounds() {
         foreach (var sound in sounds) {
            sound.AudioSource.Stop();
         }
      }

      public bool IsPlaying(string name) {
         if (!TryGetSoundByName(name, out Sound sound)) {
            Debug.LogError($"Couldn't find a sound named {name}.");
            return false;
         }

         return sound.AudioSource.isPlaying;
      }

      private bool TryGetSoundByName(string name, out Sound sound) {
         sound = sounds.FirstOrDefault(sound => sound.Name == name);

         return sound is not null;
      }
   }
}
