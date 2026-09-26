using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    private Enemy target;
    private int damage;
    private float speed;
    private float trailLength;
    private LineRenderer lineRenderer;

    public void Initialize(Enemy newTarget, int newDamage, float newSpeed, Color color)
    {
        target = newTarget;
        damage = newDamage;
        speed = newSpeed;

        SetupLineRenderer(color);
    }

    private void Awake()
    {
        SetupLineRenderer(Color.yellow);
    }

    private void Update()
    {
        if (target == null || target.IsDead)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position;
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime);

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position - direction * trailLength);

        if (Vector2.Distance(transform.position, targetPosition) <= 0.05f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }

    private void SetupLineRenderer(Color color)
    {
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.startColor = color;
        lineRenderer.endColor = new Color(color.r, color.g, color.b, 0f);
        lineRenderer.numCapVertices = 4;
        lineRenderer.sortingLayerName = "Default";
        lineRenderer.sortingOrder = 10;

        if (lineRenderer.material == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                lineRenderer.material = new Material(shader);
            }
        }

        trailLength = 0.35f;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.position);
    }
}