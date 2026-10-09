using UnityEngine;
using ESS.Objects;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player
{
    public class IPlayer : ObjectCharacter
    {
        [SerializeField] private float colliderRadius = 0.4f;
        [SerializeField] private int scorePerKill = 10;
        [SerializeField] private int experiencePerKill = 25;
        [SerializeField] private int baseExperienceToNextLevel = 100;
        [SerializeField] private float experienceToNextLevelGrowth = 1.25f;

        public int score { get; private set; }
        public int level { get; private set; }
        public int experience { get; private set; }
        public int experienceToNextLevel { get; private set; }

        private void Awake()
        {
            InitializeStats();
            EnsureCollider();
        }

        private void OnEnable()
        {
            IEnemy.OnDeath += HandleEnemyDeath;
        }

        private void OnDisable()
        {
            IEnemy.OnDeath -= HandleEnemyDeath;
        }

        public void InitializeStats()
        {
            maxHealth = 2000;
            attackDamage = 20;
            speed = 4f;

            health = maxHealth;
            IsDead = false;
            score = 0;
            level = 1;
            experience = 0;
            experienceToNextLevel = baseExperienceToNextLevel;
        }

        private void HandleEnemyDeath(IEnemy enemy)
        {
            score += scorePerKill;
            AddExperience(experiencePerKill);
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return;
            }

            experience += amount;
            while (experience >= experienceToNextLevel)
            {
                experience -= experienceToNextLevel;
                LevelUp();
            }
        }

        // Growing stats + full heal reward the player for surviving long enough to level up.
        private void LevelUp()
        {
            level++;
            maxHealth += 200;
            attackDamage += 5;
            speed += 0.2f;
            health = maxHealth;
            experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * experienceToNextLevelGrowth);
        }

        // Environment pickups/obstacles (Shoes, Bush, Terrain, ...) rely on OnTrigger/OnCollision callbacks,
        // which never fire without a Collider2D on the Player.
        private void EnsureCollider()
        {
            if (GetComponent<Collider2D>() != null)
            {
                return;
            }

            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = colliderRadius;
        }

    }
}