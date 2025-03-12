using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public EnemyData enemyData;
    public enum MobType { Melee, Ranged }
    public MobType mobType = MobType.Melee;
    public float attackDistance = 2f; // Distance at which the enemy attacks
    public float attackCooldown = 1f; // Time between attacks

    private Transform player;
    private bool isCooldown;
    private int currentEnemyHp;

    private void Start()
    {
        isCooldown = false;
        player = GameObject.FindWithTag("Player").transform;
    }
    private void OnTriggerEnter2D(Collider2D collision) //Enemies
    {
        Debug.Log(collision);
        if (collision.gameObject == GameObject.FindGameObjectWithTag("Player") && !isCooldown)
        {
            Attack();
            isCooldown = true;
            StartCoroutine(AttackingCooldown());
        }
    }

    IEnumerator AttackingCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        isCooldown = false;
    }

    private void Attack()
    {
        if (mobType == MobType.Melee)
        {
            // Melee attack logic (e.g., damage player in range)
            Debug.Log("Melee Attack");
            player.GetComponent<HealthController>().TakeDamage(enemyData.damage);
        }
        else if (mobType == MobType.Ranged)
        {
            // Ranged attack logic (e.g., shoot projectile at player)
            Debug.Log("Ranged Attack");
        }
    }

    // Example of handling damage
    public void TakeDamage(int damageAmount)
    {
        if (enemyData != null)
        {
            currentEnemyHp -= damageAmount;
            Debug.Log($"{enemyData.enemyName} took {damageAmount} damage. Remaining health: {currentEnemyHp}");
        }

        if (currentEnemyHp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
