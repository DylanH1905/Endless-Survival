using UnityEngine;

public class BossPowerProjectile : MonoBehaviour
{
    [SerializeField] private float hitDistance = 0.2f;

    private Player target;
    private int damage;
    private float speed;
    private Vector3 aimDirection;
    private float spriteRotationOffset;

    public void Initialize(Player newTarget, int newDamage, float newSpeed, Vector3 aimVector)
    {
        target = newTarget;
        damage = newDamage;
        speed = newSpeed;
        aimDirection = aimVector.sqrMagnitude > Mathf.Epsilon
            ? aimVector.normalized
            : Vector3.right;

        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        spriteRotationOffset = transform.eulerAngles.z - aimAngle;
    }

    private void Update()
    {
        if (target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 aimVector = target.transform.position - transform.position;
        float distance = aimVector.magnitude;
        if (distance <= hitDistance)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        aimDirection = aimVector / distance;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, aimAngle + spriteRotationOffset);

        float travelDistance = speed * Time.deltaTime;
        if (distance <= travelDistance + hitDistance)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        transform.position += aimDirection * travelDistance;
    }
}