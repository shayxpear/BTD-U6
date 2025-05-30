using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public GameObject enemyPrefab;
    public int difficulty;
}

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Room Reference")]
    [SerializeField] private RoomDetection roomDetection;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Enemy Pool")]
    [SerializeField] private List<EnemySpawnInfo> enemyPool;

    [Header("Stage Settings")]
    [SerializeField] private int currentStage = 1;
    [SerializeField] private int maxDifficultyForStage = 2;

    [Header("NoteManager")]
    [SerializeField] private BetterNoteManager noteManager;

    [HideInInspector] public bool enemiesHaveSpawned;

    private int numberOfEnemiesSpawned = 0;
    private bool hasSpawnedEnemies = false;

    private void Start()
    {
        if (roomDetection == null)
        {
            roomDetection = FindFirstObjectByType < RoomDetection >();
        }
        if (noteManager == null)
        {
            noteManager = FindFirstObjectByType<BetterNoteManager>();
        }
    }
    private void FixedUpdate()
    {
        if (roomDetection != null && !hasSpawnedEnemies)
        {
            roomDetection.enemiesInRange = 1;
        }
        if (!hasSpawnedEnemies && noteManager.playedIntro && !noteManager.trackHolder.introRiff.isPlaying)
        {
            hasSpawnedEnemies = true;
            StartSpawning();
            
        }
    }
    public void StartSpawning()
    {
        int enemiesToSpawn = NumEnemiesSpawn();
        StartCoroutine(SpawnEnemies(enemiesToSpawn));
        if (roomDetection != null)
        {

            roomDetection.enemiesInRange = 0;
            roomDetection.enemiesInRange = numberOfEnemiesSpawned; // Update the count of enemies in range
        }
    }

    public int NumEnemiesSpawn()
    {
        int baseEnemies = 1; // Base number of enemies per stage
        int extraPerStage = 1;
        return baseEnemies + (currentStage - 1) * extraPerStage;
    }
    private IEnumerator SpawnEnemies(int count)
    {

        List<Transform> availablePoints = new List<Transform>(spawnPoints);
       
        for (int i = 0; i < count; i++)
        {
            Transform spawnPoint;

            if (i < availablePoints.Count)
            {
                spawnPoint = availablePoints[i];
            }
            else
            {
                // If we run out, reuse points randomly
                spawnPoint = availablePoints[Random.Range(0, availablePoints.Count)];
            }
            // Pick an enemy from the pool with appropriate difficulty
            List<EnemySpawnInfo> validEnemies = enemyPool.FindAll(e => e.difficulty <= maxDifficultyForStage);
            if (validEnemies.Count == 0)
            {
                Debug.LogWarning("No enemies available for this stage difficulty!");
                continue;
            }

            EnemySpawnInfo chosenEnemy = validEnemies[Random.Range(0, validEnemies.Count)];

            // Instantiate the enemy
            GameObject newEnemy = Instantiate(chosenEnemy.enemyPrefab, spawnPoint.position, Quaternion.identity);
            //numberOfEnemiesSpawned++;
            

            // play intro animation
            Animator enemyAnimator = newEnemy.GetComponentInChildren<Animator>();
            if (enemyAnimator != null)
            {
                enemyAnimator.SetTrigger("Spawn");
            }

           // add a small delay between spawns
            yield return new WaitForSeconds(0.5f);
        }
    }
    // stage advancement difficulty increase
    public void AdvanceStage()
    {
        currentStage++;
        maxDifficultyForStage++;
        hasSpawnedEnemies = false;
    }
}