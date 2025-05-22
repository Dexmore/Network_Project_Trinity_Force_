using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginingController : MonoBehaviour
{
    //페이드 아웃
    [SerializeField] private UIFade fade;

    [Header("로그아웃 하기 위해 LoginController를 가져옴")]
    [SerializeField] private LoginController login;

    [Header("로그인 후 회원정보 패널")]
    public GameObject userPannel;   // 유저 닉네임 패널창
    public Text userNickname;   // 유저 닉네임
    public GameObject loginingPannel;   // 로그아웃, 회원정보수정, 회원탈퇴 패널창

    [Header("회원정보 변경창")]
    [SerializeField] private GameObject Update_pannel;  // 회원정보 수정 선택 패널 -> 닉네임, 비밀번호 중 1개 선택

    [Header("회원정보 변경창 - 닉네임")]
    [SerializeField] private GameObject Updatename_pannel;  // 닉네임 정보 수정 패널
    public InputField name_new;
    [SerializeField] private Text updatenamelog;   // 닉네임 변경 경고 문구

    [Header("닉네임 변경 후 팝업창")]
    [SerializeField] private GameObject NoticeNickname_pannel;  // 닉네임 변경 패널창
    [SerializeField] private Text NoticeNicknamelog;    // 닉네임 변경 알림

    [Header("회원정보 변경창 - 비밀번호")]
    [SerializeField] private GameObject Updatepwd_pannel;  // 비밀번호 정보 수정 패널
    public InputField pwd_new;
    [SerializeField] private Text updatepwdlog;   // 비밀번호 변경 경고 문구

    [Header("비밀번호 변경후 팝업창")]
    [SerializeField] private GameObject NoticePWD_pannel;  // 비밀번호 변경 패널창
    [SerializeField] private Text NoticePWDlog;    // 비밀번호 변경 알림

    [Header("회원탈퇴")]
    public GameObject Delete_Pannel;    // 회원탈퇴 패널창

    [Header("회원탈퇴 알림창")]
    [SerializeField] private GameObject NoticeDelete_pannel;  // 회원탈퇴 알림 패널창
    [SerializeField] private Text Deletelog;   // 회원탈퇴 알림문구

    //로그아웃
    public void LogoutBtn()
    {
        loginingPannel.SetActive(false);
        login.LoginPannel.SetActive(true);

        //temp
        EmptyLoginField();
    }

    #region 회원정보 수정
    public void OpenUpdatePannel()
    {
        Update_pannel.SetActive(true);
    }

    public void CloseUpdatePannel()
    {
        Update_pannel.SetActive(false);
    }

    public void OpennicknamePannel()
    {
        Update_pannel.SetActive(false);
        Updatename_pannel.SetActive(true);

        //입력창 초기화
        name_new.text = string.Empty;
        updatenamelog.text = string.Empty;
    }

    public void OpenpasswordPannel()
    {
        Update_pannel.SetActive(false);
        Updatepwd_pannel.SetActive(true);

        //입력창 초기화
        pwd_new.text = string.Empty;
        updatepwdlog.text = string.Empty;
    }
    public void ClosenicknamePannel()
    {
        Updatename_pannel.SetActive(false);
    }

    public void ClosepasswordPannel()
    {
        Updatepwd_pannel.SetActive(false);
    }

    public void CloseNoticeNicknamePannel()
    {
        NoticeNickname_pannel.SetActive(false);
    }

    public void CloseNoticePWDPannel()
    {
        NoticePWD_pannel.SetActive(false);
    }

    public void CloseNoticeDeletePannel()
    {
        NoticeDelete_pannel.SetActive(false);
    }

    public void UpdateNicknameBtn()
    {
        if (SQLManager.instance.info == null)   
        {
            return;
        }

        string id = SQLManager.instance.info?.User_name;
        string currentname = SQLManager.instance.info?.User_Nickname;
        string newname = name_new.text.Trim();
        //Trim() : 공백제거

        if(name_new.text.Equals(string.Empty))
        {
            updatenamelog.text = "변경할 닉네임을 입력하세요";
            return;
        }

        if (SQLManager.instance.UpdateNicknameinfo(id, currentname, newname))
        {
            // 닉네임 변경 알림창 생성
            Updatename_pannel.SetActive(false);
            NoticeNicknamelog.text = $"닉네임이 <color=yellow>{newname}</color>(으)로 변경되었습니다.";
            userNickname.text = newname;

            // 페이드인 등장
            fade.FadeIn(NoticeNickname_pannel);
            //NoticeNickname_pannel.SetActive(true);
            
            //시간 지나면 페이드 아웃
            StartCoroutine(AutoFade(NoticeNickname_pannel, 1.5f));

            // 1.5초 후 사라짐
            //Invoke("CloseNoticeNicknamePannel", 1.5f);
        }

        else
        {
            updatenamelog.text = "이미 존재하는 닉네임입니다.";
        }
    }

    public void UpdatePasswordBtn()
    {
        if (SQLManager.instance.info == null)
        {
            return;
        }

        string id = SQLManager.instance.info?.User_name;
        string newpwd = pwd_new.text.Trim();

        if(pwd_new.text.Equals(string.Empty))
        {
            updatepwdlog.text = "변경할 비밀번호를 입력하세요";
            return;
        }

        if(SQLManager.instance.Updatepasswordinfo(id, newpwd))
        {
            // 비밀번호 변경 알림창 생성
            NoticePWDlog.text = "비밀번호가 변경되었습니다.";
            Updatepwd_pannel.SetActive(false);

            // 페이드인 등장
            fade.FadeIn(NoticePWD_pannel);
            //NoticePWD_pannel.SetActive(true);
            
            //시간 지나면 페이드 아웃
            StartCoroutine(AutoFade(NoticePWD_pannel, 1.5f));

            // 1.5초 후 사라짐
            //Invoke("CloseNoticePWDPannel", 1.5f);
        }

        else
        {
            updatepwdlog.text = "다시 입력하세요";
        }
    }
    #endregion

    #region 회원탈퇴
    public void OpenDeletePannel()
    {
        Delete_Pannel.SetActive(true);
    }

    public void CloseDeletePannel()
    {
        Delete_Pannel.SetActive(false);
    }

    public void DeleteBtn()
    {
        if(SQLManager.instance.info == null)
        {
            return;
        }

        string id = SQLManager.instance.info.User_name;
        string pwd = SQLManager.instance.info.User_Password;

        if(SQLManager.instance.Deleteinfo(id, pwd))
        {
            // 회원 탈퇴 알림창 생성
            Deletelog.text = "회원탈퇴가 완료되었습니다. 그동안 이용해주셔서 감사합니다.";

            // 페이드인 등장
            fade.FadeIn(NoticeDelete_pannel);
            //NoticeDelete_pannel.SetActive(true);

            //시간 지나면 페이드 아웃
            StartCoroutine(AutoFade(NoticeDelete_pannel, 1.5f));

            loginingPannel.SetActive(false);
            Delete_Pannel.SetActive(false);

            login.LoginPannel.SetActive(true);
            EmptyLoginField();

            //1.5초 후 사라짐
            //Invoke("CloseNoticeDeletePannel", 1.5f);
        }
    }
    #endregion

    // 로그인 입력창 초기화
    public void EmptyLoginField()
    {
        login.id_i.text = string.Empty;
        login.pwd_i.text = string.Empty;
        login.log.text = string.Empty;
    }

    // 페이드 자동 아웃
    private IEnumerator AutoFade(GameObject pannel, float duration)
    {
        yield return new WaitForSeconds(duration);
        fade.Fadeout(pannel);
    }
  
}
