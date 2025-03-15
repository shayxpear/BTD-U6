using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private int health;
    [SerializeField] private int damage;
    [SerializeField] private float speed;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float rotationSpeed;


    [Header("Detection")]
    [SerializeField] private float obstacleCheckCircleRadius;
    [SerializeField] private float obstacleCheckDistance;
    [SerializeField] private LayerMask obstacleLayerMask;


    private Rigidbody2D rb;
    private PlayerDetection playerDetection;
    private Vector2 targetDirection;
    private RaycastHit2D[] obstacleCollisions;
    private Transform player;
    private bool isCooldown;
    public int GetEnemyHealth => health;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerDetection = GetComponent<PlayerDetection>();
        obstacleCollisions = new RaycastHit2D[100];
        isCooldown = false;
       
    }
    private void Start()
    {
        StartCoroutine(FindPlayer());
    }

    private IEnumerator FindPlayer()
    {
        while (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            yield return null;  // Wait a frame and try again
        }
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
        player.GetComponent<HealthController>().TakeDamage(damage);

    }
}
