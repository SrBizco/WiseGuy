using UnityEngine;
using System.Collections;

public class PoliceManager : MonoBehaviour
{
    public static PoliceManager Instance { get; private set; }

    [Header("Estado de búsqueda")]
    [SerializeField] private bool isPlayerWanted = false;
    [SerializeField] private float wantedDuration = 15f;

    public bool IsPlayerWanted => isPlayerWanted;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetPlayerWanted(bool value)
    {
        isPlayerWanted = value;

        if (value)
        {
            StopAllCoroutines();
            StartCoroutine(ClearWantedAfterTime());
        }
    }

    private IEnumerator ClearWantedAfterTime()
    {
        yield return new WaitForSeconds(wantedDuration);
        isPlayerWanted = false;
    }
}

