using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [HideInInspector] public int bulletDamage;

    private void OnCollisionEnter2D(Collision2D collision) //Objects
    {
        //GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        //Destroy(hitEffect, 5f);
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthController>().TakeDamage(bulletDamage);
        }
        Destroy(gameObject);

    }
}

