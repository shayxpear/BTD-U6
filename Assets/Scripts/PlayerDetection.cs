using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    public bool PlayerDetected { get; private set; }
    public Vector2 PlayerDirection { get; private set; }

    [Header("Detection")]
    [SerializeField] private float detectionDistance;
    [SerializeField] private GameObject detectionCollider;

    private RoomDetection roomDetection;

    private Transform player;

    private void Start()
    {
        StartCoroutine(FindPlayer());
        roomDetection = detectionCollider.GetComponent<RoomDetection>();
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

    private void Update()
    {
        if (roomDetection.playerInRange)
        {
            Vector2 enemyToPlayerVector = player.position - transform.position;
            PlayerDirection = enemyToPlayerVector.normalized;
            Debug.Log("Player is in range — Attack!");
            PlayerDetected = true;
        }
        else
        {
            PlayerDetected = false;
        }
    }

}
