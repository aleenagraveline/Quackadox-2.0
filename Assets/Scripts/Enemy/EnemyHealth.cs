using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int health = 2;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>(); // Get Animator on enemy
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (animator != null)
        {
            animator.SetTrigger("EnemyDamaged"); // Trigger the animation
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy Died");

        if (animator != null)
        {
            animator.SetTrigger("EnemyDeath"); // Play death animation
        }

        // Start coroutine to wait for animation to finish
        StartCoroutine(DeathAfterAnimation());
    }

    IEnumerator DeathAfterAnimation()
    {
        // Optional: Adjust this to match your animation's length
        yield return new WaitForSeconds(1.0f); // Wait for death animation to finish
        Destroy(gameObject);
    }
}