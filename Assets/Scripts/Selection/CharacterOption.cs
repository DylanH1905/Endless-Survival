using UnityEngine;

namespace ESS.Selection
{
    // One selectable entry in the character selection screen. Add more entries here as new
    // playable characters (besides Archer) are implemented.
    [System.Serializable]
    public class CharacterOption
    {
        public string characterId = "Archer";
        public string displayName = "Archer";
        [TextArea] public string description = "Ranged attacker who fires arrows at enemies.";
        public Sprite portrait;
    }
}
