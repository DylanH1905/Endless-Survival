
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ESS.Selection
{
    public class CharacterSelectionUI : SystemMonoBehaviour
    {
        [Header("Characters")]
        [SerializeField]
        private CharacterOption[] characters = { new CharacterOption() };

        [Header("UI References")]
        [SerializeField] private Button[] characterButtons;
        [SerializeField] private Image[] cardBackgrounds;
        [SerializeField] private Button playButton;

        [Header("Scene")]
        [SerializeField] private string gameplaySceneName = "GamePlay";

        [Header("Colors")]
        [SerializeField]
        private Color cardColor = new Color(0.15f, 0.15f, 0.2f, 0.95f);

        [SerializeField]
        private Color cardSelectedColor = new Color(0.25f, 0.45f, 0.85f, 0.95f);

        private int selectedIndex;

        private void Awake()
        {
            if (characterButtons != null)
            {
                for (int i = 0; i < characterButtons.Length; i++)
                {
                    if (characterButtons[i] == null)
                        continue;

                    int index = i;
                    characterButtons[i].onClick.AddListener(
                        () => SelectCharacter(index)
                    );
                }
            }

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (characters != null && characters.Length > 0)
                SelectCharacter(0);
        }

        private void SelectCharacter(int index)
        {
            if (characters == null ||
                index < 0 ||
                index >= characters.Length)
                return;

            selectedIndex = index;

            if (cardBackgrounds == null)
                return;

            for (int i = 0; i < cardBackgrounds.Length; i++)
            {
                if (cardBackgrounds[i] == null)
                    continue;

                cardBackgrounds[i].color =
                    i == selectedIndex
                        ? cardSelectedColor
                        : cardColor;
            }
        }

        private void OnPlayClicked()
        {
            if (characters == null ||
                characters.Length == 0 ||
                selectedIndex >= characters.Length)
            {
                Warning("No characters configured to play with.");
                return;
            }

            CharacterSelection.SelectedCharacterId =
                characters[selectedIndex].characterId;

            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}