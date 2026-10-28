using UnityEngine;

namespace ESS.Objects.Player.Sword
{
	public class Setup : IPlayer
	{
		protected override void ConfigureBaseStats()
		{
			maxHealth = 2500;
			attackDamage = 40;
			speed = 4f;
		}
	}
}
