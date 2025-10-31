using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{

    public static PlayerSpawner Instance;
    public GameObject playerPrefab;
    private GameObject player;

    private void Awake()
    {
        Instance = this;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.Instance.GetSpawnPoint();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

    }
}
