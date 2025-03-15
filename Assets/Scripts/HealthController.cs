using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthController : MonoBehaviour
{
    private int currentHealth;
    private PlayerController playerController;
    private EnemyController enemyController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        enemyController = GetComponent<EnemyController>();

        if (this.playerController != null)
        {
            currentHealth = playerController.GetPlayerHealth;
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
            Debug.Log($"Player took {damage} damage. Current Health: {currentHealth}");
        }
        else if (enemyController != null)
        {
            Debug.Log($"Enemy took {damage} damage. Current Health: {currentHealth}");
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
            Debug.Log("Player has died.");
            // Add player death logic here.
        }
        else if (enemyController != null)
        {
            Debug.Log("Enemy has died.");
            // Add enemy death logic here.
        }

        Destroy(gameObject);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
