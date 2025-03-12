using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 9;
    private int currentHealth;
    public Image healthBar;

    [Header("Health Sprites")]
    public Sprite healthBar9;
    public Sprite healthBar8;
    public Sprite healthBar7;
    public Sprite healthBar6;
    public Sprite healthBar5;
    public Sprite healthBar4;
    public Sprite healthBar3;
    public Sprite healthBar2;
    public Sprite healthBar1;
    public Sprite healthBar0;



    void Start()
    {
        currentHealth = maxHealth;
        
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
        {
            currentHealth--;
        }
        switch (currentHealth)
        {
            case 9:
                healthBar.sprite = healthBar9;
                break;
            case 8:
                healthBar.sprite = healthBar8;
                break;
            case 7:
                healthBar.sprite = healthBar7;
                break;
            case 6:
                healthBar.sprite = healthBar6;
                break;
            case 5:
                healthBar.sprite = healthBar5;
                break;
            case 4:
                healthBar.sprite = healthBar4;
                break;
            case 3:
                healthBar.sprite = healthBar3;
                break;
            case 2:
                healthBar.sprite = healthBar2;
                break;
            case 1:
                healthBar.sprite = healthBar1;
                break;
            case 0:
                healthBar.sprite = healthBar0;
                break;

        }

        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        
    }

    private void Die()
    {
        Debug.Log(" You Died.");
        //Invoke("RestartScene", 3f);
    }

    //private void RestartScene()
    //{
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    //}
    // Update is called once per frame
    //private void OnTriggerEnter2D(Collider2D collision)
    //{
       
   // }
}
