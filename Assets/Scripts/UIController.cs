using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance;
    public TextMeshProUGUI overheatedMessage;

    public GameObject deathScreen;
    public GameObject leaderBoard;
    public GameObject endScreen;
    public GameObject optionsScreen;
    public GameObject sniperScopeOverlay;
    public LeaderboardPlayer leaderboardPlayerDisplay;
    public TMP_Text deathText;
    public TMP_Text ammoText;
    public TMP_Text killsText;
    public TMP_Text deathsText;
    public TMP_Text TimerText;
    public Slider weaponTempSlider;
    public Slider healthSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowHideOptions();
        }

        if (optionsScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }

    public void ShowHideOptions()
    {
        if (!optionsScreen.activeInHierarchy)
        {
            optionsScreen.SetActive(true);

        }
        else
        {
            optionsScreen.SetActive(false);
        }
    }

    public void ReturnToMainmenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
