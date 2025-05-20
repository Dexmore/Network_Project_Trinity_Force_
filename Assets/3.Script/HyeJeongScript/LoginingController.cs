using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginingController : MonoBehaviour
{
    [Header("로그아웃 하기 위해 LoginController를 가져옴")]
    [SerializeField] private LoginController login;

    [Header("로그인 후 회원정보 패널")]
    public GameObject loginingPannel;

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
        login.gameObject.SetActive(true);

        //temp
        EmptyLoginField();
    }

    #region 회원정보 수정
    public void OpenUpdatePannel()
    {
        Update_pannel.gameObject.SetActive(true);
    }

    public void CloseUpdatePannel()
    {
        Update_pannel.gameObject.SetActive(false);
    }

    public void OpennicknamePannel()
    {
        Update_pannel.gameObject.SetActive(false);
        Updatename_pannel.gameObject.SetActive(true);

        //입력창 초기화
        name_new.text = string.Empty;
        updatenamelog.text = string.Empty;
    }

    public void OpenpasswordPannel()
    {
        Update_pannel.gameObject.SetActive(false);
        Updatepwd_pannel.gameObject.SetActive(true);

        //입력창 초기화
        pwd_new.text = string.Empty;
        updatepwdlog.text = string.Empty;
    }
    public void ClosenicknamePannel()
    {
        Updatename_pannel.gameObject.SetActive(false);
    }

    public void ClosepasswordPannel()
    {
        Updatepwd_pannel.gameObject.SetActive(false);
    }

    public void CloseNoticeNicknamePannel()
    {
        NoticeNickname_pannel.gameObject.SetActive(false);
    }

    public void CloseNoticePWDPannel()
    {
        NoticePWD_pannel.gameObject.SetActive(false);
    }

    public void CloseNoticeDeletePannel()
    {
        NoticeDelete_pannel.gameObject.SetActive(false);
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
        }

        if (SQLManager.instance.UpdateNicknameinfo(id, currentname, newname))
        {
            Updatename_pannel.gameObject.SetActive(false);
            NoticeNickname_pannel.gameObject.SetActive(true);
            NoticeNicknamelog.text = $"닉네임이 {newname}(으)로 변경되었습니다.";

            // 3초 후 사라짐
            Invoke("CloseNoticeNicknamePannel", 3f);
        }

        else
        {
            updatenamelog.text = "중복된 닉네임입니다.";
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
        }

        if(SQLManager.instance.Updatepasswordinfo(id, newpwd))
        {
            NoticePWD_pannel.gameObject.SetActive(true);
            NoticePWDlog.text = "비밀번호가 변경되었습니다.";
            Updatepwd_pannel.gameObject.SetActive(false);

            // 3초 후 사라짐
            Invoke("CloseNoticePWDPannel", 3f);
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
        Delete_Pannel.gameObject.SetActive(true);
    }

    public void CloseDeletePannel()
    {
        Delete_Pannel.gameObject.SetActive(false);
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
            NoticeDelete_pannel.gameObject.SetActive(true);
            Deletelog.text = "회원이 탈퇴되었습니다. 그동안 이용해주셔서 감사합니다.";

            loginingPannel.SetActive(false);
            Delete_Pannel.SetActive(false);

            login.gameObject.SetActive(true);
            EmptyLoginField();

            //3초 후 사라짐
            Invoke("CloseNoticeDeletePannel", 3f);
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
}
