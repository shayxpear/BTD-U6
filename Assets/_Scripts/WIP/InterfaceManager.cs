using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject InventoryMenu;

    [Header("Modules")]
    [SerializeField] private GameObject RhythmUI;
    [SerializeField] private GameObject FreestyleUI;

    private BeatManager beatManager;

    void Start()
    {
        beatManager = GameObject.Find("BeatManager").GetComponent<BeatManager>();
    }

    void Update()
    {
        RhythmModule();
        FreestyleModule();
    }

    public void RhythmModule()
    {
        switch(beatManager.songStage)  
        {
            case BeatManager.SongStage.INTRO:
                RhythmUI.SetActive(false);
                break;
            case BeatManager.SongStage.INTRO_TRANSITION:
                RhythmUI.SetActive(false);
                break;
            case BeatManager.SongStage.RIFF:
                RhythmUI.SetActive(true);
                break;
            case BeatManager.SongStage.OUTRO:
                RhythmUI.SetActive(false);
                break;
            case BeatManager.SongStage.BACKGROUND:
                RhythmUI.SetActive(false);
                break;

        }
    }
    
    public void FreestyleModule()
    {
        switch (beatManager.songStage)
        {
            case BeatManager.SongStage.INTRO:
                FreestyleUI.SetActive(true);
                break;
            case BeatManager.SongStage.INTRO_TRANSITION:
                FreestyleUI.SetActive(true);
                break;
            case BeatManager.SongStage.RIFF:
                FreestyleUI.SetActive(false);
                break;
            case BeatManager.SongStage.OUTRO:
                FreestyleUI.SetActive(false);
                break;
            case BeatManager.SongStage.BACKGROUND:
                FreestyleUI.SetActive(false);
                break;

        }
    }
}
