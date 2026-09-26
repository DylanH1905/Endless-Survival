using UnityEngine;
using ESS.Objects.Player;
namespace ESS.Objects
{

    public class AimTarget : MonoBehaviour
    {
        [SerializeField] private Transform aimAxis;
        [SerializeField] private Transform target;
        [SerializeField] private float rotationOffset;

        private void Awake()
        {
            if (aimAxis == null)
            {
                aimAxis = transform;
            }
        }

        private void LateUpdate()
        {
            if (aimAxis == null)
            {
                return;
            }

            PlayerAttack playerAttack = aimAxis.GetComponent<PlayerAttack>();
            if (playerAttack != null)
            {
                target = playerAttack.target.GetComponent<Transform>();
                return;
            }

            Transform currentTarget = target;
            if (currentTarget == null)
            {
                return;
            }

            Vector2 direction = currentTarget.position - aimAxis.position;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            aimAxis.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}