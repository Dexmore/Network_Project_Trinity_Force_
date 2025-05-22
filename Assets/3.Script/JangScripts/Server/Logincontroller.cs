using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Logincontroller : MonoBehaviour
{
    public InputField id_i;
    public InputField Password_i;

    [SerializeField] private Text log;

    public void LoginBtn()
    {
        if(id_i.text.Equals(string.Empty) || Password_i.text.Equals(string.Empty))
        {
            log.text = "아이디와 비밀번호를 입력하세요";
            return;
        }

        if(SQLManager1.instance.Login(id_i.text, Password_i.text))
        {
            // 로그인이 성공
            User_info1 info = SQLManager1.instance.info;
            Debug.Log(info.User_UID + " | " + info.User_Birthday);
            gameObject.SetActive(false);
        }
        else
        {
            log.text = "아이디와 비밀번호를 확인해 주세용~";
        }
    }
}
