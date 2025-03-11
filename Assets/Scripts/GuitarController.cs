using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuitarController : MonoBehaviour
{
    [Header("Bullet GameObject")]
    public GameObject bulletType;

    [Header("Transform Components")]
    public Transform playerSpriteTransform;
    public Transform crosshairTransform;
    public Transform firePoint;

    [Header("Animator")]
    public Animator guitarAnimator;

    [Header("Sprite Renderers")]
    public SpriteRenderer guitarSpriteRenderer;

    [Header("Controllers")]
    public PlayerController playerController;

    [Header("NoteManager")]
    public NoteManager noteManager;

    Vector2 playerScreenPosition;
    Vector2 mousePosition;

    [Header("Guitar Controller Variables")]
    public float bulletForce;
    public float bulletScale;
    public float missCooldown;

    bool cooldown;
    int mouseInput; //0: left click 1: right click

    

    void Update()
    {
        playerScreenPosition = Camera.main.WorldToScreenPoint(playerSpriteTransform.transform.position);

        mousePosition = Input.mousePosition;
        PlayerGuitar();
        PlayerCrosshair();
    }

    private void PlayGuitarIdle()
    {
        guitarAnimator.Play("mainCharacter_guitarIdle");
    }

    private void PlayerGuitar()
    {
        Vector2 guitarScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 dir = mousePosition - guitarScreenPosition;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        firePoint.rotation = Quaternion.Euler(0, 0, angle - 90);

        if (playerScreenPosition.x > mousePosition.x) // LEFT
        {
            transform.localScale = new Vector2(1f, -1);
            transform.localPosition = new Vector2(-Mathf.Abs(transform.localPosition.x), transform.localPosition.y);
        }
        else //RIGHT
        {
            transform.localScale = new Vector2(1f, 1);
            transform.localPosition = new Vector2(Mathf.Abs(transform.localPosition.x), transform.localPosition.y);
        }

        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && playerController.canDash && !cooldown)
        {
            noteManager.StartSong();
        }


    }
    public bool GetCooldown()
    {
        return cooldown;
    }
    private void PlayerCrosshair()
    {
        Vector2 mouseCursorPos = Camera.main.ScreenToWorldPoint(mousePosition);
        crosshairTransform.position = mouseCursorPos;
    }

    public IEnumerator MissCooldown()
    {
        cooldown = true;
        yield return new WaitForSeconds(missCooldown);
        cooldown = false;
    }
    public void Shoot()
    {
        GameObject bullet = Instantiate(bulletType, firePoint.position, transform.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);
        guitarAnimator.Play("mainCharacter_guitarShoot",-1, 0f);

        if (playerScreenPosition.x > mousePosition.x) { bullet.transform.localScale = new Vector2(-bulletScale, -bulletScale); }
        else {bullet.transform.localScale = new Vector2(bulletScale, bulletScale); }
    }
}
