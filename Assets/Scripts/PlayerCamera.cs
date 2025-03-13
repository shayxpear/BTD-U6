using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Components")]
    public Transform camTarget;
    [Header("Camera Displacement")]
    public float camDisplacementMultiplier = 0.15f;

    private GameObject playerObject;

    private void Start()
    {
        playerObject = GameObject.Find("Player");
        camTarget = playerObject.transform;
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 cameraDisplacement = (mousePosition - camTarget.position) * camDisplacementMultiplier;

        Vector3 finalCamPosition = camTarget.position + cameraDisplacement;
        finalCamPosition.z = -1;
        transform.position = finalCamPosition;
    }
}
