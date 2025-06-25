using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuitarController : MonoBehaviour
{
    [Header("Bullet Type")]
    public GameObject bulletType;

    Vector2 playerScreenPosition;
    Vector2 mousePosition;

    [Header("Guitar Stats")]
    [SerializeField] private int bulletDamage;
    [SerializeField] private float bulletForce;
    [SerializeField] private float bulletScale;
    [SerializeField] private float missCooldown;

    [Header("Components")]
    [SerializeField] private PlayerSpriteHandler spriteHandler;
    [SerializeField] private Transform playerSpriteTransform;
    [SerializeField] private Transform firePoint;
    [SerializeField] private BeatManager beatManager;

    PlayerController playerController;


    [Header("Player Cooldown")]
    public PlayerCooldown playerCooldown;


    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        playerScreenPosition = Camera.main.WorldToScreenPoint(playerSpriteTransform.transform.position);

        mousePosition = Input.mousePosition;
        PlayerGuitar();
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
    }
    public void Shoot()
    {
        //noteManager.noteCombo++;
        spriteHandler.guitar = PlayerSpriteHandler.Guitar.SHOOTING;
        spriteHandler.GuitarShootAnimation();

        GameObject bullet = Instantiate(bulletType, firePoint.position, transform.rotation);
        bullet.GetComponent<Bullet>().bulletDamage = bulletDamage;
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);
        if (playerScreenPosition.x > mousePosition.x) { bullet.transform.localScale = new Vector2(-bulletScale, -bulletScale); }
        else { bullet.transform.localScale = new Vector2(bulletScale, bulletScale); }
        
    }
    public void EndShoot() //End of Animation Event in Guitar Shoot
    {
        spriteHandler.guitar = PlayerSpriteHandler.Guitar.IDLE;
        spriteHandler.GuitarShootAnimation();
    }
}

    