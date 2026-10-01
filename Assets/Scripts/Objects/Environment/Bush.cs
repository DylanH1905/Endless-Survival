using UnityEngine;

namespace ESS.Objects.Environment
{
    // Walkable terrain that hides any Character (Player/Enemy) from focus logic while they stand inside its trigger area.
    [RequireComponent(typeof(Collider2D))]
    public class Bush : EnvironmentCharacter
    {
        [SerializeField] private float hiddenAlpha = 0.3f;

        private SpriteRenderer spriteRenderer;
        private float defaultAlpha = 1f;
        private int charactersInside;

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
            ObjectCharacter character = other.GetComponent<ObjectCharacter>();
            if (character == null)
            {
                return;
            }

            character.IsHidden = true;
            charactersInside++;
            SetAlpha(hiddenAlpha);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            ObjectCharacter character = other.GetComponent<ObjectCharacter>();
            if (character == null)
            {
                return;
            }

            character.IsHidden = false;
            charactersInside = Mathf.Max(charactersInside - 1, 0);
            if (charactersInside == 0)
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
