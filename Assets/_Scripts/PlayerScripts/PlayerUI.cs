using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject crosshair;

    public Sprite[] crosshairSprites;
    public Sprite crosshairSprite;

    private bool paused;

    Vector2 mousePosition;

    Transform crosshairTransform;

    void Start()
    {
        Cursor.visible = false;
        crosshairTransform = crosshair.GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        PlayerCrosshair();
    }

    private void PlayerCrosshair()
    {
        mousePosition = Input.mousePosition;
        Vector2 mouseCursorPos = Camera.main.ScreenToWorldPoint(mousePosition);
        crosshairTransform.position = mouseCursorPos;
    }
}
