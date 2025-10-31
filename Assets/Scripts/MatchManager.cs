using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MatchManager : MonoBehaviour
{

    public static MatchManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
