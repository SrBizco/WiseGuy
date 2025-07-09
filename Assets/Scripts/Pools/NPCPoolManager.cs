using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class NPCPoolManager : MonoBehaviour
{
    [Header("Prefabs disponibles")]
    public List<GameObject> npcPrefabs;

    [Header("Cantidad total de NPCs")]
    public int poolSize = 30;

    [Header("Spawn Settings")]
    public float spawnRadius = 30f;
    public float despawnDistance = 50f;

    [Header("Detección de cámara")]
    [Tooltip("Layer en la que debe estar la cámara del jugador.")]
    public LayerMask cameraLayerMask;

    private Transform playerCamera;
    private List<GameObject> npcPool = new List<GameObject>();

    void Start()
    {
        if (playerCamera == null)
        {
            Camera[] allCameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in allCameras)
            {
                if (((1 << cam.gameObject.layer) & cameraLayerMask.value) != 0)
                {
                    playerCamera = cam.transform;
                    break;
                }
            }

            if (playerCamera == null)
            {
                Debug.LogError("🛑 No se encontró ninguna cámara con la Layer asignada.");
                return;
            }
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = npcPrefabs[Random.Range(0, npcPrefabs.Count)];
            GameObject npc = Instantiate(prefab);

            var ragdoll = npc.GetComponent<RagdollActivator>();
            ragdoll?.Initialize();

            npc.SetActive(false);
            npcPool.Add(npc);
        }

        InvokeRepeating(nameof(UpdateNPCPositions), 0f, 3f);
    }

    void UpdateNPCPositions()
    {
        // ✅ Validar si player está sobre NavMesh
        if (!IsPlayerOnNavMesh())
        {
            Debug.Log("⚠️ Player fuera del NavMesh. NPCs no se actualizarán hasta volver a zona válida.");
            return;
        }

        foreach (GameObject npc in npcPool)
        {
            if (!npc.activeInHierarchy)
            {
                Vector3 spawnPos = RandomNavSphere(playerCamera.position, spawnRadius);

                if (spawnPos == Vector3.negativeInfinity)
                {
                    Debug.Log("⚠️ No hay NavMesh cerca. NPC no spawneado.");
                    continue;
                }

                npc.transform.position = spawnPos;
                npc.transform.rotation = Quaternion.identity;
                npc.SetActive(true);

                var civilian = npc.GetComponent<CivilianController>();
                if (civilian != null)
                {
                    civilian.enabled = true;
                    civilian.ResetState();
                }
            }
            else
            {
                float dist = Vector3.Distance(npc.transform.position, playerCamera.position);
                if (dist > despawnDistance)
                {
                    npc.SetActive(false);
                }
            }
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDir = Random.insideUnitSphere * dist;
        randDir += origin;

        if (NavMesh.SamplePosition(randDir, out NavMeshHit hit, dist, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return Vector3.negativeInfinity;
        }
    }

    bool IsPlayerOnNavMesh()
    {
        return NavMesh.SamplePosition(playerCamera.position, out NavMeshHit hit, 2f, NavMesh.AllAreas);
    }
}
