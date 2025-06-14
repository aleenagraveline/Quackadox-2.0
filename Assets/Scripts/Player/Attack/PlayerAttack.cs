using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject attackPrefab; // Prefab to instantiate for attack
    public float attackSpeed = 10f; // Speed of attack movement
    public float attackLifetime = 1f; // How long the attack stays before disappearing

    private int attackIndex = 0; // Keep track of how many attacks we've shot

    private PlayerMovement playerMovement;

    public Transform attackSpawnPoint;

    [SerializeField] AudioManager sounds;
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Long Range Attack");

            if (playerMovement != null)
            {
                playerMovement.PlayDuckQuackAnimation(); // Set bool true
                Debug.Log("AnimationQuack");

                // Stop animation after a delay (match the animation duration)
                StartCoroutine(StopQuackAfterDelay(0.5f)); // Adjust 0.5f to match your animation length
            }

            PerformLongRangeAttack();
        }
    }

    IEnumerator StopQuackAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (playerMovement != null)
        {
            playerMovement.StopDuckQuackAnimation(); // Set bool false
        }
    }

    void PerformLongRangeAttack()
    {
        attackIndex = 0;
        ShootAttackWithDelay();
    }

    void ShootAttackWithDelay()
    {
        if (attackIndex < 1)
        {
            GameObject attack = Instantiate(attackPrefab, attackSpawnPoint.position, Quaternion.identity);
            sounds.Play("DuckAttack");

            // Get direction based on SpriteRenderer flip
            bool facingRight = !playerMovement.GetComponent<SpriteRenderer>().flipX;
            Vector3 direction = facingRight ? Vector3.right : Vector3.left;

            // Add slight spacing for visual spread effect
            direction += new Vector3(attackIndex * 0.2f * (facingRight ? 1 : -1), 0, 0);

            Rigidbody2D rb = attack.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = direction.normalized * attackSpeed;
            }

            // Start the delay before destroying the attack
            StartCoroutine(AttackLifetime(attack, attackLifetime));

            attackIndex++;

            Invoke(nameof(ShootAttackWithDelay), 0.2f);
        }
    }

    // Coroutine to handle attack lifetime after hitting enemy
    IEnumerator AttackLifetime(GameObject attack, float delay)
    {
        // Wait for the specified attack lifetime before destroying
        yield return new WaitForSeconds(delay);

        // Destroy the attack object after the delay
        Destroy(attack);
    }
}

