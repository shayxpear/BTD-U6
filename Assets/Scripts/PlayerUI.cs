using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private int playerHealth;
    public Image healthBar;
    public Sprite healthBar_9;
    public Sprite healthBar_8;
    public Sprite healthBar_7;
    public Sprite healthBar_6;
    public Sprite healthBar_5;
    public Sprite healthBar_4;
    public Sprite healthBar_3;
    public Sprite healthBar_2;
    public Sprite healthBar_1;
    public Sprite healthBar_0;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        playerHealth = GetComponent<HealthController>().currentHealth;
        switch (playerHealth)
        {
            case 9:
                healthBar.sprite = healthBar_9;
                break;
            case 8:
                healthBar.sprite = healthBar_8;
                break;
            case 7:
                healthBar.sprite = healthBar_7;
                break;
            case 6:
                healthBar.sprite = healthBar_6;
                break;
            case 5:
                healthBar.sprite = healthBar_5;
                break;
            case 4:
                healthBar.sprite = healthBar_4;
                break;
            case 3:
                healthBar.sprite = healthBar_3;
                break;
            case 2:
                healthBar.sprite = healthBar_2;
                break;
            case 1:
                healthBar.sprite = healthBar_1;
                break;
            case 0:
                healthBar.sprite = healthBar_0;
                break;

        }
    }
}
