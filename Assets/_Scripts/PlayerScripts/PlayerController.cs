using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Player Stats")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float dashTime; //should never be less than or equal to 1
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashCooldown;

    [Header("Handlers")]
    [SerializeField] private PlayerSpriteHandler spriteHandler;
    [SerializeField] private InputHandler inputHandler;

    [Header("Managers")]
    [SerializeField] private BeatManager beatManager;

    private Rigidbody2D rb;
    private Collider2D playerCollider; //Prevents player from going through walls when dashing, not used to test if enemies have attacked the player

    private Transform playerSpriteTransform;

    bool canDash = true;

    Vector2 movement;
    Vector2 playerScreenPosition;
    Vector2 mousePosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        playerSpriteTransform = GameObject.Find("SpriteController").GetComponent<Transform>();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        playerScreenPosition = Camera.main.WorldToScreenPoint(playerSpriteTransform.transform.position);
        mousePosition = Input.mousePosition;
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        PlayerMovement();
        PlayerDodge();
        //Dash();
    }
    private void PlayerDodge()
    {
        if (inputHandler.successfulDodge && canDash)
        {
            StartCoroutine(Dodging());
        }
    }

    IEnumerator Dodging()
    {
        inputHandler.successfulDodge = false;
        canDash = false;
        spriteHandler.bodyAnimator.speed = 1 * (dashTime * (beatManager.currentBPM / 60f));
        rb.linearVelocity = new Vector2(movement.x, movement.y).normalized * dashSpeed;

        playerCollider.excludeLayers = LayerMask.GetMask("Enemies", "Bullets", "CollisionBullets"); //Dodge layers
        rb.excludeLayers = LayerMask.GetMask("Enemies", "Bullets", "CollisionBullets"); //Exclude layers
        switch (playerScreenPosition.y + 200 < mousePosition.y)
        {
            case true:
                spriteHandler.playerBody = PlayerSpriteHandler.PlayerBody.DODGING_B;

                break;
            case false:
                spriteHandler.playerBody = PlayerSpriteHandler.PlayerBody.DODGING_F;
                break;
        }

        yield return new WaitForSeconds(60f/ (dashTime * beatManager.currentBPM));
        rb.linearVelocity = new Vector2(0f, 0f);
        playerCollider.excludeLayers = LayerMask.GetMask("Nothing");
        rb.excludeLayers = LayerMask.GetMask("Nothing");
        spriteHandler.bodyAnimator.speed = 1;
        canDash = true;
    }

    private void PlayerMovement()
    {
        //Move Rigidbody
        if (canDash)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
            switch (playerScreenPosition.y + 200 < mousePosition.y)
            {
                case true: // back
                    switch ((movement.x != 0 | movement.y != 0))
                    {
                        case true | true:
                            spriteHandler.playerBody = PlayerSpriteHandler.PlayerBody.RUNNING_B;
                            spriteHandler.playerHead = PlayerSpriteHandler.PlayerHead.RUNNING_B;
                            break;
                        case false | false:
                            spriteHandler.playerBody = PlayerSpriteHandler.PlayerBody.IDLE_B;
                            spriteHandler.playerHead = PlayerSpriteHandler.PlayerHead.IDLE_B;
                            break;
                    }
                    break;
                case false: // top
                    switch ((movement.x != 0 | movement.y != 0))
                    {
                        case true | true:
                            spriteHandler.playerBody = PlayerSpriteHandler.PlayerBody.RUNNING_F;
                            spriteHandler.playerHead = PlayerSpriteHandler.PlayerHead.RUNNING_F;
                            break;
                        case false | false:
                            spriteHandler.playerBody = PlayerSpriteHandler.PlayerBody.IDLE_F;
                            spriteHandler.playerHead = PlayerSpriteHandler.PlayerHead.IDLE_F;
                            break;
                    }
                    break;
            }
            switch (movement.x) // Flip player depending if the player is moving left or right
            {
                case -1:
                    playerSpriteTransform.localScale = new Vector2(-1f, 1f);
                    break;
                case 1:
                    playerSpriteTransform.localScale = new Vector2(1f, 1f);
                    break;
            }
        }


    }
}
