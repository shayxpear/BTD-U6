using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public enum AttackType {Melee, Ranged, Laser}
    public enum EnemyType {Rat, Blobby, Laser, BlobbyMini}
    public EnemyType CurrentEnemyType => enemyType;

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

    [Header("Split-On-Death Settings")]
    [SerializeField] private GameObject[] blobbyMiniPrefabs;
    [SerializeField] private float miniSpawnRadius = 0.5f;
    [SerializeField] private float miniExplosionForce = 5f;


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
            if (!animState.IsName("blob_attack"))
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
        Debug.Log("Player Detected");
        RaycastHit2D hit = Physics2D.Raycast(projPos[0].position, projPos[0].up, Mathf.Infinity, playerLayer);
        return hit.collider != null;
    }

    private IEnumerator FireLaser()
    {
        isFiring = true;
        laserLines[0].enabled = true;
        float startTime = Time.time;
        float damageTickInterval = 0.5f;
        float tickTimer = 0f;

        while (Time.time < startTime + laserDuration)
        {
            Vector3 startPos = projPos[0].position;
            startPos.z = 0;

            // Cast a ray against all layers in laserHitLayers.
            RaycastHit2D hit = Physics2D.Raycast(startPos, projPos[0].up, Mathf.Infinity, laserHitLayers);
            Vector3 endPos;

            if (hit.collider != null)
            {
                endPos = hit.point;
                endPos.z = 0;
            }
            else
            {
                endPos = startPos + projPos[0].up * 100f;
                endPos.z = 0;
            }

            laserLines[0].SetPosition(0, startPos);
            laserLines[0].SetPosition(1, endPos);

            // Accumulate time and do damage tick when interval is met.
            tickTimer += Time.deltaTime;
            if (tickTimer >= damageTickInterval)
            {
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    HealthController hc = hit.collider.GetComponent<HealthController>();
                    if (hc != null)
                    {
                        hc.TakeDamage(laserDamage);
                    }
                }
                tickTimer = 0f;
            }

            yield return null;
        }
        //Disable the laser after the duration
        laserLines[0].enabled = false;
        PlayEnemyWalkingAnimation();
        isFiring = false;
        isCooldown = true;
        //Start cooldown
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

    private void OnDestroy()
    {
        // Find the RoomDetection instance in the scene.
        RoomDetection roomDetection = Object.FindAnyObjectByType<RoomDetection>();

        // Determine if this enemy is inside the room detection area.
        bool isInsideRoom = false;
        if (roomDetection != null)
        {
            Collider2D roomCollider = roomDetection.GetComponent<Collider2D>();
            if (roomCollider != null && roomCollider.OverlapPoint(transform.position))
            {
                isInsideRoom = true;
            }
        }

        // If this is a bigblob, spawn mini blobs.
        if (enemyType == EnemyType.Blobby && blobbyMiniPrefabs != null && blobbyMiniPrefabs.Length > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                Debug.Log("Spawning mini blobs");
                var prefabToSpawn = blobbyMiniPrefabs[i % blobbyMiniPrefabs.Length];
                var spawnDirection = Random.insideUnitCircle.normalized;
                var spawnPosition = (Vector2)transform.position + spawnDirection * miniSpawnRadius;
                GameObject miniBlob = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                Rigidbody2D rbMini = miniBlob.GetComponent<Rigidbody2D>();
                if (rbMini != null)
                {
                    rbMini.AddForce(spawnDirection * miniExplosionForce, ForceMode2D.Impulse);
                }

                // If the mini blob spawns inside the room, update the enemy count.
                if (roomDetection != null)
                {
                    Collider2D roomCollider = roomDetection.GetComponent<Collider2D>();
                    if (roomCollider != null && roomCollider.OverlapPoint(miniBlob.transform.position))
                    {
                        roomDetection.AddEnemy();
                    }
                }
            }
        }

        
        if (roomDetection != null && isInsideRoom && enemyType == EnemyType.Blobby)
        {
            roomDetection.RemoveEnemy();
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
            case EnemyType.Blobby:
                health = 3;
                damage = 1;
                speed = 0f;
                attackCooldown = 2;
                rotationSpeed = 0;
                rangedAttackRange = 2f;
                projSpeed = 2f;
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
            case EnemyType.BlobbyMini:
                health = 3;
                damage = 1;
                speed = 0f;
                attackCooldown = 2;
                rotationSpeed = 0;
                rangedAttackRange = 2f;
                projSpeed = 3f;
                attackType = AttackType.Ranged;
                bulletCollision = false;
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
            case EnemyType.Blobby:
                enemyAnimator.Play("blob_idle");
                break;
            case EnemyType.BlobbyMini:
                enemyAnimator.Play("blob_walk");
                break;
            case EnemyType.Laser:
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
            case EnemyType.Blobby:
                enemyAnimator.Play("blob_attack");
                break;
            case EnemyType.BlobbyMini:
                enemyAnimator.Play("blob_attack");
                break;
            case EnemyType.Laser:
                enemyAnimator.Play("rat_attack");
                break;
        }
    }
}
