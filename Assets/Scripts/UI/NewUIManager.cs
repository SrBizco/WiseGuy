using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NewUIManager : MonoBehaviour
{
    public static NewUIManager Instance { get; private set; }

    [Header("Wanted")]
    [SerializeField] private TextMeshProUGUI wantedTMP;

    [Header("Player Bars")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Slider staminaBar;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        UpdateWanted();
    }

    private void UpdateWanted()
    {
        if (wantedTMP == null) return;

        bool isWanted = PoliceManager.Instance != null && PoliceManager.Instance.IsPlayerWanted;
        wantedTMP.gameObject.SetActive(isWanted);
    }

    public void SetHealth(float current, float max)
    {
        if (healthBar == null) return;

        float ratio = current / max;
        healthBar.value = ratio;

        UpdateHealthColor(ratio);
    }

    private void UpdateHealthColor(float healthPercent)
    {
        if (healthFillImage == null) return;

        if (healthPercent > 0.66f)
        {
            healthFillImage.color = Color.green;
        }
        else if (healthPercent > 0.33f)
        {
            healthFillImage.color = Color.yellow;
        }
        else
        {
            healthFillImage.color = Color.red;
        }
    }

    public void SetStamina(float current, float max)
    {
        if (staminaBar == null) return;
        staminaBar.value = current / max;
    }
}
