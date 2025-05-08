using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class ButtonManager : MonoBehaviour
{
    public AudioSource hoverButtonOn;
    public AudioSource hoverButtonOff;
    public GameObject indicator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayGame()
    {
        SceneManager.LoadScene("Tutorial 1"); 
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void OnPointerEnter()
    {
        if (indicator != null)
        {
            indicator.SetActive(true);
            hoverButtonOn.Play();
        }
    }

    public void OnPointerExit()
    {
        if (indicator != null)
        {
            indicator.SetActive(false);
            hoverButtonOff.Play();
        }
    }
}
