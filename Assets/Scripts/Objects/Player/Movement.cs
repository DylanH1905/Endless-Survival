using UnityEngine;
using UnityEngine.UI;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Player player;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashLength = 0.5f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private bool isDashLocked;
    [SerializeField] private Button dashButton;

    [SerializeField] private SkillHandler skillHandler;

    private Vector2 movement;
    private float activeMoveSpeed;
    private float dashCounter;
    private float dashCooldownCounter;
    private bool dashRequested;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (player == null)
        {
            player = GetComponent<Player>();
        }

        if (skillHandler == null)
        {
            skillHandler = GetComponent<SkillHandler>();
        }

        activeMoveSpeed = player != null && player.speed > 0f ? player.speed : moveSpeed;
    }

    public void OnClickDash()
    {
        if (!isDashLocked)
        {
            dashRequested = true;
            if (dashButton != null)
            {
                dashButton.interactable = false;
            }
        }
    }

    public void OnClickMultiple()
    {
        if (skillHandler != null)
        {
            skillHandler.OnClickMultiple();
        }
    }

    public void OnClickGrenade()
    {
        if (skillHandler != null)
        {
            skillHandler.OnClickGrenade();
        }
    }

    public void onClickDash()
    {
        OnClickDash();
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
        Move();
        RotateToClosestEnemy();
        UpdateDash();
    }

    private void Move()
    {
        movement.x = 0f;
        movement.y = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            movement.x -= 1f;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            movement.x += 1f;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            movement.y += 1f;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            movement.y -= 1f;
        }

        movement.Normalize();

        if (rb != null)
        {
            rb.velocity = movement * activeMoveSpeed;
        }
    }

    private void UpdateDash()
    {
        if (dashRequested && dashCooldownCounter <= 0f && dashCounter <= 0f)
        {
            activeMoveSpeed = dashSpeed;
            dashCounter = dashLength;
        }

        dashRequested = false;

        if (dashCounter > 0f)
        {
            dashCounter -= Time.deltaTime;
            if (dashCounter <= 0f)
            {
                activeMoveSpeed = player != null && player.speed > 0f ? player.speed : moveSpeed;
                dashCooldownCounter = dashCooldown;
            }
        }

        if (dashCooldownCounter > 0f)
        {
            dashCooldownCounter -= Time.deltaTime;
        }
    }

    private void RotateToClosestEnemy()
    {
        Enemy closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            float distance = (enemy.transform.position - transform.position).sqrMagnitude;
            if (distance > 0f && distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        if (closestEnemy == null)
        {
            return;
        }

        Vector3 direction = closestEnemy.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    public void AddSpeed()
    {
        moveSpeed += 1f;
        if (dashCounter <= 0f)
        {
            activeMoveSpeed += 1f;
        }
    }

}