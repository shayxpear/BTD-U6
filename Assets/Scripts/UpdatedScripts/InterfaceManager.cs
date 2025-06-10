using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceManager : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private GameObject PauseMenu;

    [Header("Inventory")]
    [SerializeField] private GameObject InventoryMenu;

    [Header("Rhythm Bar")]
    [SerializeField] private GameObject RhythmUI;

    private BeatManager beatManager;

    void Start()
    {
        beatManager = GameObject.Find("BeatManager").GetComponent<BeatManager>();
    }

    void Update()
    {
        RhythmModule();
    }

    public void RhythmModule()
    {
        if(beatManager.playingSong)
        {
            
        }
        
    }
}
