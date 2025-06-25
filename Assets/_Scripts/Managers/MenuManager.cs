using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject buttonPanel;

    void Start()
    {
        buttonPanel.SetActive(false);
        StartCoroutine(ShowButtons());
    }

    IEnumerator ShowButtons()
    {
        yield return new WaitForSeconds(3f);
        buttonPanel.SetActive(true);
    }
}
