using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum AttackType {Melee, Ranged, Laser}
    public enum EnemyType {Rat, Sumelse, Laser}

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
    [SerializeField] private float rangedAttackRange;

    [Header("Detection")]
    [SerializeField] private float obstacleCheckCircleRadius;
    [SerializeField] private float obstacleCheckDistance;
    [SerializeField] private LayerMask obstacleLayerMask;

    [Header("Projectile Stats")]
    [SerializeField] private float projSpeed;
    [SerializeField] private Transform[] projPos; 
    [SerializeField] private GameObject projPrefab;
    [SerializeField] private bool bulletCollision;

    [Header("Leap Attack")]
    [SerializeField] private float leapSpeed;
    [SerializeField] private float leapDuration;
    [SerializeField] private float leapChargeDuration;
    [SerializeField] private float leapCooldown;

    [Header("Laser Settings")]
    [SerializeField] private LineRenderer[] laserLines;
    [SerializeField] private float laserDuration;
    [SerializeField] private float laserCooldown;
    [SerializeField] private int laserDamage;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask laserHitLayers;

    [Header("Collider Settings")]
    [SerializeField] private CircleCollider2D attackTriggerCollider;

    private bool isFiring;
    private bool isLeaping;
    private Rigidbody2D rb;
    private PlayerDetection playerDetection;
    private Vector2 targetDirection;
    private RaycastHit2D[] obstacleCollisions;
    private Transform player;
    private bool isCooldown;
    private bool enemyCollided;
    private bool hasLeaped = false;
    private bool leapRest = false;


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
            yield return null;
        }
    }

    private void FixedUpdate()
    {
        UpdateTargetDirection();
        RotateTowardsTarget();
        SetVelocity();
        if (!isFiring && !isCooldown && PlayerDetected() && attackType == AttackType.Laser)
        {
            StartCoroutine(FireLaser());
        }
        
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
        if(isFiring || targetDirection == Vector2.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(transform.up, targetDirection);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        rb.SetRotation(rotation);
    }

    private void SetVelocity()
    {
        if(leapRest || isFiring || isLeaping || targetDirection == Vector2.zero || (attackType == AttackType.Ranged && isCooldown))
        {
            rb.linearVelocity = Vector2.zero;
        }
        else if (hasLeaped)
        {
            rb.linearVelocity = transform.up * leapSpeed;
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
        //if (collision.gameObject == GameObject.FindGameObjectWithTag("Player") && attackType == AttackType.Ranged && !isCooldown)
        //{

        //    PlayEnemyAttackAnimation();

        //    StartCoroutine(AttackingCooldown());
        //}
 
        // When the attack type is melee, the enemy will leap towards the player
        if (collision.CompareTag("Player") && attackType == AttackType.Melee && !isCooldown)
        {
            StartCoroutine(LeapAttack());
        }

    }
    private void OnTriggerExit2D(Collider2D collision) //Player exits attack zone
    {
        //if (collision.gameObject == GameObject.FindGameObjectWithTag("Player") && attackType == AttackType.Ranged && isCooldown)
        //{
        //    PlayEnemyWalkingAnimation();
        //}
      
        if (collision.gameObject == GameObject.FindGameObjectWithTag("Player") && attackType == AttackType.Melee && !isLeaping && !hasLeaped || leapRest)
        {
            PlayEnemyWalkingAnimation();
        }
        if (collision.CompareTag("Player") && attackType == AttackType.Ranged && isCooldown)
        {
            AnimatorStateInfo animState = enemyAnimator.GetCurrentAnimatorStateInfo(0);
            if (!animState.IsName("blob_attack")) // Replace with your ranged attack animation name if different
            {
                PlayEnemyWalkingAnimation();
            }
        }
    }
    private void OnTriggerStay2D(Collider2D collision) //Player stays in attack zone
    {

        if (collision.gameObject == GameObject.FindGameObjectWithTag("Player") && attackType == AttackType.Ranged && !isCooldown)
        {
            PlayEnemyAttackAnimation();

            StartCoroutine(AttackingCooldown());
        }
        if (collision.gameObject == GameObject.FindGameObjectWithTag("Player") && attackType == AttackType.Ranged && isCooldown)
        {
            PlayEnemyWalkingAnimation();
        }
    }
    private IEnumerator LeapAttack()
    {
        isCooldown = true;
        StartCoroutine(AttackingCooldown());
        isLeaping = true;

    
        PlayEnemyAttackAnimation();
        // Start Leap Charge
        yield return new WaitForSeconds(leapChargeDuration);
        hasLeaped = true;
        isLeaping = false;


        // Leap towards the player
        yield return new WaitForSeconds(leapDuration);
       
        // Reset the leap state
        hasLeaped = false;
        leapRest = true;

        yield return new WaitForSeconds(leapCooldown);
        leapRest = false;
    }

    

    IEnumerator AttackingCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false;
    }



    private void ShootProjectile()
    {
        Debug.Log("Shooting Projectile");
        foreach (Transform pos in projPos)
        {
            // Use the spawn point's rotation instead of parent's rotation
            GameObject newProjectile = Instantiate(projPrefab, pos.position, pos.rotation);
            Rigidbody2D rbProj = newProjectile.GetComponent<Rigidbody2D>();
            Bullet bullet = newProjectile.GetComponent<Bullet>();

            if (bullet != null)
            {
                bullet.enemyBulletDamage = damage;
                bullet.bulletCollision = bulletCollision;
                if (bullet.bulletCollision)
                {
                    Debug.Log("Bullet Collision Off");
                }
            }
            isCooldown = true;
            PlayEnemyWalkingAnimation();
            if (rbProj != null)
            {
                // Use the spawn point's up direction instead of parent's up
                rbProj.linearVelocity = pos.up * projSpeed;
            }
        }
    }
    //Player detection for laser
    private bool PlayerDetected()
    {
        foreach (Transform pos in projPos)
        {
            RaycastHit2D hit = Physics2D.Raycast(pos.position, pos.up, Mathf.Infinity, playerLayer);
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                Debug.Log("Player Detected from position: " + pos.name);
                return true;
            }
        }
        return false;
    }

    private IEnumerator FireLaser()
    {
        isFiring = true;
        foreach (LineRenderer line in laserLines) // Enable all laser lines
        {
            line.enabled = true;
        }

        float startTime = Time.time;

        while (Time.time < startTime + laserDuration)
        {
            for (int i = 0; i < projPos.Length; i++)
            {
                Transform pos = projPos[i];
                LineRenderer line = laserLines[i];
                // Set the laser line's start and end positions
                Vector3 startPos = pos.position;
                startPos.z = 0;
                RaycastHit2D hit = Physics2D.Raycast(startPos, pos.up, Mathf.Infinity, laserHitLayers);

                Vector3 endPos;
                // Check if the raycast hit something
                if (hit.collider != null)
                {
                    endPos = hit.point;
                    endPos.z = 0;
                    if (hit.collider.CompareTag("Player"))
                    {
                        // Player hit and apply damage
                        HealthController hc = hit.collider.GetComponent<HealthController>();
                        if (hc != null)
                        {
                            hc.TakeDamage(laserDamage);
                        }
                    }
                }
                else
                {
                    endPos = startPos + pos.up * 100f;
                    endPos.z = 0;
                }

                line.SetPosition(0, startPos);
                line.SetPosition(1, endPos);
            }
            yield return null;
        }

        foreach (LineRenderer line in laserLines) // Disable all laser lines
        {
            line.enabled = false;
        }

        isFiring = false;
        isCooldown = true;
        yield return new WaitForSeconds(laserCooldown);
        isCooldown = false;
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
                speed = 1f;
                attackCooldown = 1;
                rotationSpeed = 500;
                rangedAttackRange = 0f;
                attackType = AttackType.Melee;
                leapSpeed = 5f;
                leapDuration = 0.3f;
                leapChargeDuration = 0.9f;
                bulletCollision = false;

                leapSpeed = 5f;
                leapDuration = 0.3f;
                leapChargeDuration = 0.95f;
                leapCooldown = 1f;
                break;
            case EnemyType.Sumelse:
                health = 3;
                damage = 1;
                speed = 0.5f;
                attackCooldown = 1;
                rotationSpeed = 500;
                rangedAttackRange = 2f;
                projSpeed = 3f;
                attackType = AttackType.Ranged;
                bulletCollision = false;
                break;
            case EnemyType.Laser:
                health = 3;
                damage = 1;
                speed = 0f;
                attackCooldown = 1;
                rotationSpeed = 500;
                rangedAttackRange = 2f;
                projSpeed = 3f;
                attackType = AttackType.Laser;
                bulletCollision = false;
                laserDuration = 2f;
                laserCooldown = 3f;
                laserDamage = 2;
                
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
            case EnemyType.Sumelse:
                enemyAnimator.Play("blob_idle");
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
            case EnemyType.Sumelse:
                enemyAnimator.Play("blob_attack");
                break;
        }
    }
}
