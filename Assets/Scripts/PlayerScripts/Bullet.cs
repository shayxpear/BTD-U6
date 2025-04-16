using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public int bulletDamage;
    [HideInInspector] public int enemyBulletDamage;
    [HideInInspector] public bool bulletCollision;
    private string bulletLayerName = "Bullets";

    private void Start()
    {
        // Set initial layer when bullet spawns
        SetBulletLayer();
    }

    // Switches the bullets layer depending on weather or not they can collide with eachother
    private void SetBulletLayer()
    {
        if (bulletCollision)
        {
            gameObject.layer = LayerMask.NameToLayer("CollisionBullets");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer(bulletLayerName);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) //Objects
    {
        //GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        //Destroy(hitEffect, 5f);
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<HealthController>().TakeDamage(bulletDamage);
        }
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<HealthController>().TakeDamage(enemyBulletDamage);
        }
        Destroy(gameObject);

    }
}
