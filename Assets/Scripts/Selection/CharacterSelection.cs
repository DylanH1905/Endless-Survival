namespace ESS.Selection
{
    // Persists the player's chosen character across the Selection -> GamePlay scene transition.
    public static class CharacterSelection
    {
        private const string PlayerPrefsKey = "ESS.SelectedCharacterId";

        public static string SelectedCharacterId
        {
            get => UnityEngine.PlayerPrefs.GetString(PlayerPrefsKey, "Archer");
            set => UnityEngine.PlayerPrefs.SetString(PlayerPrefsKey, value);
        }
    }
}
