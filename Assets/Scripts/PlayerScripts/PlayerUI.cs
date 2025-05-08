using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int playerHealth;
    private HealthController healthController;

    public Image healthBar;
    public Sprite[] healthBarSprites;

    public GameObject pauseMenu;
    public GameObject crosshair;

    public Sprite[] crosshairSprites;
    public Sprite crosshairSprite;

    private bool paused;

    Vector2 mousePosition;

    Transform crosshairTransform;

    void Start()
    {
        Cursor.visible = false;
        healthController = GetComponent<HealthController>();
        crosshairTransform = crosshair.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
            paused = !paused;
            pauseMenu.SetActive(paused);
        }

        PlayerCrosshair();
        PlayerHealth();
    }

    private void PlayerCrosshair()
    {
        mousePosition = Input.mousePosition;
        Vector2 mouseCursorPos = Camera.main.ScreenToWorldPoint(mousePosition);
        crosshairTransform.position = mouseCursorPos;
    }

    private void PlayerHealth()
    {
        if (healthController == null || healthBarSprites.Length != 10) return;

        int playerHealth = healthController.GetCurrentHealth();
        playerHealth = Mathf.Clamp(playerHealth, 0, 9);  // Ensure health is within bounds

        healthBar.sprite = healthBarSprites[playerHealth];
    }
}
