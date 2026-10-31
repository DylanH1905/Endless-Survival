using UnityEngine;
using ESS.Objects.Enemy;
namespace ESS.Objects.Enemy
{
    public class Boss : IEnemy
    {
        [SerializeField] private float powerAttackRange = 6f;
        [SerializeField] private float powerProjectileSpeed = 8f;

        private Vector3 baseScale;
        private int bossLevel = 1;

        public event System.Action<int> LevelConfigured;
        public int CurrentLevel => bossLevel;

        private void Awake()
        {
            baseScale = transform.localScale;

            FocusPlayer focusPlayer = GetComponent<FocusPlayer>();
            if (focusPlayer == null)
            {
                focusPlayer = gameObject.AddComponent<FocusPlayer>();
            }

            focusPlayer.ConfigurePowerAttack(powerAttackRange, powerProjectileSpeed);
        }

        public override void InitializeStats()
        {
            maxHealth = Mathf.RoundToInt(5000f * Mathf.Pow(1.65f, bossLevel - 1));
            attackDamage = Mathf.RoundToInt(50f * Mathf.Pow(1.35f, bossLevel - 1));
            speed = 6f + 0.25f * (bossLevel - 1);

            health = maxHealth;
        }

        public void ConfigureBossLevel(int level)
        {
            bossLevel = Mathf.Max(1, level);
            transform.localScale = baseScale * (1f + 0.2f * (bossLevel - 1));
            LevelConfigured?.Invoke(bossLevel);
        }
    }
}