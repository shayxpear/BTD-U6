using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    public bool PlayerDetected { get; private set; }
    public Vector2 PlayerDirection { get; private set; }

    private RoomDetection roomDetection;

    private Transform player;

    private void Start()
    {
        StartCoroutine(FindPlayer());
        StartCoroutine(FindDetectionCollider());
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

    private IEnumerator FindDetectionCollider()
    {
        while (roomDetection == null)
        {
            GameObject detectionCollider = GameObject.FindWithTag("DetectionCollider");
            if (detectionCollider != null)
            {
                roomDetection = detectionCollider.GetComponent<RoomDetection>();
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
           // Debug.Log("Player is in range � Attack!");
            PlayerDetected = true;
        }
        else
        {
            PlayerDetected = false;
        }
    }

}
