using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
    [Header("로그인 후 회원정보 수정, 탈퇴 창")]
    [SerializeField] private LoginingController logining;

    // 페이드 인아웃
    [SerializeField] private UIFade fade;

    [Header("로그인 : 아이디 입력해서 게임 입장")]
    public GameObject LoginPannel;    //로그인 패널    
    public InputField id_i;
    public InputField pwd_i;
    public Text log;  // 로그인 경고 안내 문구

    [Header("로그인 완료 팝업창")]
    [SerializeField] private GameObject NoticeLogin_pannel;  // 로그인 완료 알림 패널창
    [SerializeField] private Text NoticeLoginlog;   // 로그인 완료 알림문구

    [Header("로그인 시 닉네임 입력 팝업창")]
    [SerializeField] GameObject nicknameLogin_pannel;   // 로그인 하면서 닉네임 만들기 패널
    [SerializeField] InputField nicknameLogin_Signup;    // 로그인 때 닉네임 입력 필드
    [SerializeField] private Text nicknameLogin_log;   //닉네임 중복 시 경고 안내 문구


    [Header("회원가입 1단계: 아이디, 비밀번호 입력 + UI OnOff 조절")]
    [SerializeField] GameObject Signup_pannel;  // 1단계 패널
    [SerializeField] InputField id_Signup;
    [SerializeField] InputField pwd_Signup;
    [SerializeField] private Text log1_signup;   //회원가입시(아이디 중복 시) 경고 안내 문구

    [Header("회원가입 2단계: 닉네임 입력 + UI OnOff 조절")]
    private string cached_id;   //1단계 저장한 아이디
    [SerializeField] GameObject Nickname_pannel;    // 2단계 닉네임 만들기 패널
    [SerializeField] InputField nickname_Signup;    // 2단계에서 사용할 닉네임 입력 필드
    [SerializeField] private Text log2_signup;   //회원 가입시(닉네임 중복 시) 경고 안내 문구

    [Header("회원가입 완료 팝업창")]
    [SerializeField] private GameObject NoticeSignup_pannel;  // 회원가입 알림 패널창
    [SerializeField] private Text NoticeSignuplog;   // 회원가입 알림문구

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
            User_info info = SQLManager.instance.info;
            cached_id = id_i.text;

            string sqlname = SQLManager.instance.info.User_Nickname;
            // 만약 로그인 시 nickname이 null이라면
            if (string.IsNullOrEmpty(sqlname) /*|| sqlname.ToLower() == "null"*/)
            {
                LoginPannel.SetActive(false);   // 로그인 창 없어지고
                //닉네임 입력 패널을 보여준다.
                nicknameLogin_pannel.SetActive(true);

                // 닉네임 입력창 초기화
                nicknameLogin_Signup.text = string.Empty;
                nicknameLogin_log.text = string.Empty;

                return;
            }

            //로그인 성공 후
            NoticeLoginlog.text = $"<color=yellow>{info.User_Nickname}</color>님 안녕하세요";
            LoginPannel.SetActive(false);

            // 페이드인 등장
            fade.FadeIn(NoticeLogin_pannel);
            //NoticeLogin_pannel.SetActive(true);

            //시간 지나면 페이드 아웃
            StartCoroutine(fade.AutoFade(NoticeLogin_pannel, 1.5f));


            logining.loginingPannel.SetActive(true);    //로그아웃, 회원정보 변경, 회원탈퇴 패널창 나타남
            logining.userNickname.text = SQLManager.instance.info.User_Nickname;

        }
        else
        {
            log.text = "아이디와 비밀번호를 확인해주세요";
        }
    }
    #endregion

    #region 닉네임 입력하고 로그인
    public void NicknameLoginBtn()
    {
        if (nicknameLogin_Signup.text.Equals(string.Empty))
        {
            nicknameLogin_log.text = "닉네임을 입력하세요";
            return;
        }

        if(SQLManager.instance.CompleteLoginwithName(cached_id, nicknameLogin_Signup.text))
        {
            // 바로 로그인 생성
            User_info info = SQLManager.instance.info;
            NoticeLoginlog.text = $"<color=yellow>{info.User_Nickname}</color>님 안녕하세요";

            nicknameLogin_pannel.SetActive(false);  //로그인 닉네임 패널 비활성화

            // 페이드인 등장
            fade.FadeIn(NoticeLogin_pannel);
            //NoticeLogin_pannel.SetActive(true);

            //시간 지나면 페이드 아웃
            StartCoroutine(fade.AutoFade(NoticeLogin_pannel, 1.5f));


            logining.loginingPannel.SetActive(true);    //로그아웃, 회원정보 변경, 회원탈퇴 패널창 나타남
            logining.userNickname.text = SQLManager.instance.info.User_Nickname;

        }
        else
        {
            nicknameLogin_log.text = "이미 사용 중인 닉네임입니다.";
        }
    }
    #endregion

    #region 회원가입
    // 회원가입창 열기
    public void OpenSignupPannel()
    {
        Signup_pannel.SetActive(true);

        //입력창 초기화
        id_Signup.text = string.Empty;
        pwd_Signup.text = string.Empty;
        log1_signup.text = string.Empty;
    }

    // 회원가입창 닫기
    public void CloseSignupPannel()
    {
        Signup_pannel.SetActive(false);
    }

    // 1단계 아이디, 비밀 번호 입력 -> 중복이 안되면 2단계로 가는 버튼
    public void CheckIDAndOPenNicknamePannelBtn()
    {
        if (id_Signup.text.Equals(string.Empty) || pwd_Signup.text.Equals(string.Empty))
        {
            log1_signup.text = "아이디와 비밀번호를 입력하세요";
            return;
        }

        if (SQLManager.instance.SignupStep1(id_Signup.text, pwd_Signup.text))
        {
            // 2단계에 입력할 아이디 저장 
            cached_id = id_Signup.text;

            // 1단계 성공 + 등록
            Signup_pannel.SetActive(false);
            Nickname_pannel.SetActive(true);

            // 닉네임 입력창 초기화
            nickname_Signup.text = string.Empty;
            log2_signup.text = string.Empty;
        }

        else
        {
            log1_signup.text = "이미 존재하는 아이디입니다.";
        }
    }

    // 2단계 닉네임 입력 -> 중복이 안되면 회원 가입 완료
    public void CompleteSignup()
    {
        if (nickname_Signup.text.Equals(string.Empty))
        {
            log2_signup.text = "닉네임을 입력하세요";
            return;
        }

        if(SQLManager.instance.SignupStep2(nickname_Signup.text, cached_id))
        {
            // 2단계 성공 + 회원가입 완료
            NoticeSignuplog.text = $"<color=yellow>{nickname_Signup.text}</color>님 가입 완료";
            Nickname_pannel.SetActive(false);

            // 페이드인 등장
            fade.FadeIn(NoticeSignup_pannel);
            //NoticeSignup_pannel.SetActive(true);

            //시간 지나면 페이드 아웃
            StartCoroutine(fade.AutoFade(NoticeSignup_pannel, 1.5f));

            //Invoke("CloseNoticePannel", 1.5f);
        }

        else
        {
            log2_signup.text = "이미 존재하는 닉네임입니다.";
        }
    }
    #endregion

    // 닉네임 패널 닫기
    public void CloseNicknamePannel()
    {
        Nickname_pannel.SetActive(false);
    }

    // 로그인 시 닉네임 패널 닫음
    public void CloseNicknameLoginPannel()
    {
        nicknameLogin_pannel.SetActive(false);
        LoginPannel.SetActive(true);

        //입력창 초기화
        logining.EmptyLoginField();
    }
}
