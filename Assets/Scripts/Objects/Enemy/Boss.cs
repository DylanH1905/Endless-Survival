public class Boss : Enemy
{
    public new void InitializeStats()
    {
        maxHealth = 500;
        attackDamage = 50;
        speed = 3f;

        health = maxHealth;
    }
}