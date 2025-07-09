using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        NewUIManager.Instance.SetHealth(currentHealth, maxHealth);
        AudioManager.Instance.PlayDamage();
        Debug.Log($"⚠️ Player recibió daño: {amount}. Vida actual: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("💀 Player murió");
        // Podés desactivar controles, mostrar UI de muerte, etc.
    }
}
