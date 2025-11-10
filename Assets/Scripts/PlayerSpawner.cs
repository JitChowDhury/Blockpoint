using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{

    public static PlayerSpawner Instance;
    public GameObject playerPrefab;
    public GameObject deathEffect;
    private GameObject player;

    [SerializeField] private float respawnTime = 5f;
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

    public void Die(string damager)
    {

        UIController.Instance.deathText.text = "You were killed by " + damager;



        if (player != null)
        {
            StartCoroutine(DieCo());
        }

    }

    public IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(deathEffect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UIController.Instance.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnTime);
        UIController.Instance.deathScreen.SetActive(false);
        SpawnPlayer();

    }
}
