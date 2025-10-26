using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    public Transform[] spawnPoints;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (Transform spawn in spawnPoints)
        {
            spawn.gameObject.SetActive(false);

        }
    }

    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
