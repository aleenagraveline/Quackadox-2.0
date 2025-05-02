using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDamage : MonoBehaviour
{
    public int damage = 1; // Amount of damage this attack does
    public float destroyDelay = 0.2f; // Delay before destroying the attack after hitting an enemy

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if we collided with an enemy
        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

        // If the object has an EnemyHealth component, damage it
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);

            // Start a coroutine to delay destruction
            StartCoroutine(DelayDestroy());
        }
    }

    IEnumerator DelayDestroy()
    {
        // Wait for the specified destroy delay
        yield return new WaitForSeconds(destroyDelay);

        // Destroy the attack object
        Destroy(gameObject);
    }
}
