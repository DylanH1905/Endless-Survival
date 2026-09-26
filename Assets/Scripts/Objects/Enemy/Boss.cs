using UnityEngine;

public class Boss : Enemy
{
    [SerializeField] private float powerAttackRange = 6f;
    [SerializeField] private float powerProjectileSpeed = 8f;

    private void Awake()
    {
        FocusPlayer focusPlayer = GetComponent<FocusPlayer>();
        if (focusPlayer == null)
        {
            focusPlayer = gameObject.AddComponent<FocusPlayer>();
        }

        focusPlayer.ConfigurePowerAttack(powerAttackRange, powerProjectileSpeed);
    }

    public override void InitializeStats()
    {
        maxHealth = 5000;
        attackDamage = 50;
        speed = 6f;

        health = maxHealth;
    }
}