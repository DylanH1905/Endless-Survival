using ESS.Objects.Player;
using UnityEngine;

namespace ESS.Objects.Terrain
{
    // Walkable terrain that hides the Player from enemies while they stand inside its trigger area.
    [RequireComponent(typeof(Collider2D))]
    public class Bush : EnvironmentCharacter
    {
        [SerializeField] private float hiddenAlpha = 0.5f;

        private SpriteRenderer spriteRenderer;
        private float defaultAlpha = 1f;
        private int playersInside;

        private void Awake()
        {
            objectType = EnvironmentObjectType.Bush;
            blocksMovement = false;
            effectObject = false;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                defaultAlpha = spriteRenderer.color.a;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            IPlayer player = other.GetComponent<IPlayer>();
            if (player == null)
            {
                return;
            }

            player.IsHidden = true;
            playersInside++;
            SetAlpha(hiddenAlpha);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            IPlayer player = other.GetComponent<IPlayer>();
            if (player == null)
            {
                return;
            }

            player.IsHidden = false;
            playersInside = Mathf.Max(playersInside - 1, 0);
            if (playersInside == 0)
            {
                SetAlpha(defaultAlpha);
            }
        }

        private void SetAlpha(float alpha)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
