using UnityEngine;

public class EnemySprite : MonoBehaviour
{
    private Transform parentTransform;
    
    [SerializeField] private EnemyController enemyController; // Drag the child object with EnemyController here

    public void Attack()
    {
        if (enemyController != null)
        {
            enemyController.Attack();
        }
    }
    private void Start()
    {
        parentTransform = transform.parent;
    }

    private void LateUpdate()
    {
        // Counter-rotate the sprite to keep it upright
        transform.rotation = Quaternion.identity;
    }
}

