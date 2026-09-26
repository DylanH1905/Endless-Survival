using UnityEngine;
using UnityEngine.UI;

namespace ESS.Objects.Player
{
    public class SkillHandler : MonoBehaviour
    {
        [Header("Grenade")]
        [SerializeField] private GameObject grenade;
        [SerializeField] private bool isGrenadeLocked;
        [SerializeField] private Button grenadeButton;

        [Header("Multiple Projectiles")]
        [SerializeField] private GameObject projectile;
        [SerializeField] private int numberOfProjectiles = 3;
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private bool isMultipleLocked;
        [SerializeField] private Button multipleButton;

        private bool grenadeRequested;
        private bool multipleRequested;

        public void OnClickMultiple()
        {
            if (!isMultipleLocked)
            {
                multipleRequested = true;
                SetButtonInteractable(multipleButton, false);
            }
        }

        public void OnClickGrenade()
        {
            if (!isGrenadeLocked)
            {
                grenadeRequested = true;
                SetButtonInteractable(grenadeButton, false);
            }
        }

        public void onClickMutiple()
        {
            OnClickMultiple();
        }

        public void onClickGen()
        {
            OnClickGrenade();
        }

        private void Update()
        {
            if (grenadeRequested)
            {
                if (grenade != null)
                {
                    Instantiate(grenade, transform.position, Quaternion.identity);
                }

                grenadeRequested = false;
            }

            if (multipleRequested)
            {
                if (projectile != null && numberOfProjectiles > 0)
                {
                    SpawnProjectiles();
                }

                multipleRequested = false;
            }
        }

        private void SpawnProjectiles()
        {
            float angleStep = 360f / numberOfProjectiles;

            for (int index = 0; index < numberOfProjectiles; index++)
            {
                float angle = index * angleStep * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                GameObject spawnedProjectile = Instantiate(projectile, transform.position, Quaternion.identity);
                Rigidbody2D projectileRigidbody = spawnedProjectile.GetComponent<Rigidbody2D>();

                if (projectileRigidbody != null)
                {
                    projectileRigidbody.velocity = direction * projectileSpeed;
                }
            }
        }

        private static void SetButtonInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }
}