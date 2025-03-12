using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    public bool PlayerDetected { get; private set; }
    public Vector2 PlayerDirection { get; private set; }

    [Header("Detection")]
    [SerializeField] private float detectionDistance;

    private Transform player;

    private void Awake()
    {
        player  = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        CheckDistance();
    }

    private void CheckDistance()
    {
        Vector2 enemyToPlayerVector = player.position - transform.position;
        PlayerDirection = enemyToPlayerVector.normalized;

        if(enemyToPlayerVector.magnitude <= detectionDistance)
        {
            PlayerDetected = true;
        }
        else
        {
            PlayerDetected = false;
        }
    }
}
