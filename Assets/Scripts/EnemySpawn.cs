using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public GameObject[] enemyPrefabs;
    public int minEnemies = 2;
    public int maxEnemies = 5;
    void Start()
    {
        //SpawnEnemies();
    }

    //void SpawnEnemies()
   // {
        //int enemyCount = Random.Range(minEnemies, maxEnemies + 1); // Random number of enemies

        //for (int i = 0; i < enemyCount; i++)
       // {
            //Vector2 randomPosition = new Vector2(
            //    Random.Range(-roomSize.x / 2, roomSize.x / 2),
            //    Random.Range(-roomSize.y / 2, roomSize.y / 2)
           // );

           // int randomEnemyIndex = Random.Range(0, enemyPrefabs.Length); // Pick a random enemy
          //  Instantiate(enemyPrefabs[randomEnemyIndex], randomPosition, Quaternion.identity);
      //  }
   // }
}
