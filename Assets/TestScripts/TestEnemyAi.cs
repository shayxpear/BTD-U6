using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEnemyAi : MonoBehaviour
{
    private Transform target;
    public float speed;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }
    private void Update()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
    }
}
