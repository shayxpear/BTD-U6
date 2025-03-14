using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject Controller;
    void Start()
    {
        Instantiate(Controller);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
