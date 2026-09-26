using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : ObjectCharacter
{
    public void Start()
    {
        InitializeStats();
    }

    public void InitializeStats()
    {
        maxHealth = 150;
        attackDamage = 15;
        speed = 4f;

        health = maxHealth;
        IsDead = false;
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        if (IsDead)
        {
            Destroy(gameObject);
        }
    }
}