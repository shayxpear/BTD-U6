using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthController : MonoBehaviour
{
    private CharacterStats characterStats;
    public int GetCurrentHealth { get; private set; }

    void Start()
    {
        // Determine if this is the Player or an Enemy
        if (CompareTag("Player"))
        {
            // Get PlayerStats from the GameManager
            characterStats = GameManager.Instance.playerStats;
        }
        else if (CompareTag("Enemy"))
        {
            EnemyStats enemyStats = GetComponent<EnemyController>().enemyStats;
            if (enemyStats != null)
            {
                characterStats = enemyStats;
            }
        }

        // Initialize health
        if (characterStats != null)
        {
            GetCurrentHealth = characterStats.health;
        }
        else
        {
            Debug.LogError("CharacterStats not assigned in HealthController!");
        }
    }

    void Update()
    {
        if (GetCurrentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        GetCurrentHealth -= damage;
    }

    private void Die()
    {
        if (characterStats is PlayerStats)
        {
            Debug.Log("Player has died.");
            // Handle player-specific death logic here, e.g., reload scene or show game over screen
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            Debug.Log("Enemy has died.");
            Destroy(gameObject);
        }
    }
}
