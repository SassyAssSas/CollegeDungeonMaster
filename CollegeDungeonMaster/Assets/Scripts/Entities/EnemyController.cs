using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities.ScriptableObjects.Enemies;
using GameSystems.Navigation;
using GameSystems.Audio;
using GameSystems;

namespace Entities.Controllers {
   [RequireComponent(typeof(CircleCollider2D))]
   [RequireComponent(typeof(Rigidbody2D))]
   public class EnemyController : MonoBehaviour {
      [SerializeField] private Enemy enemy;

      [SerializeField] private SpriteRenderer _weaponDisplay;

      private delegate IEnumerator EnemyBehaviour();
      private EnemyBehaviour enemyBehaviour;

      public delegate void EnemyAction();
      public event EnemyAction OnDeath;

      private Rigidbody2D _rb;
      private SpriteRenderer _sr;
      private Animator _animator;

      private int health;

      private bool justGotHit;

      public void Initialize(Enemy enemy) {
         if (enemy == null) {
            Debug.LogError("Enemy type was null.");
            Destroy(gameObject);
         }

         _rb = GetComponent<Rigidbody2D>();
         _sr = GetComponent<SpriteRenderer>();
         _animator = GetComponent<Animator>();

         this.enemy = Instantiate(enemy);

         switch (enemy) {
            case Shooter shooter:
               _sr.sprite = shooter.Sprite;
               _weaponDisplay.sprite = shooter.Gun.Sprite;

               enemyBehaviour += ShooterBehaviour;
               StartCoroutine(ShooterBehaviour());
               break;

            default:
               throw new System.Exception("Not implemented enemy behaviour.");
         }

         health = enemy.Health;
      }

      public void Start() {
         Player.Instance.OnPlayerDeath += OnPlayerDeath;
      }

      public void OnDestroy() {
         Player.Instance.OnPlayerDeath -= OnPlayerDeath;
      }

      public void DealDamage(int damage) {
         if (health <= 0)
            return;

         health -= damage;
         justGotHit = true;

         StopAllCoroutines();
         StartCoroutine(HitBehaviour());
      }

      private IEnumerator ShooterBehaviour() {
         var shooter = enemy as Shooter;

         shooter.Gun = Instantiate(shooter.Gun);
         shooter.Gun.Owner = Items.Weapons.Gun.GunOwner.Enemy;

         yield return new WaitForEndOfFrame();

         StartCoroutine(ShooterAttack(shooter));
         StartCoroutine(ShooterMove(shooter));
         StartCoroutine(ShooterAnimate(shooter));
      }

      private IEnumerator ShooterAnimate(Shooter shooter) {
         _animator.runtimeAnimatorController = shooter.AnimatorController;

         if (shooter.AnimatorController != null) {
            _animator.Play("Running");
         }

         while (true) {
            // Rotating the body
            var scaleX = Player.Instance.transform.position.x > transform.position.x ? 1f : -1f;

            if (transform.localScale.x != scaleX)
               transform.localScale = Vector3.Scale(transform.localScale, new Vector3(-1f, 1f, 1f));

            // Rotating the gun
            var facingModifier = transform.localScale.x < 0 ? -1f : 1f;

            var direction = Player.Instance.transform.position - _weaponDisplay.transform.position;
            var targetRotation = Quaternion.FromToRotation(Vector2.right * facingModifier, direction);

            _weaponDisplay.transform.rotation = targetRotation;
            yield return new WaitForEndOfFrame();
         }
      }

