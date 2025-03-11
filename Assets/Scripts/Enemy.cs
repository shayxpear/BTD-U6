using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyData enemyData;

    public enum EnemyState { Idle, Chasing } // Removed Attacking
    public EnemyState currentState = EnemyState.Idle;

    public float chaseDistance = 5f; // Distance at which enemy starts chasing the player
    public float stopChasingDistance = 5f; // Distance at which enemy returns to Idle (after chasing)

    private Transform player;
    public Transform enemyTransform;

    public Animator enemyAnimator;

    private int currentEnemyHp;

    private void Start()
    {
        currentEnemyHp = enemyData.hp;
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        // Switch states based on current state
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleState();
                break;

            case EnemyState.Chasing:
                ChasingState();
                break;
        }
    }

    private void IdleState()
    {
        // Calculate the distance from the enemy to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if the player is within chase distance
        if (distanceToPlayer <= chaseDistance)
        {
            Debug.Log($"Enemy {enemyData.enemyName} is now chasing the player.");
            currentState = EnemyState.Chasing;
        }
    }

    private void ChasingState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Move towards the player while chasing
        transform.position = Vector3.MoveTowards(transform.position, player.position, enemyData.speed * Time.deltaTime);

        // Check if the player is too far, return to Idle
        if (distanceToPlayer > stopChasingDistance)  // Make sure this distance is large enough to consider "lost"
        {
            Debug.Log("Lost player, returning to Idle");
            currentState = EnemyState.Idle;
        }
    }

    // Example of handling damage
    public void TakeDamage(int damageAmount)
    {
        if (enemyData != null)
        {
            currentEnemyHp -= damageAmount;
            Debug.Log($"{enemyData.enemyName} took {damageAmount} damage. Remaining health: {currentEnemyHp}");
        }

        if (currentEnemyHp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
