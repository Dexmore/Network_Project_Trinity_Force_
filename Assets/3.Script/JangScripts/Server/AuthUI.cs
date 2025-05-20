using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class AuthUI : MonoBehaviour
{
    public static AuthUI Instance;

    public InputField usernameInput;
    public InputField passwordInput;
    public Button loginButton;

    [HideInInspector] public string Username;
    [HideInInspector] public string Password;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginClicked);
    }

    void OnLoginClicked()
    {
        Username = usernameInput.text.Trim();
        Password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
        {
            Debug.LogWarning("⚠️ 아이디와 비밀번호를 모두 입력하세요.");
            return;
        }

        if (!NetworkClient.isConnected && !NetworkClient.active)
        {
            NetworkManager.singleton.StartClient(); // 연결 시도
        }
    }
}
