using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;


public class MatchManager : MonoBehaviourPunCallbacks, IOnEventCallback
{

    public static MatchManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public enum EventCodes : byte
    {
        NewPlayer,
        ListPlayers,
        UpdateStat
    }

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();
    private int index;

    private List<LeaderboardPlayer> lboardPlayers = new List<LeaderboardPlayer>();
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowLeaderBoard();
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            UIController.Instance.leaderBoard.SetActive(false);
        }
    }


    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code < 200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerReceive(data);
                    break;
                case EventCodes.ListPlayers:
                    ListPlayersReceive(data);
                    break;
                case EventCodes.UpdateStat:
                    UpdateStatsReceived(data);
                    break;

            }

        }
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }
    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true }


        );
    }

    public void NewPlayerReceive(object[] dataRecieved)
    {
        PlayerInfo player = new PlayerInfo((string)dataRecieved[0], (int)dataRecieved[1], (int)dataRecieved[2], (int)dataRecieved[3]);

        allPlayers.Add(player);
        ListPlayersSend();
    }
    public void ListPlayersSend()
    {
        object[] package = new object[allPlayers.Count];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i] = piece;
        }

        PhotonNetwork.RaiseEvent(
    (byte)EventCodes.ListPlayers,
    package,
    new RaiseEventOptions { Receivers = ReceiverGroup.All },
    new SendOptions { Reliability = true }


);
    }

    public void ListPlayersReceive(object[] dataRecieved)
    {
        allPlayers.Clear();

        for (int i = 0; i < dataRecieved.Length; i++)
        {

            object[] piece = (object[])dataRecieved[i];
            PlayerInfo player = new PlayerInfo(
                    (string)piece[0],
                    (int)piece[1],
                    (int)piece[2],
                    (int)piece[3]
            );

            allPlayers.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                index = i;
            }
        }
    }
    public void UpdateStatsSend(int actorSending, int statToUpdate, int amountToChange)
    {
        object[] package = new object[] { actorSending, statToUpdate, amountToChange };
        PhotonNetwork.RaiseEvent(
    (byte)EventCodes.UpdateStat,
    package,
    new RaiseEventOptions { Receivers = ReceiverGroup.All },
    new SendOptions { Reliability = true }


);
    }

    public void UpdateStatsReceived(object[] dataRecieved)
    {
        int actor = (int)dataRecieved[0];
        int statType = (int)dataRecieved[1];
        int amount = (int)dataRecieved[2];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                switch (statType)
                {
                    case 0:
                        allPlayers[i].kills += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : kills " + allPlayers[i].kills);
                        break;
                    case 1:
                        allPlayers[i].deaths += amount;
                        Debug.Log("Player " + allPlayers[i].name + " : deaths " + allPlayers[i].deaths);
                        break;

                }
                if (i == index)
                {
                    UpdateStatsDisplay();
                }

                break;

            }
        }
    }

    public void UpdateStatsDisplay()
    {

        if (allPlayers.Count > index)
        {
            UIController.Instance.killsText.text = "Kills: " + allPlayers[index].kills;
            UIController.Instance.deathsText.text = "Deaths: " + allPlayers[index].deaths;
        }
        else
        {
            UIController.Instance.killsText.text = "Kills: 0";
            UIController.Instance.deathsText.text = "Deaths: 0";
        }

    }

    void ShowLeaderBoard()
    {
        UIController.Instance.leaderBoard.SetActive(true);

        foreach (LeaderboardPlayer lp in lboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lboardPlayers.Clear();

        UIController.Instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        foreach (PlayerInfo player in allPlayers)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.Instance.leaderboardPlayerDisplay, UIController.Instance.leaderboardPlayerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);

            newPlayerDisplay.gameObject.SetActive(true);

            lboardPlayers.Add(newPlayerDisplay);
        }
    }

}

[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;

    public PlayerInfo(string _name, int _actor, int _kills, int _deaths)
    {
        name = _name;
        actor = _actor;
        kills = _kills;
        deaths = _deaths;

    }
}
