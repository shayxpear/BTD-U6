using UnityEngine;

public class PlayerSpriteHandler : MonoBehaviour
{
    [Header("Player Animators")]
    public Animator bodyAnimator;
    public Animator headAnimator;
    public Animator guitarAnimator;

    [Header("Player Sprites")]
    public SpriteRenderer bodySprite;
    public SpriteRenderer headSprite;
    public SpriteRenderer guitarSprite;

    [Header("Current State")]
    public PlayerBody playerBody;
    public PlayerHead playerHead;
    public Guitar guitar;

    public enum PlayerBody { IDLE_F, IDLE_B, RUNNING_F, RUNNING_B, DODGING_F, DODGING_B };
    public enum PlayerHead {  IDLE_F, IDLE_B, RUNNING_F, RUNNING_B, DODGING_F, DODGING_B, SHOOTING};
    public enum Guitar { IDLE, SHOOTING, COOLDOWN };

    private void Start()
    {
    }
    void Update()
    {
        PlayerBodyAnimations();
        PlayerHeadAnimations();
    }

    public void PlayerBodyAnimations()
    {
        switch (playerBody)
        {
            case PlayerBody.IDLE_F:
                bodyAnimator.SetTrigger("IDLE_F");
                guitarSprite.enabled = true;
                bodySprite.sortingOrder = -1;
                break;
            case PlayerBody.IDLE_B:
                bodyAnimator.SetTrigger("IDLE_B");
                guitarSprite.enabled = true;
                bodySprite.sortingOrder = 1;
                break;
            case PlayerBody.RUNNING_F:
                bodyAnimator.SetTrigger("RUNNING_F");
                guitarSprite.enabled = true;
                bodySprite.sortingOrder = -1;
                break;
            case PlayerBody.RUNNING_B:
                bodyAnimator.SetTrigger("RUNNING_B");
                guitarSprite.enabled = true;
                bodySprite.sortingOrder = 1;
                break;
            case PlayerBody.DODGING_F:
                bodyAnimator.SetTrigger("DODGING_F");
                guitarSprite.enabled = false;
                bodySprite.sortingOrder = -1;
                break;
            case PlayerBody.DODGING_B:
                bodyAnimator.SetTrigger("DODGING_B");
                bodySprite.sortingOrder = 1;
                guitarSprite.enabled = false;
                break;

        }
    }

    public void PlayerHeadAnimations()
    {

    }

    public void GuitarShootAnimation()
    {
        switch (guitar)
        {
            case Guitar.IDLE:
                guitarAnimator.ResetTrigger("SHOOTING");
                guitarAnimator.Play("IDLE", -1, 0f);
                break;
            case Guitar.SHOOTING:
                guitarAnimator.ResetTrigger("SHOOTING");
                guitarAnimator.SetTrigger("SHOOTING");
                guitarAnimator.Play("SHOOTING", -1, 0f);
                break;
        }
    }
}