      private IEnumerator ShooterMove(Shooter shooter) {
         const int directions = 18;
         float angle = 360 / directions;

         float noSeeTime = 0;

         float enemiesSpreadDistance = Random.Range(2.75f, 4f);
         float minPlayerDistance = Random.Range(5.5f, 7.5f);

         var offsetAngle = Random.Range(0, 2) == 1 ? 5f : -5f;
         var escapedWall = true;

         while (true) {
            if (SeesPlayer())
               noSeeTime = 0f;
            else
               noSeeTime += Time.fixedDeltaTime;

            var otherEnemiesAround = Physics2D.OverlapCircleAll(transform.position, enemiesSpreadDistance, 1 << 6);

            // If the enemy sees or saw the player not too long ago
            if (noSeeTime <= 1.2f || Vector3.Distance(Player.Instance.transform.position, transform.position) < 2f) {

               // We raycast in different directions and determine the best way to go by
               var bestDirection = (vector: Vector3.zero, weight: 0f);
               for (int i = 0; i < directions; i++) {
                  var direction = Quaternion.Euler(0f, 0f, angle * i) * Vector2.right;
                  var hit = Physics2D.Raycast(transform.position, direction, 30f, 1 << 8);
                  if (!hit)
                     continue;

                  var rotationOffset = Quaternion.Euler(0f, 0f, offsetAngle);
                  var unwantedObjectsModifier = 1f;

                  // If we hit the wall, the final weight value of this ray will be lowered
                  if (hit.distance < 1f) {
                     unwantedObjectsModifier *= hit.distance;

                     // Also the enemy will switch strafe direction
                     if (escapedWall || hit.distance < 0.8f)
                        offsetAngle = -offsetAngle;

                     escapedWall = false;
                  }
                  else {
                     escapedWall = true;
                  }

                  // if there are other enemies around, the final weight value of the ray will be decreased 
                  // Depending on how close it is to the direction to that enemy and the distance between this and the other enemy
                  foreach (var obj in otherEnemiesAround) {
                     if (obj.gameObject == gameObject || Vector3.Distance(obj.transform.position, transform.position) > enemiesSpreadDistance)
                        continue;

                     var distanceModifier = 1f - Vector3.Distance(obj.transform.position, transform.position) / enemiesSpreadDistance;
                     var modifier = Vector3.Dot(direction, rotationOffset * (obj.transform.position - transform.position).normalized);

                     unwantedObjectsModifier *= 1f - modifier * distanceModifier * 2f;
                  }

                  // If the enemy gets too close to the player, the final weight of the ray will be decreased
                  // Depending on how close it is to the direction to the player and the distance between enemy and the player
                  if (Vector3.Distance(Player.Instance.transform.position, transform.position) < minPlayerDistance) {
                     var distanceModifier = 1f - Vector3.Distance(Player.Instance.transform.position, transform.position) / minPlayerDistance;
                     var modifier = Vector3.Dot(direction, rotationOffset * (Player.Instance.transform.position - transform.position).normalized);

                     unwantedObjectsModifier *= 1f - modifier * distanceModifier * 2f;
                  }

                  // Calculating weight of the ray by targetting the player and multiplying by other factors
                  var weight = Vector3.Dot(direction, (Player.Instance.transform.position - transform.position).normalized) + 1f;
                  weight *= unwantedObjectsModifier;

                  // Kinda obvious
                  if (bestDirection.weight < weight)
                     bestDirection = (direction, weight); 

                  #region Draw debug lines ooo
                  if (weight < 0) weight = 0;
                  var lineTarget = transform.position + ((Vector3)hit.point - transform.position).normalized * weight;
                  Debug.DrawLine(transform.position, lineTarget, Color.green, 0.01f);
                  #endregion
               }

               _rb.velocity = Vector3.Lerp(_rb.velocity, shooter.MovementSpeed * bestDirection.vector, Time.fixedDeltaTime * 5f);
            }
            else {
               // If the enemy doesn't see player for too long, it builds a path to him and starts following it until it sees the player
               Vector3Int[] offsets = new Vector3Int[] {
                  Vector3Int.zero, Vector3Int.up, Vector3Int.down
               };

               // Using offsets so in case if the enemy will be standing in a wall tile with smaller hitbox, it still would be able to build a path
               var roundedPosition = Vector3Int.FloorToInt(transform.position);
               var roundedTarget = Vector3Int.FloorToInt(Player.Instance.transform.position);
               List<Vector3> path = new();
               foreach (var offset in offsets) {
                  var generatePathTask = NavigationSystem.Instance.GeneratePathAsync(roundedPosition + offset, roundedTarget);

                  yield return new WaitUntil(() => generatePathTask.IsCompleted);

                  path = generatePathTask.GetAwaiter().GetResult();
                  if (path.Count != 0)
                     break;
               }

               if (path.Count == 0)
                  Debug.LogWarning("Failed to generate path.");

               // Following the path until we see the player again
               // If the player went too far from the previous path target point, we generate a new one immideately
               bool stopFollowingPath = false;
               foreach (var position in path) {
                  while (Vector3.Distance(transform.position, position) > 0.5f) {
                     if (SeesPlayer() || Vector3.Distance(Player.Instance.transform.position, roundedTarget) > 5f) {
                        stopFollowingPath = true;
                        break;
                     }

                     _rb.velocity = Vector3.Lerp(_rb.velocity, (shooter.MovementSpeed + 1f) * (position - transform.position), Time.fixedDeltaTime * 5f);

                     yield return new WaitForFixedUpdate();
                  }

                  if (stopFollowingPath)
                     break;
               }
            }

            yield return new WaitForFixedUpdate();
         }
      }

      private IEnumerator ShooterAttack(Shooter shooter) {
         yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

         while (true) {
            var reloadTime = justGotHit
               ? 0f
               : shooter.Gun.ReloadTimeSeconds + Random.Range(0f, 0.5f);

            justGotHit = false;

            yield return new WaitForSeconds(reloadTime);
            yield return new WaitUntil(SeesPlayer);

            var facingModifier = transform.localScale.x < 0 ? -1f : 1f;

            // Shooting
            var offset = shooter.Gun.GunpointOffset * new Vector2(facingModifier, 1f);

            Vector3 bulletPosition = _weaponDisplay.transform.position + _weaponDisplay.transform.rotation * offset;

            var startRotation = Quaternion.FromToRotation(Vector3.right * facingModifier, _weaponDisplay.transform.right);
            var angle = shooter.Gun.BulletsPerShot / 2 * shooter.Gun.AngleBetweenBullets * facingModifier;

            for (int i = 0; i < shooter.Gun.BulletsPerShot; i++) {
               var bullet = Instantiate(shooter.Gun.Bullet);
               bullet.gun = shooter.Gun;

               BulletPooler.Instance.CreateFromPool(bullet, bulletPosition, startRotation * Quaternion.Euler(0f, 0f, angle));
               angle += shooter.Gun.AngleBetweenBullets * -facingModifier;
            }

            AudioManager.Instance.PlayOneShot(shooter.Gun.ShootSoundName);
         }
      }

      private bool SeesPlayer() {
         var direction = (Player.Instance.transform.position - transform.position).normalized;
         var hit = Physics2D.Raycast(transform.position, direction, 10f, 1 << 3 | 1 << 8);

         if (hit)
            return hit.collider.gameObject.layer == 3;

         return false;
      }

      private IEnumerator HitBehaviour() {
         if (health > 0) {
            _animator.Play("Hit");
            _rb.velocity = Vector2.zero;

            yield return new WaitForSeconds(0.25f);

            StartCoroutine(enemyBehaviour.Method.Name);
         }
         else {
            _animator.Play("Hit");

            yield return new WaitForSeconds(0.15f);

            OnDeath?.Invoke();

            Destroy(gameObject);
         }
      }

      private void OnPlayerDeath() {
         StopAllCoroutines();

         _animator.Play("Idle");
         _rb.simulated = false;
      }
   }
}