using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage = 1;
    //public GameObject hitEffect;
    private void OnCollisionEnter2D(Collision2D collision) //Objects
    {
        //GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        //Destroy(hitEffect, 5f);
        if (collision.gameObject != GameObject.FindGameObjectWithTag("Player"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) //Enemies
    {
        if (collision.gameObject.TryGetComponent<EnemyAI>(out EnemyAI enemyComponent))
        {
            enemyComponent.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
