using UnityEngine;
using Entities.ScriptableObjects.Items;
using Items.Weapons;
using GameSystems.Audio;

public class PlayerInventory : MonoBehaviour {
   private PlayerInput input;

   [SerializeField] private ItemEntity groundItemPrefab;

   private AudioClip gunEquipSound;
   private AudioClip swordEquipSound;

   private void Awake() {
      input = new();

      gunEquipSound = Resources.Load<AudioClip>("Sounds/Effects/Collecting/GunEquip");
      swordEquipSound = Resources.Load<AudioClip>("Sounds/Effects/Collecting/SwordEquip");
   }

   private void OnEnable() {
      input.Enable();
   }

   private void OnDisable() {
      input.Disable();
   }

   private void OnDestroy() {
      if (input is null)
         return;

      input.Dispose();
   }

   private void OnTriggerEnter2D(Collider2D collision) {
      if (collision.gameObject.layer != 9)
         return;

      var groundItem = collision.GetComponent<ItemEntity>();
      if (!groundItem.CanBeTaken)
         return;

      if (groundItem.Item is Weapon weapon) {
         var currentWeapon = Instantiate(Player.Instance.Attack.Weapon);

         var droppedWeapon = Instantiate(groundItemPrefab, collision.transform.position, Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f)));
         droppedWeapon.Initialize(currentWeapon);

         Player.Instance.Attack.SetWeapon(weapon);

         if (weapon is Gun)
            AudioManager.Instance.PlayOneShot("GunEquip");
         else
            AudioManager.Instance.PlayOneShot("SwordEquip");
      }

      Destroy(collision.gameObject);
   }
}
