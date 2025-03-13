using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthController : MonoBehaviour
{
    [Header("Health")]
    public CharacterStats characterStats;
    public int GetCurrentHealth { get; private set; }


    void Start()
    {
        GetCurrentHealth = characterStats.health;  
    }

    void Update()
    {
        if (GetCurrentHealth == 0)
        {
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        this.GetCurrentHealth -= damage;
    }

    private void Die()
    {
        if (characterStats is PlayerStats)
        {
            Debug.Log("Player has died.");
            Destroy(gameObject);
            // Handle player-specific death logic
        }
        else
        {
            Destroy(gameObject);
        }

    }
}
