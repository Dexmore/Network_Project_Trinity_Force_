using Mirror;
using UnityEngine;

public class SimpleNetworkManager : NetworkManager
{
    public override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject); // 씬 전환 시 유지
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        // 서버에서 AuthMessage 수신 처리 등록
        NetworkServer.RegisterHandler<AuthMessage>(OnAuthMessageReceived);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        Debug.Log("클라이언트: 서버에 연결됨");

        // AuthUI에서 입력한 사용자 정보
        if (AuthUI.Instance != null)
        {
            AuthMessage msg = new AuthMessage
            {
                username = AuthUI.Instance.Username,
                password = AuthUI.Instance.Password
            };

            // 메시지 전송
            NetworkClient.Send(msg);
            Debug.Log($"클라이언트 로그인 요청: {msg.username}, {msg.password}");
        }
        else
        {
            Debug.LogError("AuthUI 인스턴스를 찾을 수 없습니다.");
        }
    }

    // 서버에서 인증 메시지를 수신하고 플레이어를 생성
    private void OnAuthMessageReceived(NetworkConnectionToClient conn, AuthMessage msg)
    {
        Debug.Log($"서버: 로그인 메시지 수신 - {msg.username}, {msg.password}");

        // 플레이어 생성 및 연결
        GameObject player = Instantiate(playerPrefab);
        player.GetComponent<SimpleNetworkPlayer>().playerName = msg.username;

        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
