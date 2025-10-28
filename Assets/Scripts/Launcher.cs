using UnityEngine;
using Photon.Pun;
using TMPro;
public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject createRoomScreen;
    [SerializeField] private GameObject menuButtons;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_InputField roomNameInput;
    void Start()
    {
        CloseMenus();
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network...";

        PhotonNetwork.ConnectUsingSettings();
    }

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {

        PhotonNetwork.JoinLobby();
        loadingText.text = "Joining Lobby....";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }
    public void OpenRoomCreate()
    {
        CloseMenus();
        createRoomScreen.SetActive(true);
    }
}
