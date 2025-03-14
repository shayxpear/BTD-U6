using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Scriptable Objects")]
    public PlayerStats playerStats;
    public EnemyStats[] enemyStatsList;  // Array to manage different enemy types

    public AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            musicSource = GetComponent<AudioSource>();
            LoadScriptableObjects();
            if (musicSource != null && !musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadScriptableObjects()
    {
        // Load PlayerStats once
        playerStats = Resources.Load<PlayerStats>("Player");
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats not found in Resources!");
        }

        // Load all EnemyStats once
        enemyStatsList = Resources.LoadAll<EnemyStats>("/Enemies");
        if (enemyStatsList.Length == 0)
        {
            Debug.LogError("No EnemyStats found in Resources/Enemies folder!");
        }
    }

    // Example method to get enemy stats by type
    public EnemyStats GetEnemyStats(EnemyType enemyType)
    {
        foreach (var enemyStats in enemyStatsList)
        {
            if (enemyStats.enemyType == enemyType)
            {
                return enemyStats;
            }
        }

        Debug.LogWarning($"EnemyStats for {enemyType} not found!");
        return null;
    }
}
