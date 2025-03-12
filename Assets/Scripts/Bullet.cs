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
        if (collision.gameObject.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<HealthController>().TakeDamage(damage);
        }
        Destroy(gameObject);

    }
}
