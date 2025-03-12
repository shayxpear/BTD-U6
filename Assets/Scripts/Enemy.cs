using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyData enemyData;

    public enum EnemyState { Idle, Chasing, Attacking }
    public enum MobType { Melee, Ranged }

    public EnemyState currentState = EnemyState.Idle; 
    public MobType mobType = MobType.Melee;

    public float chaseDistance = 5f; // Distance at which enemy starts chasing the player
    public float stopChasingDistance = 5f; // Distance at which enemy returns to Idle (after chasing)
    public float attackDistance = 2f; // Distance at which the enemy attacks
    public float moveSpeed = 2f; // Speed of the enemy
    public float attackCooldown = 1f; // Time between attacks

    private Transform player;
    public Transform enemyTransform;
    private float timeSinceLastAttack = 1f;

    public Animator enemyAnimator;

    private int currentEnemyHp;

    private void Start()
    {
        currentEnemyHp = enemyData.hp;
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        // Switch states based on current state
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleState();
                break;

            case EnemyState.Chasing:
                ChasingState();
                break;
            case EnemyState.Attacking:
                AttackingState();
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

    private void AttackingState()
    {
        // Attack the player if the cooldown is over
        if (timeSinceLastAttack >= attackCooldown)
        {
            Attack();
            timeSinceLastAttack = 0f; // Reset the attack cooldown
        }

        // Check if the player is out of attack range
        if (Vector3.Distance(transform.position, player.position) > attackDistance)
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void Attack()
    {
        if (mobType == MobType.Melee)
        {
            // Melee attack logic (e.g., damage player in range)
            Debug.Log("Melee Attack");
        }
        else if (mobType == MobType.Ranged)
        {
            // Ranged attack logic (e.g., shoot projectile at player)
            Debug.Log("Ranged Attack");
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
