using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthController : MonoBehaviour
{
    [Header("Health")]
    public int currentHealth;
    public EnemyData health;

    void Start()
    {
        currentHealth = health.hp;
    }

    void Update()
    {
        if (currentHealth == 0)
        {
            Die();
        }
    }

    public void TakeDamage(int damage)
    {
        this.currentHealth -= damage;
    }

    private void Die()
    {
        Debug.Log(" You Died.");
        Destroy(gameObject);
    }
}
