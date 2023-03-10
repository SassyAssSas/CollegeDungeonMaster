using UnityEngine;
using Entities.Items;
using Items.Weapons;
using GameSystems.Audio;
using UI;

public class PlayerInventory : MonoBehaviour {
   [SerializeField] private WeaponEntity groundItemPrefab;

   public int Coins { get; set; }

   private void OnTriggerEnter2D(Collider2D collision) {
      switch (collision.gameObject.layer) {
         case 7:
            var collectableEntity = collision.gameObject.GetComponent<CollectableEntity>();
            CollectItem(collectableEntity);
            break;

         case 9:
            var weaponEntity = collision.GetComponent<WeaponEntity>();
            if (weaponEntity.CanBePicked)
               EquipWeapon(weaponEntity);            
            break;
      }
   }

   private void CollectItem(CollectableEntity collectableEntity) {
      switch (collectableEntity.Type) {
         case CollectableEntity.CollectableType.Coin:
            Coins++;
            GameUI.Instance.CoinsDisplay.SetCoins(Coins);
            break;
         default:
            throw new System.Exception("Unhandled collectable type");
      }

      Destroy(collectableEntity.gameObject);
   }

   private void EquipWeapon(WeaponEntity weaponEntity) {
      var currentWeapon = Player.Instance.Attack.Weapon;

      Player.Instance.Attack.SetWeapon(weaponEntity.Weapon);

      var equipSound = weaponEntity.Weapon switch {
         Gun => "GunEquip",
         Sword => "SwordEquip",
         _ => throw new System.Exception("Unhandled weapon type")
      };
      AudioManager.Instance.PlayOneShot(equipSound);

      weaponEntity.Initialize(currentWeapon);
      weaponEntity.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));
   }
}
