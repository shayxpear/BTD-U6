using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HealthController : MonoBehaviour
{
    private int currentHealth;
    private EnemyController enemyController;
    private EnemySpawnManager enemySpawnManager;

    void Awake()
    {
        
        enemyController = GetComponent<EnemyController>();

        if (this.enemyController != null)
        {
            currentHealth = enemyController.GetEnemyHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, currentHealth);

        if (enemyController != null)
        {
            //Debug.Log($"Enemy took {damage} damage. Current Health: {currentHealth}");
            StartCoroutine(HurtEnemyCoroutine());
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
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
            
           
        if (enemyController != null)
        {
            StartCoroutine(DieEnemyCoroutine());
        }
    }
    private IEnumerator DieEnemyCoroutine()
    {
        // Trigger the death animation.
        enemyController.PlayEnemyDeathAnimation();

        // Check if the death state exists in any Animator layer
        Animator animator = enemyController.enemyAnimator;
        int deathHash = Animator.StringToHash("death");
        bool hasDeathState = false;
        for (int i = 0; i < animator.layerCount; i++)
        {
            if (animator.HasState(i, deathHash))
            {
                hasDeathState = true;
                break;
            }
        }
        if (hasDeathState)
        {
            // Wait until the animator is in the death state
            yield return new WaitUntil(() =>
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.IsName("death");
            });

            // Wait until the death animation has played through
            yield return new WaitUntil(() =>
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.normalizedTime >= 1f;
            });
        }
        // Once the death animation is complete (or if it doesn't exist), destroy this enemy.
        Destroy(gameObject);
    }
    private IEnumerator HurtEnemyCoroutine()
    {
        // Trigger the death animation.
        enemyController.PlayEnemyHurtAnimation();
        // Check if the death state exists in any Animator layer
        Animator animator = enemyController.enemyAnimator;
        int hurtHash = Animator.StringToHash("hurt");
        bool hasHurtState = false;
        for (int i = 0; i < animator.layerCount; i++)
        {
            if (animator.HasState(i, hurtHash))
            {
                hasHurtState = true;
                break;
            }
        }
        if (hasHurtState)
        {
            // Wait until the animator is in the death state
            yield return new WaitUntil(() =>
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.IsName("hurt");
            });

            // Wait until the death animation has played through
            yield return new WaitUntil(() =>
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                return stateInfo.normalizedTime >= 1f;
            });
        }
        // Once the death animation is complete (or if it doesn't exist), destroy this enemy.
        enemyController.ouch = false;
    }
    private void OnDestroy()
    {
     
    }
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    
}
