using UnityEngine;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private HealthBarUI healthBar;
    [SerializeField] private float smoothSpeed = 8f;

    private float targetFillAmount;

    private void Awake()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        if (healthBar == null)
        {
            healthBar = GetComponent<HealthBarUI>();
        }

        if (healthBar != null)
        {
            healthBar.SetTarget(player);
        }
    }

    private void Start()
    {
        UpdateHealthValue();
    }

    private void Update()
    {
        UpdateHealthValue();

        if (healthBar != null)
        {
            healthBar.SetFillAmount(Mathf.MoveTowards(
                healthBar.FillAmount,
                targetFillAmount,
                smoothSpeed * Time.deltaTime));
        }
    }

    private void UpdateHealthValue()
    {
        if (player == null || healthBar == null)
        {
            return;
        }

        targetFillAmount = player.maxHealth > 0
            ? Mathf.Clamp01((float)player.health / player.maxHealth)
            : 0f;
    }
}