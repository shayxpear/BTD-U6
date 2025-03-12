using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Enemy Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float strength;

    private Rigidbody2D rb;
    private PlayerDetection playerDetection;
    private Vector2 targetDirection;
    private RaycastHit2D[] obstacleCollisions;

    [Header("Obstacle Checker")]
    [SerializeField] private float obstacleCheckCircleRadius;
    [SerializeField] private float obstacleCheckDistance;
    [SerializeField] private LayerMask obstacleLayerMask; //in order to ignore everything but player

    public EnemyData enemyData;
    public enum MobType { Melee, Ranged }
    public MobType mobType = MobType.Melee;
    public float attackDistance = 2f; // Distance at which the enemy attacks
    public float attackCooldown = 1f; // Time between attacks

    private Transform player;
    private bool isCooldown;
    private int currentEnemyHp;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerDetection = GetComponent<PlayerDetection>();
        obstacleCollisions = new RaycastHit2D[100];
        isCooldown = false;
        player = GameObject.FindWithTag("Player").transform;
    }

    private void FixedUpdate()
    {
        UpdateTargetDirection();
        RotateTowardsTarget();
        SetVelocity();
    }

    private void UpdateTargetDirection()
    {
        HandlePlayerTargeting();
        HandleObstacles();
    }

    private void HandlePlayerTargeting()
    {
        if (playerDetection.PlayerDetected)
        {
            targetDirection = playerDetection.PlayerDirection;
        }
        else
        {
            targetDirection = Vector2.zero;
        }
    }

    private void RotateTowardsTarget()
    {
        if(targetDirection == Vector2.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(transform.up, targetDirection);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        rb.SetRotation(rotation);
    }

    private void SetVelocity()
    {
        if(targetDirection == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = transform.up * speed;
        }
    }

    private void HandleObstacles()
    {
        var contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(obstacleLayerMask);

        int numberOfCollisions = Physics2D.CircleCast(
            transform.position,
            obstacleCheckCircleRadius,
            transform.up, contactFilter,
            obstacleCollisions,
            obstacleCheckDistance);

        for (int i = 0; i < numberOfCollisions; i++)
        {
            var obstacleCollision = obstacleCollisions[i];

            if(obstacleCollision.collider.gameObject == gameObject)
            {
                continue;
            }

            var targetRotation = Quaternion.LookRotation(transform.forward, obstacleCollision.normal);
            var rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            targetDirection = rotation * Vector2.up;
            break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) //Enemies
    {
        //Debug.Log(collision);
        if (collision.gameObject == GameObject.FindGameObjectWithTag("Player") && !isCooldown)
        {
            Attack();
            isCooldown = true;
            StartCoroutine(AttackingCooldown());
        }
    }

    IEnumerator AttackingCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false;
    }

    private void Attack()
    {
        if (mobType == MobType.Melee)
        {
            // Melee attack logic (e.g., damage player in range)
            Debug.Log("Melee Attack");
            player.GetComponent<HealthController>().TakeDamage(enemyData.damage);
        }
        else if (mobType == MobType.Ranged)
        {
            // Ranged attack logic (e.g., shoot projectile at player)
            Debug.Log("Ranged Attack");
        }
    }
}
