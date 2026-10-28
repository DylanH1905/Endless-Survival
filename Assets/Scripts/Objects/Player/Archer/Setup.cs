using UnityEngine;

namespace ESS.Objects.Player.Archer
{
    public class Setup : IPlayer
    {
        protected override void ConfigureBaseStats()
        {
            maxHealth = 1500;
            attackDamage = 8;
            speed = 5f;
        }
    }
}
