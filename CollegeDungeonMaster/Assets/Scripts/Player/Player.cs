using UnityEngine;
using UI;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerAttack))]
public class Player : MonoBehaviour {
   private Player() { }

   public static Player Instance { get; private set; }

   public PlayerMovement Movement { get; private set; }
   public PlayerAttack Attack { get; private set; }
   public PlayerAnimation AnimationController { get; private set; }

   public delegate void PlayerAction();
   public event PlayerAction OnPlayerGotHit;

   public int MaxHealth {
      get => _maxHealth;
      set {
         if (PlayerHealth > _maxHealth)
            PlayerHealth = value;

         _maxHealth = value;
      }
   }
   [SerializeField] private int _maxHealth = 100;

   public int PlayerHealth { get; private set; }

   private void Awake() {
      if (Instance == null) {
         Instance = this;
         DontDestroyOnLoad(this);

         Movement = GetComponent<PlayerMovement>();
         Attack = GetComponent<PlayerAttack>();
         AnimationController = GetComponent<PlayerAnimation>();

         PlayerHealth = MaxHealth;
      }
      else {
         Destroy(gameObject);
      }
   }

   private void Start() {
      GameManager.Instance.OnGameStateChange += OnGameStateChange;
   }

   private void OnDestroy() {
      GameManager.Instance.OnGameStateChange -= OnGameStateChange;
   }

   public void DealDamage(int damage) {
      if (PlayerHealth > 0) {
         PlayerHealth -= damage;

         BarsManager.Instance.HealthBar.SetFillingValue(PlayerHealth / (float)MaxHealth);

         OnPlayerGotHit?.Invoke();
      }
      else {
         Debug.Log("You're dead lol");
      }      
   }

   private void OnGameStateChange(GameManager.GameState state) {
      switch (state) {
         case GameManager.GameState.InGame:
            Movement.Enable();
            Attack.Enable();
            AnimationController.Enable();
            break;

         case GameManager.GameState.Paused:
            Movement.Disable();
            Attack.Disable();
            AnimationController.Disable();
            break;

         case GameManager.GameState.MainMenu:
            Destroy(gameObject);
            break;
      }
   }
}
