using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private GameObject playerHealthPrefab;
    [SerializeField] private int health;
    [SerializeField] private Transform healthTransform;

    private GameObject[] healthArray;

    private void Awake()
    {
        DisplayHealth();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(1);
        }
    }

    public void DisplayHealth()
    {
        healthArray = new GameObject[health];
        for (int h = 0; h < health; h++)
        {
            GameObject healthObj = Instantiate(playerHealthPrefab, healthTransform);
            healthArray[h] = healthObj;
        }
    }
    
    public void TakeDamage(int damage)
    {
        health -= damage;
        Destroy(healthArray[health]);
        health = Mathf.Clamp(health, 0, health);
    }

    public int GetCurrentHealth()
    {
        return health;
    }

    IEnumerator Respawn()
    {
        Destroy(GameObject.Find("PlayerPrefab"));
        Destroy(GameObject.Find("Managers"));
        Debug.Log("Player has died.");
        SceneManager.LoadScene("Tutorial 1");
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        yield return new WaitForSeconds(0.9f);

    }
}
