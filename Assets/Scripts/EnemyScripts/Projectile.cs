using UnityEngine;

public class Projectile : MonoBehaviour
{
    private void Start()
    {
        Destroy(gameObject, 5f); // auto-destroy after 5 seconds to avoid clutter
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Projectile hit: " + collision.gameObject.name);

        if (collision.gameObject.CompareTag("Player"))
        {
            //collision.gameObject.GetComponent<HealthController>()?.TakeDamage(damage);
            Debug.Log("Hit player with projectile");
            Destroy(gameObject);
        }

    }
}
