using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using GameSystems;
using GameSystems.Audio;
using Items.Weapons;
using UI;

public class PlayerAttack : MonoBehaviour {
   [field: SerializeField] public Weapon Weapon { get; private set; }

   [SerializeField] private SpriteRenderer _weaponDisplay;

   private PlayerInput input;

   private delegate IEnumerator AttackAction();
   private AttackAction attackAction;

   private Vector3 mousePosition;

   private bool rotateWeapon = true;
   private bool attackPerformed = false;
   private bool canShoot = true;

   private void Awake() {
      input = new();
      input.Player.Attack.performed += OnAttackPerformed;
      input.Player.Reload.performed += OnReloadPerformed;

      Weapon = Instantiate(Weapon);

      _weaponDisplay.sprite = Weapon.Sprite;

      switch (Weapon) {
         case Gun gun:
            if (gun.BulletsLeft == -1)
               gun.BulletsLeft = gun.Ammo;

            gun.Owner = Gun.GunOwner.Player;

            attackAction = GunAttack;
            break;

         case Sword:
            attackAction = SwordAttack;
            break;

         default:
            throw new System.Exception("Not implemented weapon behaviour.");
      }

      StartCoroutine(attackAction.Method.Name);
   }

   private void Start() {
      Player.Instance.OnPlayerDeath += OnPlayerDeath;
   }

   private void OnEnable() {
      input.Enable();
   }

   private void OnDisable() {
      input.Disable();
   }

   private void OnDestroy() {
      Player.Instance.OnPlayerDeath -= OnPlayerDeath;

      if (input is not null) {
         input.Player.Reload.performed -= OnReloadPerformed;
         input.Player.Attack.performed -= OnAttackPerformed;
         input.Dispose();
      }
   }

   private void Update() {
      if (!rotateWeapon)
         return;

      mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
      mousePosition.z = 0f;
      
      bool flipWeapon = transform.localScale.x == -1f;

      Vector3 offset = new();
      bool usePlayerPosition = false;
      if (Weapon is Gun gun){
         offset = -gun.GunpointOffset.y * _weaponDisplay.transform.up;

         if (Vector2.Distance(transform.position, mousePosition) < gun.GunpointOffset.magnitude * 1.2f)
            usePlayerPosition = true;
      }

      var direction = usePlayerPosition
        ? mousePosition - transform.position
        : mousePosition - _weaponDisplay.transform.position + offset;

      var fromDirection = flipWeapon ? Vector3.left : Vector3.right;

      _weaponDisplay.transform.rotation = Quaternion.FromToRotation(fromDirection, direction);
   }

   public void EnableInput() {
      input.Enable();
      rotateWeapon = true;
   }
  
   public void DisableInput() {
      input.Disable();
      rotateWeapon = false;
   }

   public void SetWeapon(Weapon newWeapon) {
      if (newWeapon == null)
         throw new System.Exception("Weapon was null.");

      Weapon = Instantiate(newWeapon);

      _weaponDisplay.sprite = Weapon.Sprite;

      switch (Weapon) {
         case Gun gun:
            if (gun.BulletsLeft == -1)
               gun.BulletsLeft = gun.Ammo;

            GameUI.Instance.Bars.AmmoBar.SetFillingValue(gun.BulletsLeft / (float)gun.Ammo);
            attackAction = GunAttack;
            break;

         case Sword:
            GameUI.Instance.Bars.AmmoBar.SetFillingValue(1f);
            attackAction = SwordAttack;
            break;

         default:
            throw new System.Exception("Not implemented weapon behaviour.");
      }

      attackPerformed = false;
      canShoot = true;

      StopAllCoroutines();
      StartCoroutine(attackAction.Method.Name);
   }

   private void OnReloadPerformed(InputAction.CallbackContext context) {
      Debug.Log("Reload not implemented yet");
   }

   private void OnAttackPerformed(InputAction.CallbackContext context) {
      if (canShoot)
         attackPerformed = true;
   }

   private void OnPlayerDeath()
      => _weaponDisplay.gameObject.SetActive(false); 

   private bool AttackPerformed() {
      if (attackPerformed) {
         attackPerformed = false;
         return true;
      }

      return false;
   }

   private IEnumerator GunAttack() {
      var gun = Weapon as Gun;

      while (true) {
         if (gun.BulletsLeft == 0) {
            canShoot = false;

            AudioManager.Instance.PlayOneShot("ReloadStarted");

            yield return new WaitForSeconds(gun.ReloadTimeSeconds);
            gun.BulletsLeft = gun.Ammo;

            AudioManager.Instance.PlayOneShot("ReloadFinished");

            GameUI.Instance.Bars.AmmoBar.SetFillingValue(1f);

            canShoot = true;
         }

         yield return new WaitUntil(AttackPerformed);

         var facingModifier = transform.localScale.x > 0f ? 1f : -1f;

         var offset = gun.GunpointOffset * new Vector2(facingModifier, 1f);
         var bulletPosition = _weaponDisplay.transform.position + _weaponDisplay.transform.rotation * offset;

         var startRotation = Quaternion.FromToRotation(Vector3.right * facingModifier, _weaponDisplay.transform.right);
         var angle = gun.BulletsPerShot / 2 * gun.AngleBetweenBullets * facingModifier;

         for (int i = 0; i < gun.BulletsPerShot; i++) {
            var bullet = Instantiate(gun.Bullet);
            bullet.gun = gun;
   
            BulletPooler.Instance.CreateFromPool(bullet, bulletPosition, startRotation * Quaternion.Euler(0f, 0f, angle));
            angle += gun.AngleBetweenBullets * -facingModifier;
         }
         
         AudioManager.Instance.PlayOneShot(gun.ShootSoundName);

         canShoot = false;

         gun.BulletsLeft--;
         GameUI.Instance.Bars.AmmoBar.SetFillingValue(gun.BulletsLeft / (float)gun.Ammo);

         if (gun.BulletsLeft > 0) {
            yield return new WaitForSeconds(gun.ShootCooldownSeconds);
         }

         canShoot = true;
      }
   }

   private IEnumerator SwordAttack() {
      // var sword = Weapon as Sword;
      yield return null;
   }
}
