
using UnityEngine;
using UnityEngine.UI;
using ESS;
using ESS.Objects.Player;

namespace ESS.UI.GamePlay
{
	public class PlayerCharacterController : SystemMonoBehaviour
	{
		[Header("Character Parts")]
		[SerializeField] private SpriteRenderer headRenderer;
		[SerializeField] private SpriteRenderer weaponRenderer;
		[SerializeField] private SpriteRenderer powerRenderer;

		[SerializeField] private PlayerCombatController combatController;

		private void Start()
		{
			ApplySelectedCharacter();
		}

		private void ApplySelectedCharacter()
		{
			CharacterData character = SelectedCharacter.Current;

			if (character == null)
			{
				Error("No character selected!");
				return;
			}

			if (headRenderer != null)
				headRenderer.sprite = character.head;

			if (weaponRenderer != null)
				weaponRenderer.sprite = character.weapon;

			if (powerRenderer != null)
				powerRenderer.sprite = character.power;

			combatController.ApplyCombat(character);
		}
	}
}