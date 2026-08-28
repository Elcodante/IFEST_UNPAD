using UnityEngine;
using UnityEngine.UI;

public class PlayerResourceManager : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float startingHealth = 100f;
    [SerializeField] private Slider healthSlider;

    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float startingHunger = 100f;
    [SerializeField] private Slider hungerSlider;

    public float CurrentHealth { get; private set; }
    public float CurrentHunger { get; private set; }

    private bool gameOver;

    private void Awake()
    {
        ResetResources();
    }

    public void RemoveHealth(float amount)
    {
        if (gameOver)
            return;

        CurrentHealth -= amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0f, maxHealth);

        UpdateUI();

        if (CurrentHealth <= 0f)
        {
            TriggerGameOver();
        }
    }

    public void RemoveHunger(float amount)
    {
        if (gameOver)
            return;

        CurrentHunger -= amount;
        CurrentHunger = Mathf.Clamp(CurrentHunger, 0f, maxHunger);

        UpdateUI();

        // Hunger 0 TIDAK langsung Game Over.
        // Health akan mulai berkurang.
        if (CurrentHunger <= 0f)
        {
            RemoveHealth(1f);
        }
    }

    public void ResetResources()
    {
        gameOver = false;

        CurrentHealth = Mathf.Clamp(
            startingHealth,
            0f,
            maxHealth
        );

        CurrentHunger = Mathf.Clamp(
            startingHunger,
            0f,
            maxHunger
        );

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = CurrentHealth;
        }

        if (hungerSlider != null)
        {
            hungerSlider.maxValue = maxHunger;
            hungerSlider.value = CurrentHunger;
        }
    }

    private void TriggerGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        GameManager.Instance?.TriggerGameOver();
    }
}