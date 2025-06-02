using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthController : MonoBehaviour
{
    private int currentHealth;
    private PlayerController playerController;
    private EnemyController enemyController;
    private EnemySpawnManager enemySpawnManager;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        enemyController = GetComponent<EnemyController>();

        if (this.playerController != null)
        {
            currentHealth = playerController.GetPlayerHealth;
            enemySpawnManager = FindFirstObjectByType<EnemySpawnManager>();
        }
        else if (this.enemyController != null)
        {
            currentHealth = enemyController.GetEnemyHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, currentHealth);

        if (playerController != null)
        {
            //Debug.Log($"Player took {damage} damage. Current Health: {currentHealth}");
        }
        else if (enemyController != null)
        {
            //Debug.Log($"Enemy took {damage} damage. Current Health: {currentHealth}");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (playerController != null)
        {
            if (enemySpawnManager != null)
            {
                Debug.Log("Setting isDead to true on EnemySpawnManager");
                enemySpawnManager.isDead = true;
            }
            else
            {
                Debug.LogWarning("EnemySpawnManager not found!");
            }
            Destroy(gameObject);
            Debug.Log("Player has died.");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //StartCoroutine(Respawn());
           
        }
        else if (enemyController != null)
        {
            //Debug.Log("Enemy has died.");
            // Add enemy death logic here.
            Destroy(gameObject);
        }
    }
    private void OnDestroy()
    {
     
    }
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    IEnumerator Respawn()
    {
        
        yield return new WaitForSeconds(0.9f);
        
    }
}
