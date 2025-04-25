using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;
    public Rigidbody2D playerRB;
    [SerializeField] private bool shouldPushPlayer;
    [SerializeField] private float knockbackPower;
    [SerializeField] private bool isPitfall;

    //brian's addition for UI testing
    public PlayerHealth Phealth;

    private bool playerDamaged;
    [SerializeField] private float damageCooldown;
    private float damageCountdown;

    [SerializeField] private AudioManager sounds;
    [SerializeField] private CheckpointManager checkpoints;

    void Start()
    {
        playerRB = player.GetComponent<PlayerMovement>().GetRB();
        playerDamaged = false;
        damageCountdown = 0;
        if (shouldPushPlayer && knockbackPower == 0)
        {
            knockbackPower = 20;
        }
    }

    void Update()
    {
        if (damageCountdown > 0)
        {
            damageCountdown -= Time.deltaTime;
        }

        if (damageCountdown <= 0)
        {
            playerDamaged = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("DAMAGE COLLISION: " + collider.gameObject);
        if (collider.gameObject == player && !playerDamaged)
        {
            Debug.Log("OW!");
            sounds.Play("Damage");

            if (shouldPushPlayer)
            {
                player.GetComponent<PlayerMovement>().SetPushed(true);
                float movementDirection = player.GetComponent<PlayerMovement>().GetHorizontal();
                if (movementDirection < 0)
                {
                    playerRB.velocity = new Vector3(knockbackPower, 0f, 0f);
                    Debug.Log("Movement is negative");
                    Debug.Log(playerRB.velocity);
                    StartCoroutine(ChangeExternalVelocityRight());
                }
                else
                {
                    playerRB.velocity = new Vector3(-knockbackPower, 0f, 0f);
                    Debug.Log(playerRB.velocity);
                    Debug.Log("Movement is positive or zero");
                    StartCoroutine(ChangeExternalVelocityLeft());
                }
            }
            else if (isPitfall && Phealth.health > 0)
            {
                checkpoints.RespawnPlayerAtCheckpoint();
            }

            Phealth.Damaged();
            Debug.Log("Take damage");
            playerDamaged = true;
            damageCountdown = damageCooldown;
        }
    }

    public IEnumerator ChangeExternalVelocityRight()
    {
        while (playerRB.velocity.x > 0)
        {
            playerRB.velocity = new Vector3(playerRB.velocity.x - knockbackPower / 10, 0f, 0f);
            Debug.Log("Zeroing velocity right");
            Debug.Log(playerRB.velocity);
            yield return null;
        }

        player.GetComponent<PlayerMovement>().SetPushed(false);

        yield return new WaitForSeconds(1.0f);
    }

    public IEnumerator ChangeExternalVelocityLeft()
    {
        while (playerRB.velocity.x < 0)
        {
            playerRB.velocity = new Vector3(playerRB.velocity.x + knockbackPower / 10, 0f, 0f);
            Debug.Log("Zeroing velocity left");
            Debug.Log(playerRB.velocity);
            yield return null;
        }

        player.GetComponent<PlayerMovement>().SetPushed(false);

        yield return new WaitForSeconds(1.0f);
    }

}

