using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuitarController : MonoBehaviour
{
    [Header("Bullet Type")]
    public GameObject bulletType;
    Transform playerSpriteTransform;
    Transform crosshairTransform;
    Transform firePoint;
    Animator guitarAnimator;

    Vector2 playerScreenPosition;
    Vector2 mousePosition;

    [Header("Guitar Stats")]
    [SerializeField] private int bulletDamage;
    [SerializeField] private float bulletForce;
    [SerializeField] private float bulletScale;
    [SerializeField] private float missCooldown;

    GameObject spriteController;
    GameObject guitarController;
    GameObject noteManagerObject;
    GameObject crosshairController;
    GameObject firePointObject;

    PlayerController playerController;
    NoteManager noteManager;


    bool cooldown;

    void Start()
    {
        spriteController = GameObject.Find("SpriteController");
        guitarController = GameObject.Find("GuitarController");
        crosshairController = GameObject.Find("CrosshairController");
        noteManagerObject = GameObject.Find("NoteManager");
        firePointObject = GameObject.Find("firingPoint");

        playerController = GetComponent<PlayerController>();
        noteManager = noteManagerObject.GetComponent<NoteManager>();
        playerSpriteTransform = spriteController.GetComponent<Transform>();
        crosshairTransform = crosshairController.GetComponent<Transform>();
        firePoint = firePointObject.GetComponent<Transform>();
        guitarAnimator = guitarController.GetComponent<Animator>();
        
    }

    void Update()
    {
        if (Time.timeScale == 0f)
            return;

        playerScreenPosition = Camera.main.WorldToScreenPoint(playerSpriteTransform.transform.position);

        mousePosition = Input.mousePosition;
        PlayerGuitar();
        PlayerCrosshair();
    }

    private void PlayerGuitar()
    {
        Vector2 guitarScreenPosition = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 dir = mousePosition - guitarScreenPosition;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        guitarController.transform.rotation = Quaternion.Euler(0, 0, angle);
        firePoint.rotation = Quaternion.Euler(0, 0, angle - 90);

        if (playerScreenPosition.x > mousePosition.x) // LEFT
        {
            guitarController.transform.localScale = new Vector2(1f, -1);
            guitarController.transform.localPosition = new Vector2(-Mathf.Abs(guitarController.transform.localPosition.x), guitarController.transform.localPosition.y);
        }
        else //RIGHT
        {
            guitarController.transform.localScale = new Vector2(1f, 1);
            guitarController.transform.localPosition = new Vector2(Mathf.Abs(guitarController.transform.localPosition.x), guitarController.transform.localPosition.y);
        }

        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && playerController.CanDash && !cooldown)
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
        GameObject bullet = Instantiate(bulletType, firePoint.position, guitarController.transform.rotation);
        bullet.GetComponent<Bullet>().bulletDamage = bulletDamage; 
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.up * bulletForce, ForceMode2D.Impulse);
        guitarAnimator.Play("mainCharacter_guitarShoot",-1, 0f);

        if (playerScreenPosition.x > mousePosition.x) { bullet.transform.localScale = new Vector2(-bulletScale, -bulletScale); }
        else {bullet.transform.localScale = new Vector2(bulletScale, bulletScale); }
    }
}
