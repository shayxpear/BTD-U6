using UnityEngine;

[System.Serializable]
public abstract class CharacterStats : ScriptableObject
{
    [Header("General Attributes")]
    public int damage;
    public int health;
    public float speed;
}

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/PlayerStats")]
public class PlayerStats : CharacterStats
{
    [Header("Player Attributes")]
    public int playerLevel;
    public float dashSpeed;
    public float dashTime;

    [Header("Guitar Attributes")]
    public int bulletForce;
    public int bulletScale;
    public float missCooldown;

}

public enum EnemyType { Rat, Blob }
public enum AttackType { Melee, Ranged}
[CreateAssetMenu(fileName = "EnemyStats", menuName = "Stats/EnemyStats")]
public class EnemyStats : CharacterStats
{
    [Header("Enemy Type")]
    public EnemyType enemyType;
    public AttackType attackType;

    [Header("Enemy Attributes")]
    public int experienceValue;
    public float rotationSpeed;
    public float attackCooldown;

    [Header("Enemy Detection")]
    public float obstacleCheckCircleRadius;
    public float obstacleCheckDistance;
    public LayerMask obstacleLayerMask; //in order to ignore everything but player
}

