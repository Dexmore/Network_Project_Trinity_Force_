using UnityEngine;
using TMPro;

public class UserSlotController : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statusText;

    public void SetUserInfo(string nickname, bool isReady)
    {
        nameText.text = nickname;
        statusText.text = isReady ? "준비 완료" : "준비 중";
    }

    public void SetReady(bool isReady)
    {
        statusText.text = isReady ? "준비 완료" : "준비 중";
    }
}
