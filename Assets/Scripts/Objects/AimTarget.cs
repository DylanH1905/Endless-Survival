
using UnityEngine;
using ESS.Objects.Player;

namespace ESS.Objects
{
    public class AimTarget : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform aimAxis;
        [SerializeField] private Transform target;
        [SerializeField] private IPlayerAttack playerAttack;

        [Header("Rotation")]
        [SerializeField] private float rotationOffset;

        private void Awake()
        {
            if (aimAxis == null)
            {
                aimAxis = transform;
            }

            if (playerAttack == null)
            {
                playerAttack = GetComponentInParent<IPlayerAttack>();
            }
        }

        private void LateUpdate()
        {
            UpdateTarget();

            if (aimAxis == null || target == null)
            {
                return;
            }

            Vector2 direction = target.position - aimAxis.position;

            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float angle = Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

            aimAxis.rotation = Quaternion.Euler(
                0f,
                0f,
                angle + rotationOffset
            );
        }

        private void UpdateTarget()
        {
            if (playerAttack == null)
            {
                return;
            }

            target = playerAttack.target != null
                ? playerAttack.target.transform
                : null;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}