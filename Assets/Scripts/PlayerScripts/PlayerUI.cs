using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int playerHealth;
    private HealthController healthController;

    public Image healthBar;
    public Sprite[] healthBarSprites;

    void Start()
    {
        healthController = GetComponent<HealthController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (healthController == null || healthBarSprites.Length != 10) return;

        int playerHealth = healthController.GetCurrentHealth();
        playerHealth = Mathf.Clamp(playerHealth, 0, 9);  // Ensure health is within bounds

        healthBar.sprite = healthBarSprites[playerHealth];
    }
}
