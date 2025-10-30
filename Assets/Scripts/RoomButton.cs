using TMPro;
using UnityEngine;
using Photon.Realtime;
public class RoomButton : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;

    private RoomInfo roomInfo;

    public void SetButtonDetails(RoomInfo inputInfo)
    {
        roomInfo = inputInfo;
        buttonText.text = roomInfo.Name;
    }
    public void OpenRoom()
    {
        Launcher.Instance.joinRoom(roomInfo);
    }

}
