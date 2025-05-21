// Assets/Scripts/EnterRoomHandler.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterRoomHandler : MonoBehaviour
{
    [Tooltip("입장 시 로드할 씬 이름")]
    [SerializeField] private string sceneName = "GameScene";

    /// <summary>
    /// 버튼 OnClick()에 바인딩.
    /// 눌렀을 때 씬 전환을 합니다.
    /// </summary>
    public void EnterRoom()
    {
        SceneManager.LoadScene(sceneName);
    }
}
