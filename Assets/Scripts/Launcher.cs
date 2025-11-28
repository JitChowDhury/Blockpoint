using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject createRoomScreen;
    [SerializeField] private GameObject nameInputScreen;
    [SerializeField] private GameObject roomScreen;
    [SerializeField] private GameObject errorScreen;
    [SerializeField] private GameObject roomBrowserScreen;
    [SerializeField] private GameObject menuButtons;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject roomTestButton;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private TMP_Text roomNameText, playerNameLabel;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private RoomButton theRoomButton;
    [SerializeField] private string levelToPlay;
    [SerializeField] private Animator titleAnimator;
    private List<RoomButton> allRoomButtons = new List<RoomButton>();
    private List<TMP_Text> allPlayerNames = new List<TMP_Text>();

    public static bool hasSetNickName;

    public string[] allMaps;
    public bool changeMapBetweenRounds = true;


    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        CloseMenus();
        loadingScreen.SetActive(true);
        loadingText.text = "Connecting To Network...";
        if (!hasSetNickName)
        {
            // user has never entered name → do NOT play title animation yet
        }
        else
        {
            // returning from match or restart → replay animation
            PlayTitleAnimation();
        }

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

#if UNITY_EDITOR
        roomTestButton.SetActive(true);
#endif
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void CloseMenus()
    {
        loadingScreen.SetActive(false);
        menuButtons.SetActive(false);
        createRoomScreen.SetActive(false);
        roomScreen.SetActive(false);
        errorScreen.SetActive(false);
        roomBrowserScreen.SetActive(false);
        nameInputScreen.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {

        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
        loadingText.text = "Joining Lobby....";
    }

    public override void OnJoinedLobby()
    {
        CloseMenus();
        menuButtons.SetActive(true);

        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();
        if (!hasSetNickName)
        {
            CloseMenus();
            nameInputScreen.SetActive(true);
            if (PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }

        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }
    public void OpenRoomCreate()
    {
        CloseMenus();
        createRoomScreen.SetActive(true);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 8;

            PhotonNetwork.CreateRoom(roomNameInput.text, options);

            CloseMenus();
            loadingText.text = "Creating Room....";
            loadingScreen.SetActive(true);
        }
    }

    public void CloseCreateRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnJoinedRoom()
    {

        CloseMenus();
        roomScreen.SetActive(true);

        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        ListAllPlayers();
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }

    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text player in allPlayerNames)
        {
            Destroy(player.gameObject);
        }
        allPlayerNames.Clear();
        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
            newPlayerLabel.text = players[i].NickName;
            newPlayerLabel.gameObject.SetActive(true);
            allPlayerNames.Add(newPlayerLabel);
        }

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newPlayerLabel = Instantiate(playerNameLabel, playerNameLabel.transform.parent);
        newPlayerLabel.text = newPlayer.NickName;
        newPlayerLabel.gameObject.SetActive(true);
        allPlayerNames.Add(newPlayerLabel);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        ListAllPlayers();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Failed To Create  Room: " + message;
        CloseMenus();
        errorScreen.SetActive(true);

    }

    public void CloseErrorScreen()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        CloseMenus();
        loadingText.text = "Leaving Room..";
        loadingScreen.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        CloseMenus();
        menuButtons.SetActive(true);

    }

    public void OpenRoomBrowser()
    {
        CloseMenus();
        roomBrowserScreen.SetActive(true);
    }

    public void CloseRoomBrowser()
    {
        CloseMenus();
        menuButtons.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton rb in allRoomButtons)
            Destroy(rb.gameObject);

        allRoomButtons.Clear();
        theRoomButton.gameObject.SetActive(false);

        foreach (RoomInfo info in roomList)
        {
            if (!info.RemovedFromList &&
                info.IsOpen == true &&
                info.PlayerCount < info.MaxPlayers)
            {
                RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
                newButton.SetButtonDetails(info);
                newButton.gameObject.SetActive(true);
                allRoomButtons.Add(newButton);
            }
        }
    }
    public void JoinRoom(RoomInfo inputInfo)
    {
        PhotonNetwork.JoinRoom(inputInfo.Name);
        CloseMenus();
        loadingText.text = "Joining Room...";
        loadingScreen.SetActive(true);
    }
    public void SetNickName()
    {
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;
            PlayerPrefs.SetString("playerName", nameInput.text);

            CloseMenus();
            menuButtons.SetActive(true);
            PlayTitleAnimation();
            hasSetNickName = true;
        }
    }
    private void PlayTitleAnimation()
    {

        titleAnimator.SetTrigger("Play");
        titleAnimator.SetTrigger("Button");



        // titleAnimator.Play("TextAnim");
        // titleAnimator.Play("ButtonAnim");
    }


    public void StartGame()
    {
        // PhotonNetwork.LoadLevel(levelToPlay);

        PhotonNetwork.LoadLevel(allMaps[Random.Range(0, allMaps.Length)]);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void QuickJoin()
    {
        PhotonNetwork.CreateRoom("Test");
        CloseMenus();
        loadingText.text = "Creating Room";
        loadingScreen.SetActive(true);

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 8;
    }
}
