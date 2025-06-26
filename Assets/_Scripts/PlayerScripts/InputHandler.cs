using UnityEngine;
public class InputHandler : MonoBehaviour
{
    [Header("Player Movement")]
    public KeyCode DodgeInput = KeyCode.LeftShift;
    public KeyCode AltDodgeInput = KeyCode.Space;
    [Header("Player Attack")]
    public KeyCode LeftShoot = KeyCode.Mouse0;
    public KeyCode RightShoot = KeyCode.Mouse1;

    [Header("Input Conditions (DEBUG)")]
    public bool successfulDodge;
    public bool successfulLeftShoot;
    public bool successfulRightShoot;
    public Vector2 getPlayerMovement;
    public void Update()
    {
        getPlayerMovement.x = Input.GetAxisRaw("Horizontal");
        getPlayerMovement.y = Input.GetAxisRaw("Vertical");
    }
    public bool GetDodgeDown()
    {
        return Input.GetKeyDown(DodgeInput) || Input.GetKeyDown(AltDodgeInput);
    }

    public bool GetLeftShootDown()
    {
        return Input.GetKeyDown(LeftShoot);
    }

    public bool GetRightShootDown()
    {
        return Input.GetKeyDown(RightShoot);
    }
}