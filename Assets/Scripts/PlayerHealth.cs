using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    
    void Start()
    {
        currentHealth = maxHealth;
        
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (currentHealth < 0)
        {
            Die();
        }
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
