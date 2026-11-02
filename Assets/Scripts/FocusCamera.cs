using UnityEngine;
using ESS.UI.GamePlay;

namespace ESS
{
    public class FocusCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.2f;
        [SerializeField] private bool focusOnlyWhileHoldingKey = true;
        [SerializeField] private KeyCode focusKey = KeyCode.Space;
        [SerializeField] private Joystick joystick;

        private Vector3 velocity;

        private void Awake()
        {
            if (joystick == null)
                joystick = FindObjectOfType<Joystick>();
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            bool joystickIsMoving = joystick != null
                && joystick.Direction.sqrMagnitude > 0.001f;
            if (focusOnlyWhileHoldingKey && !Input.GetKey(focusKey) && !joystickIsMoving)
            {
                velocity = Vector3.zero;
                return;
            }

            Vector3 targetPosition = new Vector3(
                target.position.x,
                target.position.y,
                transform.position.z
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void FocusImmediately()
        {
            if (target == null)
                return;

            transform.position = new Vector3(
                target.position.x,
                target.position.y,
                transform.position.z
            );
        }
    }
}