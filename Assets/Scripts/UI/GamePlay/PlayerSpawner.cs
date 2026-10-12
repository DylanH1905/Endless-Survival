using UnityEngine;
using ESS;
using ESS.Objects.Player;

namespace ESS.UI.GamePlay
{

	public class PlayerSpawner : SystemMonoBehaviour
	{
		[SerializeField] private Transform spawnPoint;

		private void Start()
		{
			CharacterData character = SelectedCharacter.Current;

			if (character == null)
			{
				Info("No character selected!");
				return;
			}
		}
	}
}