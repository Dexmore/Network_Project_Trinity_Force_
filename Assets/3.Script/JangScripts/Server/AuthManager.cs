using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    private Dictionary<string, string> userDB = new();

    public void Authenticate(NetworkConnectionToClient conn, AuthMessage msg)
    {
        Debug.Log($"서버: 로그인 요청 - {msg.username}, {msg.password}");

        if (!userDB.ContainsKey(msg.username))
        {
            userDB[msg.username] = msg.password;
            Debug.Log("신규 회원 등록 완료");
        }

        if (userDB[msg.username] == msg.password)
        {
            Debug.Log("로그인 성공");
            conn.authenticationData = msg.username;
            conn.isAuthenticated = true;
        }
        else
        {
            Debug.LogError("비밀번호 불일치, 연결 거절");
            conn.Disconnect();
        }
    }
}
