using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [Header("로그인 후 회원정보 수정, 탈퇴 창")]
    [SerializeField] private LoginingController logining;

    [Header("로그인 : 아이디 입력해서 게임 입장")]
    public InputField id_i;
    public InputField pwd_i;
    public Text log;  // 로그인 경고 안내 문구

    [Header("회원가입 : UI OnOff 조절")]
    [SerializeField] GameObject Signup_pannel;
    [SerializeField] InputField id_Signup;
    [SerializeField] InputField pwd_Signup;
    [SerializeField] private Text log_signup;   //회원 가입시 경고 안내 문구

    #region 로그인
    public void LoginBtn()
    {
        if(id_i.text.Equals(string.Empty) || pwd_i.text.Equals(string.Empty))
        {
            log.text = "아이디와 비밀번호를 입력하세요";
            return;
        }

        if (SQLManager.instance.Login(id_i.text, pwd_i.text))
        {
            //로그인 성공
            User_info info = SQLManager.instance.info;
            Debug.Log($"{info.User_name}님 안녕하세요");
            gameObject.SetActive(false);
            logining.loginingPannel.SetActive(true);

        }

        else
        {
            log.text = "아이디와 비밀번호를 확인해주세요";
        }
    }
    #endregion

    #region 회원가입
    // 회원가입창 버튼
    public void OpenSignupPannel()
    {
        Signup_pannel.gameObject.SetActive(true);
    }

    public void CloseSignupPannel()
    {
        Signup_pannel.gameObject.SetActive(false);
    }

    public void SignUpBtn()
    {
        if (id_Signup.text.Equals(string.Empty) || pwd_Signup.text.Equals(string.Empty))
        {
            log_signup.text = "아이디와 비밀번호를 입력하세요";
            return;
        }

        if (SQLManager.instance.Signup(id_Signup.text, pwd_Signup.text))
        {
            // 회원가입 성공
            Debug.Log($"{id_Signup.text} 가입 성공");
            Signup_pannel.gameObject.SetActive(false);

            //temp
            id_Signup.text = string.Empty;
            pwd_Signup.text = string.Empty;
            log_signup.text = string.Empty;
        }

        else
        {
            Debug.Log("다시 확인하세요");
        }
    }
    #endregion
}
