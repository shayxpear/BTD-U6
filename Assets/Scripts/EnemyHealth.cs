using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public EnemyData enemyData;  // Reference to the ScriptableObject for this enemy

    private int currentHealth;  // Stores the current health of the enemy

    private void Start()
    {
        // Initialize health based on the data from the enemyData asset
        currentHealth = enemyData.hp;
        Debug.Log("Enemy initial health: " + currentHealth);
    }

    public void TakeDamage(int damage)
    {
        // Reduce current health based on damage taken
        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage. Current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle death logic, like playing death animations or destroying the enemy
        Debug.Log("Enemy died.");
        Destroy(gameObject);
    }
}
