using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player
{
	public abstract class IPlayerAttack : SystemMonoBehaviour
	{
		[SerializeField] protected IPlayer player;

		public IEnemy target { get; protected set; }

		protected virtual void Awake()
		{
			if (player == null)
			{
				player = GetComponent<IPlayer>();
			}
		}

		protected virtual bool CanAttack =>
			player != null && !player.IsDead;

		protected virtual void OnDisable()
		{
			target = null;
		}
	}
}