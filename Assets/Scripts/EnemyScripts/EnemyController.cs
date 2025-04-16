using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum AttackType {Melee, Ranged}
    public enum EnemyType {Rat, Sumelse}

    [Header("Enemy Type")]
    [SerializeField] private EnemyType enemyType;
    [SerializeField, HideInInspector] private EnemyType lastEnemyType;
    [SerializeField] private AttackType attackType;

    public Animator enemyAnimator;

    [Header("Enemy Stats")]
    [SerializeField] private int health;
    [SerializeField] private int damage;
    [SerializeField] private float speed;
    [SerializeField] private float attackCooldown;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float attackRange;

    [Header("Detection")]
    [SerializeField] private float obstacleCheckCircleRadius;
    [SerializeField] private float obstacleCheckDistance;
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Projectile Stats")]
    [SerializeField] private float projSpeed;
    [SerializeField] private Transform projPos;
    [SerializeField] private GameObject projPrefab;

    private Rigidbody2D rb;
    private PlayerDetection playerDetection;
    private Vector2 targetDirection;
    private RaycastHit2D[] obstacleCollisions;
    private Transform player;
    private bool isCooldown;
    private bool enemyCollided;
    private bool isPlayerInRange;
    private float stopBufferTime = 0.2f;
    private float stopTimer = 0f;

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
        PlayerInRange();
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

    private void OnCollisionEnter2D(Collision2D collision) //Enemy touches player
    {
        //Debug.Log(collision);
        if (collision.gameObject == GameObject.FindGameObjectWithTag("Player") && !isCooldown)
        {
            enemyCollided = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision) //Player stops touching enemy
    {
        if (collision.gameObject == GameObject.FindGameObjectWithTag("Player") && !isCooldown)
        {
            enemyCollided = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) //Player enters attack zone
    {
        if (collision.gameObject == GameObject.FindGameObjectWithTag("Player"))
        {
            PlayEnemyAttackAnimation();
            isCooldown = true;
            StartCoroutine(AttackingCooldown());
        }
    }

    private void OnTriggerExit2D(Collider2D collision) //Player exits attack zone
    {
        if (collision.gameObject == GameObject.FindGameObjectWithTag("Player"))
        {
            PlayEnemyWalkingAnimation();
        }
    }

    IEnumerator AttackingCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false;
    }

    private void PlayerInRange()
    {
        var playerLayer = LayerMask.GetMask("Player");
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.up, attackRange, playerLayer);
        Debug.DrawRay(transform.position, transform.up * attackRange, Color.yellow);

        if (hit && !isCooldown)
        {
            Debug.Log("Raycast Hit: Player");
            isPlayerInRange = true;
            stopTimer = stopBufferTime;
            Attack();
            isCooldown = true;
            StartCoroutine(AttackingCooldown());
        }
        else
        {
            stopTimer -= Time.fixedDeltaTime;
            isPlayerInRange = stopTimer > 0f;
        }
    }

    private void StopStartMovement()
    {
        if (targetDirection == Vector2.zero || isPlayerInRange)
        {
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.linearVelocity = transform.up * speed;
        }
    }

    private void ShootProjectile()
    {
        Debug.Log("Shooting Projectile");
        GameObject newProjectile = Instantiate(projPrefab, projPos.position, transform.rotation);
        Rigidbody2D rbProj = newProjectile.GetComponent<Rigidbody2D>();
        if (rbProj != null)
        {
            rbProj.linearVelocity = transform.up * projSpeed;
        }
    }

    public void Attack()
    {
        switch (attackType)
        {
            case AttackType.Melee:
                Debug.Log("Melee Attack");
                if (player != null && enemyCollided)
                    player.GetComponent<HealthController>()?.TakeDamage(damage);
                break;
            case AttackType.Ranged:
                Debug.Log("Ranged Attack");
                if (player != null)
                    ShootProjectile();
                break;
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && enemyType != lastEnemyType)
        {
            //Changes Enemy stats within the inspector for enemy types
            SetEnemyType(enemyType);
            lastEnemyType = enemyType;
        }
    }

    //Holds Stats for enemy Types
    private void SetEnemyType(EnemyType preset)
    {
        switch (enemyType)
        {
            case EnemyType.Rat:
                health = 3;
                damage = 1;
                speed = 0.7f;
                attackCooldown = 1;
                rotationSpeed = 500;
                attackRange = 0.3f;
                attackType = AttackType.Melee;
                break;
            case EnemyType.Sumelse:
                health = 3;
                damage = 0;
                speed = 0.5f;
                attackCooldown = 1;
                rotationSpeed = 500;
                attackRange = 1.5f;
                projSpeed = 0.5f;
                attackType = AttackType.Ranged;
                break;
        }
    }

    //Hold Enemy Animations
    public void PlayEnemyWalkingAnimation()
    {
        switch (enemyType)
        {
            case EnemyType.Rat:
                enemyAnimator.Play("rat_walk");
                break;
        }
    }

    public void PlayEnemyAttackAnimation()
    {
        switch (enemyType)
        {
            case EnemyType.Rat:
                enemyAnimator.Play("rat_attack");
                break;
        }
    }
}
