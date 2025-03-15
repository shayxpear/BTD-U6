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

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider; //Prevents player from going through walls when dashing, not used to test if enemies have attacked the player

    private GameObject spriteController;
    private GameObject guitarController;
    private Transform playerSpriteTransform;
    private SpriteRenderer guitarSpriteRenderer;

    public bool CanDash { get; private set; }
    public int GetPlayerHealth => health;
    private float currentDashTime;

    Vector2 movement;
    Vector2 playerScreenPosition;
    Vector2 mousePosition;

    private void Start()
    {
        spriteController = GameObject.Find("SpriteController");
        guitarController = GameObject.Find("GuitarController");

        rb = GetComponent<Rigidbody2D>();
        animator = spriteController.GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();

        playerSpriteTransform = spriteController.GetComponent<Transform>();
        guitarSpriteRenderer = guitarController.GetComponent<SpriteRenderer>();
        
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

        if (CanDash && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)) && (movement.x != 0 || movement.y != 0))
        {
            StartCoroutine(Dash(new Vector2(movement.x, movement.y).normalized));
        }
    }

    IEnumerator Dash(Vector2 direction)
    {
        guitarSpriteRenderer.enabled = false;
        CanDash = false;
        currentDashTime = dashTime;
        playerCollider.excludeLayers = LayerMask.GetMask("Enemies");
        rb.excludeLayers = LayerMask.GetMask("Enemies");

        switch (direction.y > 0)
        {
            case true:
                animator.Play("mainCharacter_dash_back", -1, 0f);
                break;
            case false:
                animator.Play("mainCharacter_dash", -1, 0f);
                break;
        }

        while (currentDashTime > 0f)
        {
            currentDashTime -= Time.deltaTime; // Lower the dash timer each.

            rb.linearVelocity = direction * dashSpeed; // Dash in the direction that was held down.

            yield return null; // Returns out of the coroutine this frame so we don't hit an infinite loop.
        }
        rb.linearVelocity = new Vector2(0f, 0f); // Stop dashing. 
        
        guitarSpriteRenderer.enabled = true;
        CanDash = true;
        playerCollider.excludeLayers = LayerMask.GetMask("Nothing");
        rb.excludeLayers = LayerMask.GetMask("Nothing");
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
                            animator.Play("mainCharacter_walkCycle_back");
                            guitarSpriteRenderer.sortingOrder = -1;
                            break;
                        case false | false:
                            animator.Play("mainCharacter_idleCycle_back");
                            guitarSpriteRenderer.sortingOrder = -1;
                            break;
                    }
                    break;
                case false: // top
                    switch ((movement.x != 0 | movement.y != 0))
                    {
                        case true | true:
                            animator.Play("mainCharacter_walkCycle");
                            guitarSpriteRenderer.sortingOrder = 1;
                            break;
                        case false | false:
                            animator.Play("mainCharacter_idleCycle");
                            guitarSpriteRenderer.sortingOrder = 1;
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
