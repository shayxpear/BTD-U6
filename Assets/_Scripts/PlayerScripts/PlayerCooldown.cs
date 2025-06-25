using UnityEngine;

public class PlayerCooldown : MonoBehaviour
{
    public bool cooldown;
    public GameObject playerCooldown;
    public Animator playerCooldownAnim;
    public bool GetCooldown()
    {
        return cooldown;
    }

    public void StartCooldown()
    {
        playerCooldown.SetActive(true);
        cooldown = true;
        playerCooldownAnim.Play("cooldown");
    }

    public void EndCooldown()
    {
        cooldown = false;
        playerCooldown.SetActive(false);
    }
}
