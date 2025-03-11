using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Components")]
    public Rigidbody2D rb;
    public Animator animator;
    public Collider2D playerCollider; //Prevents player from going through walls when dashing, not used to test if enemies have attacked the player
    public Transform playerSpriteTransform;
    public SpriteRenderer guitarSpriteRenderer;

    [HideInInspector] public bool canDash;

    [Header("PlayerController Variables")]
    [SerializeField] float moveSpeed;
    [SerializeField] float dashTime;
    [SerializeField] float dashSpeed;

    private float currentDashTime;

    Vector2 movement;
    Vector2 playerScreenPosition;
    Vector2 mousePosition;

    private void Start()
    {
        canDash = true;
    }

    void Update()
    {
        playerScreenPosition = Camera.main.WorldToScreenPoint(playerSpriteTransform.transform.position);
        mousePosition = Input.mousePosition;

        PlayerMovement();
        PlayerDash();
    }
    private void PlayerDash()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (canDash && (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.Space)) && (movement.x != 0 || movement.y != 0))
        {
            StartCoroutine(Dash(new Vector2(movement.x, movement.y).normalized));
        }
    }

    IEnumerator Dash(Vector2 direction)
    {
        guitarSpriteRenderer.enabled = false;
        canDash = false;
        playerCollider.enabled = false;
        currentDashTime = dashTime;

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

            rb.velocity = direction * dashSpeed; // Dash in the direction that was held down.

            yield return null; // Returns out of the coroutine this frame so we don't hit an infinite loop.
        }

        rb.velocity = new Vector2(0f, 0f); // Stop dashing. 
        canDash = true;
        playerCollider.enabled = true;
        guitarSpriteRenderer.enabled = true;
    }

    private void PlayerMovement()
    {
        //Get Player Input
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        //Move Rigidbody
        if (canDash)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
            switch (playerScreenPosition.y < mousePosition.y)
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
