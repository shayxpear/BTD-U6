using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool isPaused = false;
    public AudioSource audioSource;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
        else
        {
            Time.timeScale = 1f;

            if (audioSource != null && !audioSource.isPlaying)
            {
                audioSource.UnPause();
            }
        }
    }
}
