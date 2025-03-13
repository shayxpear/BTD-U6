using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Type")]
    public EnemyStats enemyStats;

    private float obstacleCheckCircleRadius;
    private float obstacleCheckDistance;
    private LayerMask obstacleLayerMask;

    private float attackCooldown;
    private float speed;
    private float rotationSpeed;

    private Rigidbody2D rb;
    private PlayerDetection playerDetection;
    private Vector2 targetDirection;
    private RaycastHit2D[] obstacleCollisions;
    private Transform player;
    private bool isCooldown;

    private void Awake()
    {
        obstacleCheckCircleRadius = enemyStats.obstacleCheckCircleRadius;
        obstacleCheckDistance = enemyStats.obstacleCheckDistance;
        obstacleLayerMask = enemyStats.obstacleLayerMask;
        attackCooldown = enemyStats.attackCooldown;
        speed= enemyStats.speed;
        rotationSpeed= enemyStats.rotationSpeed;

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
        Debug.Log("Melee Attack");
        player.GetComponent<HealthController>().TakeDamage(enemyStats.damage);

    }
}
