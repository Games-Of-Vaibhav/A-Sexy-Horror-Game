using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;
    public UnityEvent onDeath;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Trigger the death event
        onDeath?.Invoke();
        // Additional death logic (disable player, play death animation, etc.)
        Debug.Log("Player died!");
    }
}
