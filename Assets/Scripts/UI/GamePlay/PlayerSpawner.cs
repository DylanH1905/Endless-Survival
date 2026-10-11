using UnityEngine;
using ESS;
using ESS.Objects.Player;

namespace ESS.UI.GamePlay
{

	public class PlayerSpawner : MonoBehaviour
	{
		[SerializeField] private Transform spawnPoint;

		private void Start()
		{
			CharacterData character = SelectedCharacter.Current;

			if (character == null)
			{
				Debug.LogError("No character selected!");
				return;
			}

			if (character.gameplayPrefab == null)
			{
				Debug.LogError(
					$"Missing gameplay prefab for {character.characterName}"
				);
				return;
			}

			Instantiate(
				character.gameplayPrefab,
				spawnPoint.position,
				spawnPoint.rotation
			);
		}
	}
}