using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Player Stats")]
    [SerializeField] private int health;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashCooldown;

    [Header("Sprite Handler")]
    [SerializeField] private PlayerSpriteHandler spriteHandler;

    bool isCooldown;

    private Rigidbody2D rb;
    private Collider2D playerCollider; //Prevents player from going through walls when dashing, not used to test if enemies have attacked the player

    private Transform playerSpriteTransform;
    private BetterNoteManager noteManager;
    public bool CanDash { get; private set; }
    public int GetPlayerHealth => health;
    private float currentDashTime;

    Vector2 movement;
    Vector2 playerScreenPosition;
    Vector2 mousePosition;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();

        playerSpriteTransform = GameObject.Find("SpriteController").GetComponent<Transform>();

        noteManager = FindFirstObjectByType<BetterNoteManager>();
        CanDash = true;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        playerScreenPosition = Camera.main.WorldToScreenPoint(playerSpriteTransform.transform.position);
        mousePosition = Input.mousePosition;

        PlayerMovement();
        PlayerDash();
    }
    private void PlayerDash()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (!isCooldown && CanDash && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)) && (movement.x != 0 || movement.y != 0))
        {
            StartCoroutine(Dash(new Vector2(movement.x, movement.y).normalized));
        }
    }

    IEnumerator Dash(Vector2 direction)
    {
        CanDash = false;
        isCooldown = true;
        currentDashTime = dashTime;
        playerCollider.excludeLayers = LayerMask.GetMask("Enemies", "Bullets", "CollisionBullets"); //Dodge layers
        rb.excludeLayers = LayerMask.GetMask("Enemies", "Bullets", "CollisionBullets"); //Exclude layers

        switch (playerScreenPosition.y + 100 < mousePosition.y)
        {
            case true:
                spriteHandler.playerBody = PlayerSpriteHandler.PlayerBody.DODGING_B;

                break;
            case false:
                spriteHandler.playerBody = PlayerSpriteHandler.PlayerBody.DODGING_F;
                break;
        }

        while (currentDashTime > 0f)
        {
            currentDashTime -= Time.deltaTime; // Lower the dash timer each.

            rb.linearVelocity = direction * dashSpeed; // Dash in the direction that was held down.

            yield return null; // Returns out of the coroutine this frame so we don't hit an infinite loop.
        }
        rb.linearVelocity = new Vector2(0f, 0f); // Stop dashing. 
        
        CanDash = true;
        playerCollider.excludeLayers = LayerMask.GetMask("Nothing");
        rb.excludeLayers = LayerMask.GetMask("Nothing");
        dashCooldown = (60f/noteManager.bpm); // Reset the dash cooldown to the current BPM of the song.
        yield return new WaitForSeconds(dashCooldown);
        isCooldown = false;
    }


    private void PlayerMovement()
    {
        //Get Player Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        //Move Rigidbody
        if (CanDash)
        {
            rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
            switch (playerScreenPosition.y+100 < mousePosition.y)
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
