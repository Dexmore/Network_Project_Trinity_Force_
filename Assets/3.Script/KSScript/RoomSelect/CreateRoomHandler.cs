// Assets/Scripts/CreateRoomHandler.cs
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CreateRoomHandler : MonoBehaviour
{
    [Header("Popup & Input")]
    [Tooltip("최초에 비활성화 상태로 둘 팝업 패널")]
    [SerializeField] private GameObject createRoomPopup;

    [Tooltip("방 제목을 입력할 TMP Input Field")]
    [SerializeField] private TMP_InputField roomNameInput;

    void Start()
    {
        // 시작 시 팝업 숨기기
        if (createRoomPopup != null)
            createRoomPopup.SetActive(false);
    }

    /// <summary>
    /// “방 만들기” 버튼 OnClick()에 바인딩
    /// </summary>
    public void ShowCreateRoomPopup()
    {
        if (createRoomPopup == null || roomNameInput == null)
        {
            Debug.LogError("[CreateRoomHandler] 팝업 또는 입력 필드가 할당되지 않았습니다!");
            return;
        }

        // 입력 초기화 & 포커스
        roomNameInput.text = "";
        createRoomPopup.SetActive(true);
        roomNameInput.ActivateInputField();
    }

    /// <summary>
    /// 팝업 취소 버튼 OnClick()에 바인딩
    /// </summary>
    public void HideCreateRoomPopup()
    {
        if (createRoomPopup != null)
            createRoomPopup.SetActive(false);
    }

    /// <summary>
    /// 팝업 확인 버튼 OnClick()에 바인딩  
    /// 방 제목을 저장하고 로비 씬으로 전환
    /// </summary>
    public void ConfirmCreateRoom()
    {
        if (roomNameInput == null)
        {
            Debug.LogError("[CreateRoomHandler] roomNameInput이 할당되지 않았습니다!");
            return;
        }

        string title = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(title))
        {
            Debug.LogWarning("[CreateRoomHandler] 방 제목을 입력해주세요.");
            return;
        }

        // 방 제목 저장
        RoomInfo.CurrentRoomTitle = title;

        // 로비 씬으로 전환 (씬 이름을 실제 이름으로 조정)
        SceneManager.LoadScene("LobbyScene");
    }
      public void CloseUi()
    {
        createRoomPopup.SetActive(false);
    }
}
