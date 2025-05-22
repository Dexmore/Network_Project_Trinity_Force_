// Assets/Scripts/LobbyRoomDisplay.cs
using UnityEngine;
using TMPro;

public class LobbyRoomDisplay : MonoBehaviour
{
    [Header("TMP Text Component")]
    public TextMeshProUGUI roomTitleText;

    void Start()
    {
        if (roomTitleText != null)
            roomTitleText.text = RoomInfoName.CurrentRoomTitle;
    }
}
